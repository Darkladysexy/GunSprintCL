using System;
using UnityEngine;

namespace GunSprint.Utils
{
    /// <summary>
    /// Added at runtime to pooled particle effects. Restarts the particles on spawn and hands the
    /// effect back to its pool once its lifetime is over.
    /// </summary>
    public sealed class PooledEffect : MonoBehaviour
    {
        private ParticleSystem _particles;
        private Action<PooledEffect> _release;
        private float _releaseTime;

        /// <summary>
        /// Creates an inactive template from a plain effect prefab; the pool clones this template.
        /// </summary>
        public static PooledEffect CreateTemplate(GameObject prefab, Transform parent)
        {
            GameObject template = Instantiate(prefab, parent);
            template.name = prefab.name;
            template.SetActive(false);
            return template.AddComponent<PooledEffect>();
        }

        public void Play(float lifetime, Action<PooledEffect> release)
        {
            _release = release;
            _releaseTime = Time.time + lifetime;
            if (_particles != null)
            {
                _particles.Clear(true);
                _particles.Play(true);
            }
        }

        private void Awake()
        {
            _particles = GetComponentInChildren<ParticleSystem>();
        }

        private void Update()
        {
            if (_release != null && Time.time >= _releaseTime)
            {
                Action<PooledEffect> release = _release;
                _release = null;
                release(this);
            }
        }
    }
}
