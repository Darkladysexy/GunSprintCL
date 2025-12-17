using UnityEngine;

public class ScoreMultiplier : MonoBehaviour
{
    [SerializeField] private int _multiplier = 10;

    [SerializeField] private GameObject _hitEffectPrefab;
    [SerializeField] private GameObject _fragmentPrefab;
    [SerializeField] private int _numberOfFragments = 5;
    [SerializeField] private float _explosionForce = 10f;
    [SerializeField] private float _fragmentLifeTime = 2f;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<Bullet>() != null)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.MultiplyScore(_multiplier);
            }
            
            if (_hitEffectPrefab != null)
            {
                Instantiate(_hitEffectPrefab, transform.position, Quaternion.identity);
            }

            for (int i = 0; i < _numberOfFragments; i++)
            {
                if (_fragmentPrefab != null)
                {
                    GameObject fragment = Instantiate(_fragmentPrefab, transform.position, Random.rotation);
                    Rigidbody rb = fragment.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.AddExplosionForce(_explosionForce, transform.position, 1f);
                    }
                    Destroy(fragment, _fragmentLifeTime);
                }
            }
            
            Destroy(gameObject);
        }
    }
}