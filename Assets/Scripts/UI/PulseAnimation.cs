using UnityEngine;

namespace GunSprint.UI
{
    /// <summary>
    /// Gently pulses the scale to draw attention (tap hint, new record badge).
    /// </summary>
    public class PulseAnimation : MonoBehaviour
    {
        [SerializeField, Range(0f, 0.5f)] private float _amplitude = 0.08f;

        [Tooltip("Pulses per second.")]
        [SerializeField, Min(0.01f)] private float _frequency = 1.2f;

        private Vector3 _baseScale;

        private void Awake()
        {
            _baseScale = transform.localScale;
        }

        private void OnDisable()
        {
            transform.localScale = _baseScale;
        }

        private void Update()
        {
            float wave = Mathf.Sin(Time.unscaledTime * _frequency * 2f * Mathf.PI);
            transform.localScale = _baseScale * (1f + _amplitude * wave);
        }
    }
}
