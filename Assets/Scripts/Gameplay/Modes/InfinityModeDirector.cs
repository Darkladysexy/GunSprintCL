using System.Collections.Generic;
using GunSprint.Audio;
using GunSprint.Core;
using GunSprint.Data;
using GunSprint.Enemies;
using GunSprint.Events;
using GunSprint.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace GunSprint.Gameplay.Modes
{
    /// <summary>
    /// Endless mode: streams pooled floor segments ahead of the gun, keeps enemies spawning, refills
    /// ammo on kills and finishes the run once the gun is out of ammo and at rest, or falls.
    /// </summary>
    public class InfinityModeDirector : GameModeDirector
    {
        [SerializeField] private EnemySpawner _spawner;

        [Header("Floor streaming")]
        [SerializeField] private Transform[] _floorPrefabs = new Transform[0];

        [SerializeField] private Transform _floorRoot;

        [Tooltip("Length of one floor segment along the track.")]
        [FormerlySerializedAs("_actualFloorSize")]
        [SerializeField, Min(1f)] private float _floorLength = 20f;

        [Tooltip("Smallest empty space between two segments.")]
        [FormerlySerializedAs("_gap")]
        [SerializeField, Min(0f)] private float _floorGap = 5f;

        [Tooltip("Largest gap at the start of a run; it grows to the hard gap over the difficulty distance.")]
        [SerializeField, Min(0f)] private float _maxGapEasy = 5f;

        [SerializeField, Min(0f)] private float _maxGapHard = 9f;

        [Tooltip("Meters over which the gaps and height changes reach full difficulty.")]
        [SerializeField, Min(1f)] private float _difficultyDistance = 600f;

        [Tooltip("Random height change of FloorSegment pieces at full difficulty, in meters.")]
        [SerializeField, Min(0f)] private float _heightVariation = 1.2f;

        [SerializeField] private float _floorHeight = -10f;
        [SerializeField] private Vector3 _floorRotation = new Vector3(0f, 90f, 0f);

        [Tooltip("Floors are built until this far ahead of the gun.")]
        [FormerlySerializedAs("_spawnAheadDistance")]
        [SerializeField, Min(10f)] private float _buildAheadDistance = 40f;

        [Tooltip("Floors further behind the gun than this are recycled.")]
        [SerializeField, Min(10f)] private float _recycleBehindDistance = 40f;

        [Header("Gun")]
        [SerializeField] private float _gunStartHeight = 1f;
        [SerializeField] private Vector3 _gunStartRotation = new Vector3(0f, -90f, 0f);

        [Header("Ammo")]
        [SerializeField] private UpgradeSO _ammoUpgrade;
        [SerializeField] private UpgradeSO _headStartUpgrade;
        [SerializeField, Min(0)] private int _ammoPerKill = 2;
        [SerializeField] private IntEventChannelSO _enemyKilled;
        [SerializeField] private AudioCueSO _reloadCue;

        [Header("End of run")]
        [Tooltip("Seconds the gun must rest without ammo before the run ends.")]
        [SerializeField, Min(0f)] private float _restTimeToFinish = 0.75f;

        [Tooltip("Safety net: the run ends this long after the last shot even if the gun still wobbles.")]
        [SerializeField, Min(1f)] private float _outOfAmmoTimeout = 5f;

        [SerializeField, Min(0f)] private float _firstEnemyDistance = 10f;

        private readonly Queue<Floor> _activeFloors = new Queue<Floor>();
        private PrefabPool<Transform>[] _floorPools;
        private FloorSegment[] _floorSegments;
        private System.Random _random;
        private GameSession _session;
        private float _nextFloorDistance;
        private float _restTimer;
        private bool _isRunning;
        private bool _isFirstFloor;

        public override GameMode Mode => GameMode.Infinity;

        private void OnEnable()
        {
            _enemyKilled.OnEventRaised += HandleEnemyKilled;
        }

        private void OnDisable()
        {
            _enemyKilled.OnEventRaised -= HandleEnemyKilled;
        }

        public override void BeginRun(GameSession session)
        {
            _session = session;
            _random = new System.Random();
            CreateFloorPools();

            // Head start moves the gun forward; the first floor is always centred under it.
            float headStart = _headStartUpgrade.GetValue(session.Progress.GetUpgradeLevel(_headStartUpgrade));
            Vector3 start = session.Track.GetPoint(headStart);
            start.y = _gunStartHeight;
            session.Gun.PlaceAt(start, Quaternion.Euler(_gunStartRotation));

            // The first piece is a plain floor centred under the gun, so the run never starts over a gap.
            _nextFloorDistance = headStart - _floorLength * 0.5f;
            _isFirstFloor = true;
            BuildFloorsAhead(headStart);

            int ammo = Mathf.RoundToInt(_ammoUpgrade.GetValue(session.Progress.GetUpgradeLevel(_ammoUpgrade)));
            session.RunState.SetAmmo(ammo);

            _spawner.SetTarget(session.Gun.transform);
            _spawner.StartAutoSpawn(headStart + _firstEnemyDistance);
            _isRunning = true;
        }

        public override void HandleGunFell()
        {
            _session.FinishRun();
        }

        public override void EndRun()
        {
            _isRunning = false;
            _spawner.StopAutoSpawn();
        }

        private void Update()
        {
            if (!_isRunning || !_session.RunState.IsPlaying)
            {
                return;
            }

            float gunDistance = _session.Track.GetDistance(_session.Gun.transform.position);
            BuildFloorsAhead(gunDistance);
            RecycleFloorsBehind(gunDistance);
            CheckOutOfAmmo();
        }

        private void CheckOutOfAmmo()
        {
            if (_session.RunState.Ammo > 0)
            {
                _restTimer = 0f;
                return;
            }

            _restTimer = _session.Gun.IsResting ? _restTimer + Time.deltaTime : 0f;
            bool hasTimedOut = Time.time - _session.Gun.LastShotTime > _outOfAmmoTimeout;
            if (_restTimer >= _restTimeToFinish || hasTimedOut)
            {
                _session.FinishRun();
            }
        }

        private void HandleEnemyKilled(int coins)
        {
            if (!_isRunning || _ammoPerKill == 0)
            {
                return;
            }

            _session.RunState.AddAmmo(_ammoPerKill);
            if (_reloadCue != null)
            {
                _reloadCue.Play();
            }
        }

        private void CreateFloorPools()
        {
            _floorPools = new PrefabPool<Transform>[_floorPrefabs.Length];
            _floorSegments = new FloorSegment[_floorPrefabs.Length];
            for (int i = 0; i < _floorPrefabs.Length; i++)
            {
                _floorPools[i] = new PrefabPool<Transform>(_floorPrefabs[i], _floorRoot, 4, 16);
                _floorSegments[i] = _floorPrefabs[i].GetComponent<FloorSegment>();
            }
        }

        private void BuildFloorsAhead(float gunDistance)
        {
            if (_floorPools.Length == 0)
            {
                return;
            }

            while (_nextFloorDistance < gunDistance + _buildAheadDistance)
            {
                float difficulty = Mathf.Clamp01(gunDistance / _difficultyDistance);
                int poolIndex = PickFloor();
                FloorSegment segment = _floorSegments[poolIndex];

                // FloorSegment pieces know their length and face along the track; legacy floors
                // are centred on their slot with the old fixed rotation.
                float length = segment != null ? segment.Length : _floorLength;
                Quaternion rotation = segment != null
                    ? Quaternion.LookRotation(_session.Track.Forward)
                    : Quaternion.Euler(_floorRotation);
                float height = _floorHeight;
                if (segment != null && !_isFirstFloor)
                {
                    height += RandomRange(-0.5f, 1f) * _heightVariation * difficulty;
                }

                float center = _nextFloorDistance + length * 0.5f;
                Vector3 position = _session.Track.GetPoint(center);
                position.y = height;

                Transform floor = _floorPools[poolIndex].Get(position, rotation);
                _activeFloors.Enqueue(new Floor(floor, poolIndex, center + length * 0.5f));

                float maxGap = Mathf.Lerp(_maxGapEasy, _maxGapHard, difficulty);
                _nextFloorDistance += length + RandomRange(_floorGap, Mathf.Max(_floorGap, maxGap));
                _isFirstFloor = false;
            }
        }

        private int PickFloor()
        {
            if (!_isFirstFloor)
            {
                return _random.Next(_floorPools.Length);
            }

            // Prefer a flat legacy floor (no FloorSegment) or the first entry as the starting piece.
            for (int i = 0; i < _floorSegments.Length; i++)
            {
                if (_floorSegments[i] == null)
                {
                    return i;
                }
            }

            return 0;
        }

        private float RandomRange(float min, float max)
        {
            return min + (float)_random.NextDouble() * (max - min);
        }

        private void RecycleFloorsBehind(float gunDistance)
        {
            // Distance is the far end of each piece, so long pieces are never recycled under the gun.
            while (_activeFloors.Count > 0 && _activeFloors.Peek().Distance < gunDistance - _recycleBehindDistance)
            {
                Floor floor = _activeFloors.Dequeue();
                _floorPools[floor.PoolIndex].Release(floor.Instance);
            }
        }

        private readonly struct Floor
        {
            public readonly Transform Instance;
            public readonly int PoolIndex;
            public readonly float Distance;

            public Floor(Transform instance, int poolIndex, float distance)
            {
                Instance = instance;
                PoolIndex = poolIndex;
                Distance = distance;
            }
        }
    }
}
