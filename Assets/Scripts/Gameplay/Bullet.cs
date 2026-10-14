using System;
using GunSprint.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace GunSprint.Gameplay
{
    /// <summary>
    /// Pooled projectile. Flies straight, hits the first <see cref="IHittable"/> it touches and
    /// reports back to its owner, who returns it to the pool.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private TrailRenderer _trail;

        [Tooltip("Seconds before a bullet that hit nothing is recycled.")]
        [FormerlySerializedAs("_lifeTime")]
        [SerializeField, Min(0.1f)] private float _lifetime = 2f;

        private Rigidbody _rigidbody;
        private Collider _collider;
        private Action<Bullet> _finished;
        private Vector3 _direction;
        private float _expireTime;
        private bool _isFlying;

        /// <summary>True when the bullet stopped because it hit something (not because it expired).</summary>
        public bool HasImpacted { get; private set; }

        public Vector3 ImpactPoint { get; private set; }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            TrailEffects.DisableScreenDistortion(gameObject);
        }

        public void Launch(Vector3 velocity, Collider ownerCollider, Action<Bullet> finished)
        {
            // Ignore pairs are dropped whenever a collider is deactivated, so re-apply on every launch.
            if (_collider != null && ownerCollider != null)
            {
                Physics.IgnoreCollision(_collider, ownerCollider);
            }

            _finished = finished;
            _direction = velocity.normalized;
            _expireTime = Time.time + _lifetime;
            _isFlying = true;
            HasImpacted = false;

            _rigidbody.linearVelocity = velocity;
            _rigidbody.angularVelocity = Vector3.zero;

            if (_trail != null)
            {
                _trail.Clear();
            }
        }

        private void Update()
        {
            if (_isFlying && Time.time >= _expireTime)
            {
                Finish();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!_isFlying)
            {
                return;
            }

            ImpactPoint = collision.contactCount > 0 ? collision.GetContact(0).point : transform.position;
            HasImpacted = true;

            if (collision.collider.TryGetComponent(out IHittable hittable))
            {
                hittable.TakeHit(ImpactPoint, _direction);
            }

            Finish();
        }

        private void Finish()
        {
            _isFlying = false;
            _finished?.Invoke(this);
        }
    }
}
