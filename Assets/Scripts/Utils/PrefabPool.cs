using UnityEngine;
using UnityEngine.Pool;

namespace GunSprint.Utils
{
    /// <summary>
    /// Reuses instances of a prefab component instead of calling Instantiate/Destroy for every
    /// bullet, enemy or floor. Built on UnityEngine.Pool, so there is no custom pooling logic to maintain.
    /// </summary>
    public sealed class PrefabPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly ObjectPool<T> _pool;

        public PrefabPool(T prefab, Transform parent, int defaultCapacity = 8, int maxSize = 64)
        {
            _prefab = prefab;
            _parent = parent;
            _pool = new ObjectPool<T>(
                CreateInstance,
                actionOnGet: null,
                actionOnRelease: Deactivate,
                actionOnDestroy: DestroyInstance,
                collectionCheck: Application.isEditor,
                defaultCapacity: defaultCapacity,
                maxSize: maxSize);
        }

        public int CountActive => _pool.CountActive;

        /// <summary>
        /// Takes an instance from the pool, places it and only then activates it,
        /// so trails and rigidbodies never see the old position.
        /// </summary>
        public T Get(Vector3 position, Quaternion rotation)
        {
            T instance = _pool.Get();
            instance.transform.SetPositionAndRotation(position, rotation);
            instance.gameObject.SetActive(true);
            return instance;
        }

        public void Release(T instance)
        {
            _pool.Release(instance);
        }

        public void Clear()
        {
            _pool.Clear();
        }

        private T CreateInstance()
        {
            T instance = Object.Instantiate(_prefab, _parent);
            instance.gameObject.SetActive(false);
            return instance;
        }

        private static void Deactivate(T instance)
        {
            instance.gameObject.SetActive(false);
        }

        private static void DestroyInstance(T instance)
        {
            if (instance != null)
            {
                Object.Destroy(instance.gameObject);
            }
        }
    }
}
