using UnityEngine;

public class ScoreMultiplier : MonoBehaviour
{
    [SerializeField] private int _multiplier = 10;
    [SerializeField] private GameObject _hitEffectPrefab;
    [SerializeField] private GameObject _fragmentPrefab;
    [SerializeField] private int _numberOfFragments = 10;
    [SerializeField] private float _explosionForce = 15f;
    [SerializeField] private float _fragmentLifeTime = 2f;

    private bool _isHit = false;

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Va cham voi: " + collision.gameObject.name); // Dòng kiểm tra
        if (_isHit) return;

        // Kiểm tra nếu va chạm với đạn
        if (collision.gameObject.GetComponent<Bullet>() != null)
        {
            _isHit = true;

            // 1. Hiệu ứng nổ tại chỗ
            if (_hitEffectPrefab != null) 
                Instantiate(_hitEffectPrefab, transform.position, Quaternion.identity);

            // 2. Tạo các mảnh vỡ văng ra
            for (int i = 0; i < _numberOfFragments; i++)
            {
                if (_fragmentPrefab != null)
                {
                    GameObject fragment = Instantiate(_fragmentPrefab, transform.position, Random.rotation);
                    Rigidbody rb = fragment.GetComponent<Rigidbody>();
                    if (rb != null) rb.AddExplosionForce(_explosionForce, transform.position, 2f);
                    Destroy(fragment, _fragmentLifeTime);
                }
            }

            // 3. Gọi GameManager để tính điểm x và hiện Win UI
            if (GameManager.Instance != null)
            {
                GameManager.Instance.WinLevel(_multiplier);
            }

            // 4. Chỉ ẩn stack này đi
            gameObject.SetActive(false);
        }
    }
}