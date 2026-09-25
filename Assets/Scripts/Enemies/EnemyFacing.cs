using UnityEngine;
using UnityEngine.Serialization;

namespace GunSprint.Enemies
{
    /// <summary>
    /// Turns the enemy on the Y axis to face its target (the gun), so enemies watch the player approach.
    /// </summary>
    public class EnemyFacing : MonoBehaviour
    {
        [Tooltip("Degrees per second.")]
        [FormerlySerializedAs("turnSpeed")]
        [SerializeField, Min(0f)] private float _turnSpeed = 360f;

        private Transform _target;

        /// <summary>Sets the target and instantly faces it, so freshly spawned enemies never pop around.</summary>
        public void SetTarget(Transform target)
        {
            _target = target;
            if (TryGetLookRotation(out Quaternion rotation))
            {
                transform.rotation = rotation;
            }
        }

        private void LateUpdate()
        {
            if (TryGetLookRotation(out Quaternion rotation))
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, _turnSpeed * Time.deltaTime);
            }
        }

        private bool TryGetLookRotation(out Quaternion rotation)
        {
            rotation = Quaternion.identity;
            if (_target == null)
            {
                return false;
            }

            Vector3 direction = _target.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f)
            {
                return false;
            }

            rotation = Quaternion.LookRotation(direction);
            return true;
        }
    }
}
