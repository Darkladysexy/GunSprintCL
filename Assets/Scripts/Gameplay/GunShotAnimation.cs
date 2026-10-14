using UnityEngine;

namespace GunSprint.Gameplay
{
    /// <summary>
    /// Procedural shot animation: the model kicks back and up, and the slide (if the model has a
    /// part named "Bolt" or "Slide") racks back. Code instead of Animator clips, so it works on every
    /// model in the shop and costs nothing when idle. Uses scaled time: shots in slow motion kick slowly.
    /// </summary>
    [RequireComponent(typeof(GunController), typeof(GunLoadout))]
    public class GunShotAnimation : MonoBehaviour
    {
        [Tooltip("Kick distance as a fraction of the gun length.")]
        [SerializeField, Range(0f, 0.5f)] private float _kickDistance = 0.12f;

        [Tooltip("Degrees the muzzle rises on a shot.")]
        [SerializeField, Range(0f, 45f)] private float _kickAngle = 14f;

        [Tooltip("Seconds to recover from the kick.")]
        [SerializeField, Min(0.01f)] private float _recoverDuration = 0.2f;

        [Tooltip("Slide travel as a fraction of the gun length.")]
        [SerializeField, Range(0f, 0.5f)] private float _slideTravel = 0.16f;

        [Tooltip("Seconds for the slide to return; shorter than the kick for a snappy look.")]
        [SerializeField, Min(0.01f)] private float _slideDuration = 0.1f;

        private GunController _controller;
        private Transform _visual;
        private Transform _slide;
        private Vector3 _visualRestPosition;
        private Quaternion _visualRestRotation;
        private Vector3 _slideRestPosition;
        private Vector3 _slideOffset;
        private float _kickBack;
        private float _kickTimer;
        private float _slideTimer;

        private void Awake()
        {
            _controller = GetComponent<GunController>();
        }

        private void OnEnable()
        {
            _controller.Fired += HandleFired;
        }

        private void OnDisable()
        {
            _controller.Fired -= HandleFired;
        }

        private void Start()
        {
            // After GunLoadout.Awake, so the equipped model is in place.
            GunLoadout loadout = GetComponent<GunLoadout>();
            _visual = loadout.Visual;
            _visualRestPosition = _visual.localPosition;
            _visualRestRotation = _visual.localRotation;
            _kickBack = _kickDistance * loadout.Length;

            _slide = GunModelFitter.FindPart(loadout.Model, "Bolt", "Slide");
            if (_slide != null)
            {
                _slideRestPosition = _slide.localPosition;
                // Backwards along the gun's barrel, converted into the slide parent's (scaled) space.
                Vector3 back = Vector3.back * (_slideTravel * loadout.Length);
                _slideOffset = _slide.parent.InverseTransformVector(transform.TransformVector(back));
            }
        }

        private void Update()
        {
            if (_kickTimer <= 0f && _slideTimer <= 0f)
            {
                return;
            }

            float deltaTime = Time.deltaTime;
            _kickTimer = Mathf.Max(0f, _kickTimer - deltaTime / _recoverDuration);
            _slideTimer = Mathf.Max(0f, _slideTimer - deltaTime / _slideDuration);

            // Quadratic ease-out: fast snap back at first, soft settle at the end.
            float kick = _kickTimer * _kickTimer;
            _visual.localPosition = _visualRestPosition + Vector3.back * (_kickBack * kick);
            _visual.localRotation = _visualRestRotation * Quaternion.Euler(-_kickAngle * kick, 0f, 0f);

            if (_slide != null)
            {
                _slide.localPosition = _slideRestPosition + _slideOffset * (_slideTimer * _slideTimer);
            }
        }

        private void HandleFired()
        {
            _kickTimer = 1f;
            _slideTimer = 1f;
        }
    }
}
