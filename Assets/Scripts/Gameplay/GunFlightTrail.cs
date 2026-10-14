using GunSprint.Data;
using GunSprint.Utils;
using UnityEngine;

namespace GunSprint.Gameplay
{
    /// <summary>
    /// Draws the equipped gun's trail effect while it flies fast, and stops it when the gun rests
    /// or is teleported, so it never smears across the screen.
    /// </summary>
    [RequireComponent(typeof(GunLoadout), typeof(GunMotor))]
    public class GunFlightTrail : MonoBehaviour
    {
        [Tooltip("The trail shows above this speed (m/s).")]
        [SerializeField, Min(0f)] private float _minSpeed = 3f;

        [Tooltip("Multiplier on the trail effect's width, which is designed for small bullets.")]
        [SerializeField, Min(0f)] private float _widthScale = 1.5f;

        private Rigidbody _body;
        private TrailRenderer[] _trails = new TrailRenderer[0];
        private ParticleSystem[] _particles = new ParticleSystem[0];
        private bool _isEmitting = true;

        private void Start()
        {
            _body = GetComponent<GunMotor>().Body;
            GunDefinitionSO gun = GetComponent<GunLoadout>().Gun;
            GameObject prefab = gun != null ? gun.TrailPrefab : null;
            if (prefab == null)
            {
                return;
            }

            // Built under an inactive holder so the effect's sounds cannot play on awake.
            var holder = new GameObject("FlightTrail");
            holder.SetActive(false);
            holder.transform.SetParent(transform, false);
            GameObject trail = Instantiate(prefab, holder.transform);
            trail.transform.localPosition = Vector3.zero;
            foreach (AudioSource source in trail.GetComponentsInChildren<AudioSource>(true))
            {
                source.playOnAwake = false;
                source.enabled = false;
            }

            TrailEffects.DisableScreenDistortion(trail);
            _trails = trail.GetComponentsInChildren<TrailRenderer>(true);
            foreach (TrailRenderer t in _trails)
            {
                t.widthMultiplier *= _widthScale;
            }

            _particles = trail.GetComponentsInChildren<ParticleSystem>(true);
            holder.SetActive(true);
            SetEmitting(false);
        }

        private void Update()
        {
            if (_trails.Length == 0 && _particles.Length == 0)
            {
                return;
            }

            bool isFlying = _body.linearVelocity.sqrMagnitude > _minSpeed * _minSpeed;
            if (isFlying != _isEmitting)
            {
                SetEmitting(isFlying);
            }
        }

        private void SetEmitting(bool isEmitting)
        {
            _isEmitting = isEmitting;
            foreach (TrailRenderer t in _trails)
            {
                if (isEmitting)
                {
                    t.Clear();
                }

                t.emitting = isEmitting;
            }

            foreach (ParticleSystem particles in _particles)
            {
                if (isEmitting)
                {
                    particles.Play(false);
                }
                else
                {
                    particles.Stop(false, ParticleSystemStopBehavior.StopEmitting);
                }
            }
        }
    }
}
