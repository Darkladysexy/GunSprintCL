using System;
using UnityEngine;

namespace GunSprint.Utils
{
    /// <summary>
    /// Pool for fire-and-forget particle effects. Each spawned effect returns itself to the pool
    /// after a fixed lifetime, replacing the Instantiate + Destroy(effect, time) pattern.
    /// </summary>
    public sealed class EffectPool
    {
        private readonly PrefabPool<PooledEffect> _pool;
        private readonly Action<PooledEffect> _release;
        private readonly float _lifetime;

        public EffectPool(GameObject prefab, Transform parent, float lifetime, int defaultCapacity = 4, int maxSize = 16)
        {
            _lifetime = lifetime;
            PooledEffect template = PooledEffect.CreateTemplate(prefab, parent);
            _pool = new PrefabPool<PooledEffect>(template, parent, defaultCapacity, maxSize);

            // Cached once: converting a method group to a delegate allocates on every call.
            _release = _pool.Release;
        }

        public void Spawn(Vector3 position, Quaternion rotation)
        {
            PooledEffect effect = _pool.Get(position, rotation);
            effect.Play(_lifetime, _release);
        }
    }
}
