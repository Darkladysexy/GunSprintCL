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

        [Tooltip("Empty space between two segments.")]
        [FormerlySerializedAs("_gap")]
        [SerializeField, Min(0f)] private float _floorGap = 5f;

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
        private System.Random _random;
        private GameSession _session;
        private float _nextFloorDistance;
        private float _restTimer;
        private bool _isRunning;

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

            _nextFloorDistance = headStart;
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
            for (int i = 0; i < _floorPrefabs.Length; i++)
            {
                _floorPools[i] = new PrefabPool<Transform>(_floorPrefabs[i], _floorRoot, 4, 16);
            }
        }

        private void BuildFloorsAhead(float gunDistance)
        {
            if (_floorPools.Length == 0)
            {
                return;
            }

            Quaternion rotation = Quaternion.Euler(_floorRotation);
            while (_nextFloorDistance < gunDistance + _buildAheadDistance)
            {
                int poolIndex = _random.Next(_floorPools.Length);
                Vector3 position = _session.Track.GetPoint(_nextFloorDistance);
                position.y = _floorHeight;

                Transform floor = _floorPools[poolIndex].Get(position, rotation);
                _activeFloors.Enqueue(new Floor(floor, poolIndex, _nextFloorDistance));
                _nextFloorDistance += _floorLength + _floorGap;
            }
        }

        private void RecycleFloorsBehind(float gunDistance)
        {
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
