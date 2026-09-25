using UnityEngine;

namespace GunSprint.Gameplay
{
    /// <summary>
    /// Defines the straight track the gun travels along: it starts at this transform and points
    /// towards <c>_end</c>. Distances are measured in meters along the track, ignoring height.
    /// </summary>
    public class TrackPath : MonoBehaviour
    {
        [Tooltip("Any point further along the track; only its direction matters.")]
        [SerializeField] private Transform _end;

        public Vector3 Forward
        {
            get
            {
                if (_end == null)
                {
                    return Vector3.forward;
                }

                Vector3 direction = _end.position - transform.position;
                direction.y = 0f;
                return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
            }
        }

        /// <summary>Right-hand side of the track, used for lateral offsets.</summary>
        public Vector3 Right => Vector3.Cross(Vector3.up, Forward);

        public float GetDistance(Vector3 worldPosition)
        {
            Vector3 offset = worldPosition - transform.position;
            offset.y = 0f;
            return Vector3.Dot(offset, Forward);
        }

        /// <summary>Point on the track at the given distance, at height 0.</summary>
        public Vector3 GetPoint(float distance)
        {
            Vector3 origin = transform.position;
            origin.y = 0f;
            return origin + Forward * distance;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Vector3 origin = transform.position;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(origin, origin + Forward * 120f);
            Gizmos.DrawSphere(origin, 0.15f);
        }
#endif
    }
}
