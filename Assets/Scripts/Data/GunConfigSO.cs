using UnityEngine;

namespace GunSprint.Data
{
    /// <summary>
    /// Tuning values for the gun. Kept in an asset so designers can tweak them in Play mode
    /// and keep the changes. All forces are impulses (N·s) so they behave identically in slow motion.
    /// </summary>
    [CreateAssetMenu(fileName = "GunConfig", menuName = "GunSprint/Data/Gun Config")]
    public class GunConfigSO : ScriptableObject
    {
        [Header("Shooting")]
        [Tooltip("Bullet speed before the Power upgrade is applied.")]
        [SerializeField, Min(0f)] private float _bulletSpeed = 12f;

        [Tooltip("Seconds the muzzle smoke keeps emitting after a shot.")]
        [SerializeField, Min(0f)] private float _smokeDuration = 1f;

        [Tooltip("Time scale while a bullet that is going to hit a target is in flight.")]
        [SerializeField, Range(0.05f, 1f)] private float _slowMotionScale = 0.2f;

        [Header("Recoil")]
        [Tooltip("Backwards impulse applied to the gun on every shot.")]
        [SerializeField, Min(0f)] private float _recoilImpulse = 8f;

        [Tooltip("Spin impulse applied on every shot.")]
        [SerializeField, Min(0f)] private float _spinImpulse = 2.4f;

        [Tooltip("Extra spin impulse added when the gun is already spinning at max speed.")]
        [SerializeField, Min(0f)] private float _maxSpinBonus = 3f;

        [SerializeField, Min(0.1f)] private float _maxAngularVelocity = 10f;

        [Header("Floor assist")]
        [Tooltip("Upward impulse when shooting while touching the floor.")]
        [SerializeField, Min(0f)] private float _floorJumpImpulse = 0.7f;

        [Tooltip("Extra upward impulse at height 0, fading to zero at the assist height.")]
        [SerializeField, Min(0f)] private float _upAssistImpulse = 4f;

        [SerializeField, Min(0.1f)] private float _upAssistHeight = 10f;

        [Tooltip("Fraction of the landing speed turned into an upward bounce.")]
        [SerializeField, Min(0f)] private float _landingBounce = 0.2f;

        [Header("Bounds")]
        [Tooltip("Falling below this height ends the run.")]
        [SerializeField] private float _killHeight = -5f;

        public float BulletSpeed => _bulletSpeed;

        public float SmokeDuration => _smokeDuration;

        public float SlowMotionScale => _slowMotionScale;

        public float RecoilImpulse => _recoilImpulse;

        public float SpinImpulse => _spinImpulse;

        public float MaxSpinBonus => _maxSpinBonus;

        public float MaxAngularVelocity => _maxAngularVelocity;

        public float FloorJumpImpulse => _floorJumpImpulse;

        public float UpAssistImpulse => _upAssistImpulse;

        public float UpAssistHeight => _upAssistHeight;

        public float LandingBounce => _landingBounce;

        public float KillHeight => _killHeight;
    }
}
