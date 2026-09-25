using System;
using System.Collections;
using GunSprint.Audio;
using GunSprint.Data;
using GunSprint.Events;
using GunSprint.Gameplay;
using UnityEngine;
using UnityEngine.Serialization;

namespace GunSprint.Enemies
{
    /// <summary>
    /// Pooled enemy. A shielded enemy needs two hits: the first one breaks the shield.
    /// On death it reports the coin reward through an event channel, falls over, shrinks away
    /// and hands itself back to the spawner's pool.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Enemy : MonoBehaviour, IHittable
    {
        private static readonly int s_idleState = Animator.StringToHash("Idle");
        private static readonly int s_attackTrigger = Animator.StringToHash("Attack");
        private static readonly int s_hitTrigger = Animator.StringToHash("Hit");
        private static readonly int s_shieldTrigger = Animator.StringToHash("Shield");
        private static readonly int s_dieTrigger = Animator.StringToHash("Die");

        [SerializeField] private EnemyConfigSO _config;
        [SerializeField] private IntEventChannelSO _enemyKilled;

        [FormerlySerializedAs("_anim")]
        [SerializeField] private Animator _animator;

        [FormerlySerializedAs("_rb")]
        [SerializeField] private Rigidbody _rigidbody;

        private Collider[] _colliders;
        private EnemyFacing _facing;
        private Transform _shieldIcon;
        private Transform _cameraTransform;
        private Vector3 _baseScale;
        private Action<Enemy> _despawn;
        private Coroutine _corpseRoutine;
        private WaitForSeconds _corpseDelay;
        private bool _hasShield;
        private bool _isDead;

        public event Action<Enemy> Died;

        public EnemyConfigSO Config => _config;

        public bool IsAlive => !_isDead;

        private void Awake()
        {
            _colliders = GetComponents<Collider>();
            _facing = GetComponent<EnemyFacing>();
            _baseScale = transform.localScale;

            // Cached: allocating a new WaitForSeconds on every death creates garbage.
            _corpseDelay = new WaitForSeconds(_config.CorpseDuration);

            if (_rigidbody == null)
            {
                _rigidbody = GetComponent<Rigidbody>();
            }

            if (_animator == null)
            {
                _animator = GetComponentInChildren<Animator>();
            }

            if (_animator != null)
            {
                // Follow the game's time scale (pause, slow motion) and skip bone updates off screen.
                _animator.updateMode = AnimatorUpdateMode.Normal;
                _animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            }

            Camera mainCamera = Camera.main;
            _cameraTransform = mainCamera != null ? mainCamera.transform : null;
            CreateShieldIcon();
        }

        private void LateUpdate()
        {
            // Billboard: keep the shield icon facing the camera.
            if (_shieldIcon != null && _cameraTransform != null && _shieldIcon.gameObject.activeSelf)
            {
                _shieldIcon.rotation = _cameraTransform.rotation;
            }
        }

        /// <summary>Resets a pooled enemy for a new life.</summary>
        public void Spawn(bool hasShield, Transform target, Action<Enemy> despawn)
        {
            if (_corpseRoutine != null)
            {
                StopCoroutine(_corpseRoutine);
                _corpseRoutine = null;
            }

            _despawn = despawn;
            _isDead = false;
            _hasShield = hasShield;
            transform.localScale = _baseScale;
            SetCollidersEnabled(true);
            SetRagdoll(false);

            if (_shieldIcon != null)
            {
                _shieldIcon.gameObject.SetActive(hasShield);
            }

            if (_facing != null)
            {
                _facing.enabled = true;
                _facing.SetTarget(target);
            }

            if (_animator != null)
            {
                _animator.ResetTrigger(s_attackTrigger);
                _animator.ResetTrigger(s_hitTrigger);
                _animator.ResetTrigger(s_shieldTrigger);
                _animator.ResetTrigger(s_dieTrigger);
                _animator.Play(s_idleState, 0, 0f);
            }
        }

        public void TakeHit(Vector3 point, Vector3 direction)
        {
            if (_isDead)
            {
                return;
            }

            if (_hasShield)
            {
                BreakShield();
                return;
            }

            Die(direction);
        }

        /// <summary>Returns the enemy to its pool right away (used when it is far behind the gun).</summary>
        public void Despawn()
        {
            if (_corpseRoutine != null)
            {
                StopCoroutine(_corpseRoutine);
                _corpseRoutine = null;
            }

            Action<Enemy> despawn = _despawn;
            _despawn = null;
            despawn?.Invoke(this);
        }

        private void BreakShield()
        {
            _hasShield = false;
            if (_shieldIcon != null)
            {
                _shieldIcon.gameObject.SetActive(false);
            }

            if (_animator != null)
            {
                _animator.SetTrigger(s_shieldTrigger);
            }

            PlayCue(_config.ShieldBreakCue);
        }

        private void Die(Vector3 direction)
        {
            _isDead = true;

            // Let the gun fly through the body straight away.
            SetCollidersEnabled(false);
            if (_facing != null)
            {
                _facing.enabled = false;
            }

            if (_animator != null)
            {
                _animator.ResetTrigger(s_hitTrigger);
                _animator.ResetTrigger(s_attackTrigger);
                _animator.SetTrigger(s_dieTrigger);
            }

            SetRagdoll(true);
            Vector3 knock = (direction.normalized + Vector3.up).normalized * _config.DeathKnockImpulse;
            _rigidbody.AddForce(knock, ForceMode.Impulse);

            PlayCue(_config.HitCue);
            PlayCue(_config.DeathCue);
            _enemyKilled.RaiseEvent(_config.CoinReward);
            Died?.Invoke(this);

            _corpseRoutine = StartCoroutine(RemoveCorpse());
        }

        private IEnumerator RemoveCorpse()
        {
            yield return _corpseDelay;

            float elapsed = 0f;
            while (elapsed < _config.ShrinkDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _config.ShrinkDuration);
                transform.localScale = _baseScale * (1f - t * t);
                yield return null;
            }

            _corpseRoutine = null;
            Despawn();
        }

