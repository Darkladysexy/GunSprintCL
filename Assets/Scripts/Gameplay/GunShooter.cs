using System;
using System.Collections.Generic;
using GunSprint.Audio;
using GunSprint.Core;
using GunSprint.Data;
using GunSprint.Utils;
using UnityEngine;

namespace GunSprint.Gameplay
{
    /// <summary>
    /// Fires pooled bullets from the muzzle, plays muzzle and impact effects, and triggers slow motion
    /// while a bullet that is about to hit a target is in flight.
    /// </summary>
    [DisallowMultipleComponent]
    public class GunShooter : MonoBehaviour
    {
        private const float k_PredictionDistance = 60f;

        [SerializeField] private GunConfigSO _config;
        [SerializeField] private Bullet _bulletPrefab;
        [SerializeField] private Transform _muzzle;
        [SerializeField] private ParticleSystem _muzzleSmoke;

        [Tooltip("Layers that trigger slow motion when a shot is going to hit them (enemies, targets).")]
        [SerializeField] private LayerMask _targetLayers;

        [Tooltip("Layers that block the slow motion prediction (floor, walls).")]
        [SerializeField] private LayerMask _obstacleLayers;

        [Header("Effects")]
        [SerializeField] private GameObject _impactEffect;
        [SerializeField, Min(0.1f)] private float _impactEffectLifetime = 1f;
        [SerializeField] private AudioCueSO _shootCue;

        private readonly HashSet<Bullet> _slowMotionBullets = new HashSet<Bullet>();
        private PrefabPool<Bullet> _bulletPool;
        private EffectPool _impactPool;
        private Action<Bullet> _bulletFinished;
        private Collider _ownerCollider;
        private float _smokeStopTime;

        /// <summary>Bullet speed multiplier from the Power upgrade.</summary>
        public float SpeedMultiplier { get; set; } = 1f;

        public Vector3 MuzzleForward => _muzzle.forward;

        public Transform Muzzle => _muzzle;

        public ParticleSystem MuzzleSmoke => _muzzleSmoke;

        private void Awake()
        {
            _ownerCollider = GetComponent<Collider>();
            _bulletFinished = HandleBulletFinished;

            // Pooled objects live outside the gun so they do not inherit its movement.
            Transform poolRoot = new GameObject("[Pool] Bullets").transform;
            _bulletPool = new PrefabPool<Bullet>(_bulletPrefab, poolRoot, 8, 32);
            if (_impactEffect != null)
            {
                _impactPool = new EffectPool(_impactEffect, poolRoot, _impactEffectLifetime);
            }
        }

        private void OnDisable()
        {
            // Never leave the game stuck in slow motion if the gun goes away mid-flight.
            for (int i = 0; i < _slowMotionBullets.Count; i++)
            {
                TimeScaleService.EndSlowMotion();
            }

            _slowMotionBullets.Clear();
        }

        private void Update()
        {
            if (_muzzleSmoke != null && _muzzleSmoke.isPlaying && Time.time >= _smokeStopTime)
            {
                _muzzleSmoke.Stop();
            }
        }

        /// <summary>Switches to another gun's stats (see GunLoadout).</summary>
        public void SetConfig(GunConfigSO config)
        {
            _config = config;
        }

        public void Fire()
        {
            Vector3 direction = _muzzle.forward;
            Bullet bullet = _bulletPool.Get(_muzzle.position, _muzzle.rotation);
            bullet.Launch(direction * (_config.BulletSpeed * SpeedMultiplier), _ownerCollider, _bulletFinished);

            if (IsAimingAtTarget(direction))
            {
                _slowMotionBullets.Add(bullet);
                TimeScaleService.BeginSlowMotion(_config.SlowMotionScale);
            }

            PlayMuzzleSmoke();
            if (_shootCue != null)
            {
                _shootCue.Play();
            }
        }

        private bool IsAimingAtTarget(Vector3 direction)
        {
            int mask = _targetLayers | _obstacleLayers;
            if (!Physics.Raycast(_muzzle.position, direction, out RaycastHit hit, k_PredictionDistance, mask, QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            return (_targetLayers.value & (1 << hit.collider.gameObject.layer)) != 0;
        }

        private void PlayMuzzleSmoke()
        {
            if (_muzzleSmoke == null)
            {
                return;
            }

            _smokeStopTime = Time.time + _config.SmokeDuration;
            if (!_muzzleSmoke.isPlaying)
            {
                _muzzleSmoke.Play();
            }
        }

        private void HandleBulletFinished(Bullet bullet)
        {
            if (_slowMotionBullets.Remove(bullet))
            {
                TimeScaleService.EndSlowMotion();
            }

            if (bullet.HasImpacted && _impactPool != null)
            {
                _impactPool.Spawn(bullet.ImpactPoint, Quaternion.identity);
            }

            _bulletPool.Release(bullet);
        }
    }
}
