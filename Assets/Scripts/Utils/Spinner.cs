using UnityEngine;

namespace GunSprint.Utils
{
    /// <summary>
    /// Rotates the object around one of its local axes at a constant speed (menu showcase, pickups...).
    /// </summary>
    public class Spinner : MonoBehaviour
    {
        [SerializeField] private Vector3 _axis = Vector3.up;

        [Tooltip("Degrees per second.")]
        [SerializeField] private float _speed = 45f;

        [Tooltip("Keep spinning while the game is paused (menus).")]
        [SerializeField] private bool _useUnscaledTime = true;

        private void Update()
        {
            float deltaTime = _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            transform.Rotate(_axis, _speed * deltaTime, Space.Self);
        }
    }
}