        private void SetRagdoll(bool isFalling)
        {
            if (_rigidbody == null)
            {
                return;
            }

            if (!_rigidbody.isKinematic)
            {
                // Velocities can only be written on non-kinematic bodies.
                _rigidbody.linearVelocity = Vector3.zero;
                _rigidbody.angularVelocity = Vector3.zero;
            }

            _rigidbody.isKinematic = !isFalling;
            _rigidbody.useGravity = isFalling;
            _rigidbody.constraints = RigidbodyConstraints.None;
        }

        private void SetCollidersEnabled(bool isEnabled)
        {
            foreach (Collider enemyCollider in _colliders)
            {
                enemyCollider.enabled = isEnabled;
            }
        }

        private void CreateShieldIcon()
        {
            Sprite icon = _config.ShieldIcon;
            if (icon == null)
            {
                return;
            }

            var iconObject = new GameObject("ShieldIcon");
            _shieldIcon = iconObject.transform;
            _shieldIcon.SetParent(transform, false);
            _shieldIcon.localPosition = new Vector3(0f, _config.ShieldIconHeight, 0f);

            // Size the sprite in world units regardless of its pixel size and of the enemy's scale.
            float spriteWidth = Mathf.Max(0.01f, icon.bounds.size.x);
            float parentScale = Mathf.Max(0.01f, transform.lossyScale.x);
            _shieldIcon.localScale = Vector3.one * (_config.ShieldIconSize / (spriteWidth * parentScale));

            SpriteRenderer spriteRenderer = iconObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = icon;
            iconObject.SetActive(false);
        }

        private static void PlayCue(AudioCueSO cue)
        {
            if (cue != null)
            {
                cue.Play();
            }
        }
    }
}
