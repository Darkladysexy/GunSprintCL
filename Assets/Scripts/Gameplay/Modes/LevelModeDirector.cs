using System.Collections;
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
            position.y = _finishTargetsHeight;
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
