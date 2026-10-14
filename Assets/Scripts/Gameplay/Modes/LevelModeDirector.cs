using System.Collections;
using System.Collections.Generic;
using GunSprint.Core;
using GunSprint.Data;
using GunSprint.Enemies;
using GunSprint.Events;
using UnityEngine;

namespace GunSprint.Gameplay.Modes
{
    /// <summary>
    /// Levels mode: builds a hand-made level, spawns its enemies, reveals the multiplier targets once
    /// every enemy is dead and ends the level when a target is hit (win) or the gun falls (lose).
    /// </summary>
    public class LevelModeDirector : GameModeDirector
    {
        [Tooltip("Played in order; after the last one, levels repeat in a fixed pseudo-random order.")]
        [SerializeField] private LevelConfigSO[] _levels = new LevelConfigSO[0];

        [SerializeField] private EnemySpawner _spawner;
        [SerializeField] private Transform _mapRoot;
        [SerializeField] private GameObject _finishTargets;
        [SerializeField] private IntEventChannelSO _targetHit;

        [Header("Placement")]
        [SerializeField] private Vector3 _gunStartPosition = new Vector3(2f, 1f, 0f);
        [SerializeField] private Vector3 _gunStartRotation = new Vector3(0f, -90f, 0f);
        [SerializeField] private float _finishTargetsHeight = 1f;

        [Header("Segments")]
        [Tooltip("Height of the segment pivots (their ground tops sit about 9.4 m higher).")]
        [SerializeField] private float _segmentHeight = -10f;

        [Tooltip("The first segment starts this far behind the gun, so the gun starts on solid ground.")]
        [SerializeField, Min(0f)] private float _segmentStartBack = 5f;

        [Tooltip("The finish targets stand this far before the end of the last segment.")]
        [SerializeField, Min(0f)] private float _finishInset = 4f;

        // Per segment with enemies: the stretch of track they stand on and how many.
        private readonly List<Vector2> _enemyRanges = new List<Vector2>();
        private readonly List<int> _enemySlots = new List<int>();
        private GameSession _session;
        private bool _areEnemiesSpawned;
        private bool _isFinishRevealed;

        public override GameMode Mode => GameMode.Levels;

        private void OnEnable()
        {
            _targetHit.OnEventRaised += HandleTargetHit;
            _spawner.EnemyDied += HandleEnemyDied;
        }

        private void OnDisable()
        {
            _targetHit.OnEventRaised -= HandleTargetHit;
            _spawner.EnemyDied -= HandleEnemyDied;
        }

        public override void BeginRun(GameSession session)
        {
            _session = session;

            // Seeding with the level index makes a retried level identical to the failed attempt.
            int levelIndex = session.Progress.LevelIndex;
            var random = new System.Random(levelIndex);
            LevelConfigSO level = PickLevel(levelIndex, random);

            session.Gun.PlaceAt(_gunStartPosition, Quaternion.Euler(_gunStartRotation));
            _spawner.SetTarget(session.Gun.transform);
            _finishTargets.SetActive(false);

            if (level == null)
            {
                Debug.LogError("[LevelModeDirector] No level configured.", this);
                return;
            }

            if (level.UsesSegments)
            {
                float finishDistance = BuildSegments(level, random);
                StartCoroutine(SpawnEnemiesOnSegments(level, random, finishDistance));
                return;
            }

            GameObject map = level.PickMap(random);
            if (map != null)
            {
                Instantiate(map, _mapRoot);
            }

            StartCoroutine(SpawnEnemies(level, random));
        }

        public override void HandleGunFell()
        {
            _session.FailLevel();
        }

        private LevelConfigSO PickLevel(int levelIndex, System.Random random)
        {
            if (_levels.Length == 0)
            {
                return null;
            }

            return levelIndex < _levels.Length ? _levels[levelIndex] : _levels[random.Next(_levels.Length)];
        }

        /// <summary>Chains the level's segments along the track. Returns the finish distance.</summary>
        private float BuildSegments(LevelConfigSO level, System.Random random)
        {
            TrackPath track = _session.Track;
            Quaternion rotation = Quaternion.LookRotation(track.Forward);
            float start = track.GetDistance(_gunStartPosition) - _segmentStartBack;
            float end = start;
            _enemyRanges.Clear();
            _enemySlots.Clear();

            for (int i = 0; i < level.Segments.Length; i++)
            {
                GameObject prefab = level.Segments[i];
                if (prefab == null)
                {
                    continue;
                }

                FloorSegment segment = prefab.GetComponent<FloorSegment>();
                float length = segment != null ? segment.Length : 20f;
                Vector3 position = track.GetPoint(start + length * 0.5f);
                position.y = _segmentHeight;
                Instantiate(prefab, position, rotation, _mapRoot);

                // The first segment is the safe landing area; the others get enemies.
                if (i > 0 && segment != null)
                {
                    int count = Mathf.RoundToInt(segment.EnemySlots * level.EnemyDensity);
                    if (count > 0)
                    {
                        _enemyRanges.Add(new Vector2(start + 1.5f, start + length - 1.5f));
                        _enemySlots.Add(count);
                    }
                }

                end = start + length;
                start = end + level.SegmentGap;
            }

            return end - _finishInset;
        }

        private IEnumerator SpawnEnemiesOnSegments(LevelConfigSO level, System.Random random, float finishDistance)
        {
            yield return new WaitForFixedUpdate();

            for (int i = 0; i < _enemyRanges.Count; i++)
            {
                Vector2 range = _enemyRanges[i];
                int count = _enemySlots[i];
                for (int k = 0; k < count; k++)
                {
                    float t = (k + 1f) / (count + 1f);
                    _spawner.SpawnAt(Mathf.Lerp(range.x, range.y, t), random, level.SwordChance, level.ShieldChance);
                }
            }

            PlaceFinishTargets(finishDistance);
            _areEnemiesSpawned = true;
            UpdateEnemiesRemaining();
        }

        private IEnumerator SpawnEnemies(LevelConfigSO level, System.Random random)
        {
            // Wait one physics step so the freshly spawned map colliders can be hit by ground raycasts.
            yield return new WaitForFixedUpdate();

            float distance = level.FirstEnemyDistance;
            for (int i = 0; i < level.EnemyCount; i++)
            {
                _spawner.SpawnAt(distance, random);
                distance += level.EnemySpacing;
            }

            PlaceFinishTargets(level.FinishDistance);
            _areEnemiesSpawned = true;
            UpdateEnemiesRemaining();
        }

        private void PlaceFinishTargets(float distance)
        {
            Vector3 position = _session.Track.GetPoint(distance);
            float ground = _spawner.TryGetGroundHeight(distance, out float height) ? height : 0f;
            position.y = ground + _finishTargetsHeight;
            _finishTargets.transform.position = position;
        }

        private void HandleEnemyDied(Enemy enemy)
        {
            if (_areEnemiesSpawned)
            {
                UpdateEnemiesRemaining();
            }
        }

        private void UpdateEnemiesRemaining()
        {
            int remaining = _spawner.LivingCount;
            _session.RunState.SetEnemiesRemaining(remaining);

            if (remaining == 0 && !_isFinishRevealed)
            {
                _isFinishRevealed = true;
                _finishTargets.SetActive(true);
            }
        }

        private void HandleTargetHit(int multiplier)
        {
            _session.CompleteLevel(multiplier);
        }
    }
}
