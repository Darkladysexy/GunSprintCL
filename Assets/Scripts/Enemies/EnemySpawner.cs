using System;
using System.Collections.Generic;
using GunSprint.Gameplay;
using GunSprint.Utils;
using UnityEngine;

namespace GunSprint.Enemies
{
    /// <summary>
    /// Spawns pooled enemies on the ground along the track. Levels mode places enemies explicitly;
    /// Infinity mode keeps the space ahead of the gun populated and recycles enemies left behind.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private TrackPath _track;

        [Header("Enemies")]
        [SerializeField] private Enemy _basicPrefab;
        [SerializeField] private Enemy _swordPrefab;

        [SerializeField, Range(0f, 1f)] private float _swordChance = 0.35f;

        [Tooltip("Chance for an enemy without a sword to carry a shield (needs two hits).")]
        [SerializeField, Range(0f, 1f)] private float _shieldChance = 0.35f;

        [Tooltip("0 = spawn on the centre line. Above 0 = random offset to the left or right.")]
        [SerializeField, Min(0f)] private float _lateralRange;

        [Header("Ground")]
        [SerializeField] private LayerMask _groundMask;
        [SerializeField, Min(1f)] private float _groundProbeHeight = 100f;

        [Header("Infinity mode")]
        [SerializeField, Min(1)] private int _maxAlive = 24;
        [SerializeField, Min(0.5f)] private float _spawnSpacing = 5f;

        [Tooltip("Enemies are placed this far ahead of the gun, out of the camera's view.")]
        [SerializeField, Min(0f)] private float _spawnAheadDistance = 15f;

        [Tooltip("Enemies further behind the gun than this are recycled.")]
        [SerializeField, Min(5f)] private float _recycleBehindDistance = 25f;

        private readonly List<Enemy> _active = new List<Enemy>();
        private readonly Dictionary<Enemy, PrefabPool<Enemy>> _poolByInstance = new Dictionary<Enemy, PrefabPool<Enemy>>();
        private readonly Dictionary<GameObject, EffectPool> _deathEffectPools = new Dictionary<GameObject, EffectPool>();
        private PrefabPool<Enemy> _basicPool;
        private PrefabPool<Enemy> _swordPool;
        private Action<Enemy> _despawnEnemy;
        private Action<Enemy> _enemyDied;
        private Transform _poolRoot;
        private Transform _target;
        private System.Random _autoSpawnRandom;
        private float _nextSpawnDistance;
        private bool _isAutoSpawning;

        /// <summary>Raised when an enemy is killed (not when it is recycled).</summary>
        public event Action<Enemy> EnemyDied;

        public int LivingCount { get; private set; }

        private void Awake()
        {
            _poolRoot = new GameObject("[Pool] Enemies").transform;
            _basicPool = new PrefabPool<Enemy>(_basicPrefab, _poolRoot, 8, 40);
            _swordPool = new PrefabPool<Enemy>(_swordPrefab, _poolRoot, 8, 40);
            _despawnEnemy = Despawn;
            _enemyDied = HandleEnemyDied;
        }

        private void Update()
        {
            if (!_isAutoSpawning || _target == null)
            {
                return;
            }

            float followDistance = _track.GetDistance(_target.position);
            SpawnAhead(followDistance);
            RecycleBehind(followDistance);
        }

        /// <summary>Spawns one enemy on the ground at the given distance along the track, if there is ground.</summary>
        public Enemy SpawnAt(float distance, System.Random random)
        {
            // Floors may have been moved by the pool this frame; make sure raycasts see their new position.
            Physics.SyncTransforms();
            return SpawnOnGround(distance, random);
        }

        /// <summary>The gun: enemies face it, and auto spawning follows it.</summary>
        public void SetTarget(Transform target)
        {
            _target = target;
        }

        /// <summary>Keeps the track ahead of the target populated, starting at the given distance.</summary>
        public void StartAutoSpawn(float firstDistance)
        {
            _nextSpawnDistance = firstDistance;
            _autoSpawnRandom = new System.Random();
            _isAutoSpawning = true;
        }

        public void StopAutoSpawn()
        {
            _isAutoSpawning = false;
        }

        private void SpawnAhead(float followDistance)
        {
            if (_nextSpawnDistance > followDistance + _spawnAheadDistance)
            {
                return;
            }

            Physics.SyncTransforms();
            while (_nextSpawnDistance <= followDistance + _spawnAheadDistance && _active.Count < _maxAlive)
            {
                SpawnOnGround(_nextSpawnDistance, _autoSpawnRandom);
                _nextSpawnDistance += _spawnSpacing;
            }
        }

        private void RecycleBehind(float followDistance)
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                Enemy enemy = _active[i];
                if (_track.GetDistance(enemy.transform.position) < followDistance - _recycleBehindDistance)
                {
                    enemy.Despawn();
                }
            }
        }

        private Enemy SpawnOnGround(float distance, System.Random random)
        {
            float lateral = _lateralRange > 0f ? ((float)random.NextDouble() * 2f - 1f) * _lateralRange : 0f;
            Vector3 position = _track.GetPoint(distance) + _track.Right * lateral;

            Vector3 probeStart = position + Vector3.up * _groundProbeHeight;
            if (!Physics.Raycast(probeStart, Vector3.down, out RaycastHit hit, _groundProbeHeight * 2f, _groundMask, QueryTriggerInteraction.Ignore))
            {
                // Gaps between floor segments are expected: simply leave this slot empty.
                return null;
            }

            position.y = hit.point.y;

            bool hasSword = random.NextDouble() < _swordChance;
            bool hasShield = !hasSword && random.NextDouble() < _shieldChance;
            PrefabPool<Enemy> pool = hasSword ? _swordPool : _basicPool;

            Enemy enemy = pool.Get(position, Quaternion.LookRotation(-_track.Forward));
            if (!_poolByInstance.ContainsKey(enemy))
            {
                _poolByInstance.Add(enemy, pool);
                enemy.Died += _enemyDied;
            }

            enemy.Spawn(hasShield, _target, _despawnEnemy);
            _active.Add(enemy);
            LivingCount++;
            return enemy;
        }

        private void HandleEnemyDied(Enemy enemy)
        {
            LivingCount = Mathf.Max(0, LivingCount - 1);
            PlayDeathEffect(enemy);
            EnemyDied?.Invoke(enemy);
        }

        private void Despawn(Enemy enemy)
        {
            if (!_active.Remove(enemy))
            {
                return;
            }

            if (enemy.IsAlive)
            {
                // Recycled without being killed (left behind).
                LivingCount = Mathf.Max(0, LivingCount - 1);
            }

            _poolByInstance[enemy].Release(enemy);
        }

        private void PlayDeathEffect(Enemy enemy)
        {
            GameObject effect = enemy.Config.DeathEffect;
            if (effect == null)
            {
                return;
            }

            if (!_deathEffectPools.TryGetValue(effect, out EffectPool pool))
            {
                pool = new EffectPool(effect, _poolRoot, enemy.Config.DeathEffectLifetime);
                _deathEffectPools.Add(effect, pool);
            }

            pool.Spawn(enemy.transform.position + Vector3.up, Quaternion.identity);
        }
    }
}
