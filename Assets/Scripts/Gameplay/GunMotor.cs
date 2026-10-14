using GunSprint.Audio;
using GunSprint.Data;
using UnityEngine;

namespace GunSprint.Gameplay
{
    /// <summary>
    /// Physics side of the gun: recoil, spin, floor assist and landing bounce.
    /// Uses impulses only, so a shot pushes the gun the same way at normal speed and in slow motion.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [DisallowMultipleComponent]
    public class GunMotor : MonoBehaviour
    {
        [SerializeField] private GunConfigSO _config;
        [SerializeField] private AudioCueSO _bounceCue;

        [Tooltip("Tag used by every floor collider.")]
        [SerializeField] private string _floorTag = "Floor";

        private Rigidbody _rigidbody;
        private int _floorContacts;

        public Rigidbody Body => _rigidbody;

        public bool IsTouchingFloor => _floorContacts > 0;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            // The follow camera reads the transform every frame; interpolation removes physics-step jitter.
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        }

        private void FixedUpdate()
        {
            // The gun only spins around Z (the other axes are frozen); cap the spin so it stays controllable.
            float spin = Mathf.Clamp(_rigidbody.angularVelocity.z, -_config.MaxAngularVelocity, _config.MaxAngularVelocity);
            _rigidbody.angularVelocity = new Vector3(0f, 0f, spin);
        }

        /// <summary>Switches to another gun's stats (see GunLoadout).</summary>
        public void SetConfig(GunConfigSO config)
        {
            _config = config;
        }

        /// <summary>Pushes the gun backwards and spins it, like a real recoil.</summary>
        public void ApplyRecoil(Vector3 muzzleForward)
        {
            _rigidbody.AddForce(-transform.forward * _config.RecoilImpulse, ForceMode.Impulse);

            if (IsTouchingFloor)
            {
                // Helps the gun take off again when it lies on the floor, fading out with height.
                float heightFactor = Mathf.InverseLerp(0f, _config.UpAssistHeight, _rigidbody.position.y);
                float assist = Mathf.Lerp(_config.UpAssistImpulse, 0f, heightFactor);
                _rigidbody.AddForce(Vector3.up * (_config.FloorJumpImpulse + assist), ForceMode.Impulse);
            }

            float spinFactor = Mathf.InverseLerp(0f, _config.MaxAngularVelocity, Mathf.Abs(_rigidbody.angularVelocity.z));
            float spinImpulse = _config.SpinImpulse + Mathf.Lerp(0f, _config.MaxSpinBonus, spinFactor);
            Vector3 spinAxis = Vector3.Dot(muzzleForward, Vector3.right) < 0f ? Vector3.back : Vector3.forward;
            _rigidbody.AddTorque(spinAxis * spinImpulse, ForceMode.Impulse);
        }

        /// <summary>Moves the gun to a start pose and removes any motion.</summary>
        public void Teleport(Vector3 position, Quaternion rotation)
        {
            if (!_rigidbody.isKinematic)
            {
                _rigidbody.linearVelocity = Vector3.zero;
                _rigidbody.angularVelocity = Vector3.zero;
            }

            _rigidbody.position = position;
            _rigidbody.rotation = rotation;
            transform.SetPositionAndRotation(position, rotation);
            _floorContacts = 0;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!collision.gameObject.CompareTag(_floorTag))
            {
                return;
            }

            _floorContacts++;
            if (_bounceCue != null)
            {
                _bounceCue.Play();
            }

            float bounce = collision.relativeVelocity.magnitude * _config.LandingBounce;
            _rigidbody.AddForce(Vector3.up * bounce, ForceMode.Impulse);
        }

        private void OnCollisionExit(Collision collision)
        {
            if (collision.gameObject.CompareTag(_floorTag))
            {
                _floorContacts = Mathf.Max(0, _floorContacts - 1);
            }
        }
    }
}
