using UnityEngine;
using UnityEngine.Serialization;

namespace GunSprint.Utils
{
    /// <summary>
    /// Smoothly follows a target with a fixed offset and looks slightly ahead of it.
    /// Pair it with an interpolated Rigidbody on the target to avoid jitter.
    /// </summary>
    public class FollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _offset = new Vector3(2.85f, 1.98f, 4.445f);

        [Tooltip("How quickly the camera catches up with the target. Higher is snappier.")]
        [FormerlySerializedAs("_smoothSpeed")]
        [SerializeField, Min(0.1f)] private float _followSharpness = 5f;

        [Tooltip("Positive values shift the target to the left of the frame, showing more of the track ahead.")]
        [SerializeField] private float _lookRightOffset = 0.58f;

        [Tooltip("Positive values make the camera look higher, so the target sits lower in the frame.")]
        [SerializeField] private float _lookUpOffset = 0.82f;

        private void Start()
        {
            SnapToTarget();
        }

        private void LateUpdate()
        {
            if (_target == null)
            {
                return;
            }

            // Exponential damping gives the same smoothing at 30, 60 or 120 fps.
            float blend = 1f - Mathf.Exp(-_followSharpness * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, _target.position + _offset, blend);
            LookAhead();
        }

        public void SnapToTarget()
        {
            if (_target == null)
            {
                return;
            }

            transform.position = _target.position + _offset;
            LookAhead();
        }

        private void LookAhead()
        {
            Vector3 lookPoint = _target.position
                + transform.right * _lookRightOffset
                + transform.up * _lookUpOffset;
            transform.LookAt(lookPoint);
        }
    }
}
