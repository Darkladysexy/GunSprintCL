using GunSprint.Audio;
using GunSprint.Events;
using UnityEngine;
using UnityEngine.Serialization;

namespace GunSprint.Gameplay
{
    /// <summary>
    /// Finish-line target: shatters when shot and reports its coin multiplier.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class MultiplierTarget : MonoBehaviour, IHittable
    {
        [SerializeField, Min(1)] private int _multiplier = 10;
        [SerializeField] private IntEventChannelSO _targetHit;
        [SerializeField] private AudioCueSO _breakCue;

        [Header("Shatter")]
        [SerializeField] private GameObject _fragmentPrefab;

        [FormerlySerializedAs("_numberOfFragments")]
        [SerializeField, Min(0)] private int _fragmentCount = 5;

        [SerializeField, Min(0f)] private float _explosionForce = 20f;

        [SerializeField, Min(0.1f)] private float _explosionRadius = 10f;

        [FormerlySerializedAs("_fragmentLifeTime")]
        [SerializeField, Min(0.1f)] private float _fragmentLifetime = 2f;

        private bool _isBroken;

        public int Multiplier => _multiplier;

        private void OnEnable()
        {
            _isBroken = false;
        }

        public void TakeHit(Vector3 point, Vector3 direction)
        {
            if (_isBroken)
            {
                return;
            }

            _isBroken = true;
            Shatter();

            if (_breakCue != null)
            {
                _breakCue.Play();
            }

            _targetHit.RaiseEvent(_multiplier);
            gameObject.SetActive(false);
        }

        private void Shatter()
        {
            if (_fragmentPrefab == null)
            {
                return;
            }

            // Happens once per level, so plain instantiation is fine here (no pool needed).
            Vector3 origin = transform.position;
            for (int i = 0; i < _fragmentCount; i++)
            {
                GameObject fragment = Instantiate(_fragmentPrefab, origin, Random.rotation);
                if (fragment.TryGetComponent(out Rigidbody body))
                {
                    body.AddExplosionForce(_explosionForce, origin, _explosionRadius);
                }

                Destroy(fragment, _fragmentLifetime);
            }
        }
    }
}
