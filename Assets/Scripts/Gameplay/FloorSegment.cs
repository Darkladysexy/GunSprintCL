using UnityEngine;

namespace GunSprint.Gameplay
{
    /// <summary>
    /// A piece of track (flat ground, steps, islands, ramp...). Its blocks are laid out along the
    /// local +Z axis, centred on the pivot, so level and endless builders can chain segments
    /// end to end and orient them along the track.
    /// </summary>
    public class FloorSegment : MonoBehaviour
    {
        [Tooltip("Length along local Z, in meters.")]
        [SerializeField, Min(1f)] private float _length = 20f;

        [Tooltip("How many enemies fit comfortably on this segment.")]
        [SerializeField, Min(0)] private int _enemySlots = 2;

        public float Length => _length;

        public int EnemySlots => _enemySlots;

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(2f, 0.1f, _length));
        }
#endif
    }
}
