using UnityEngine;

public class Target : MonoBehaviour {
    [SerializeField] private GameObject _hitEffectPrefab;
    [SerializeField] private GameObject _fragmentPrefab;
    [SerializeField] private int _numberOfFragments = 5;
    [SerializeField] private float _explosionForce = 10f;
    [SerializeField] private float _fragmentLifeTime = 2f;
    
    [SerializeField] private int _scoreValue = 10;

    private void OnCollisionEnter(Collision collision) {
        // Kiểm tra xem vật thể va chạm có phải là đạn không
        if (collision.gameObject.GetComponent<Bullet>() != null) {
            // Logic để tắt Slow-motion về bình thường khi mục tiêu bị bắn trúng
            if (GameManager.Instance != null) {
                GameManager.Instance.ToggleSlowMo(false);
                GameManager.Instance.AddScore(_scoreValue);
            }
            
            // Phát hiệu ứng nổ và hủy nó sau 1 giây
            if (_hitEffectPrefab != null) {
                GameObject hitEffect = Instantiate(_hitEffectPrefab, transform.position, Quaternion.identity);
                Destroy(hitEffect, 1f);
            }

            // Tạo và văng các mảnh vỡ
            for (int i = 0; i < _numberOfFragments; i++) {
                if (_fragmentPrefab != null) {
                    GameObject fragment = Instantiate(_fragmentPrefab, transform.position, Random.rotation);
                    Rigidbody rb = fragment.GetComponent<Rigidbody>();
                    if (rb != null) {
                        rb.AddExplosionForce(_explosionForce, transform.position, 1f);
                    }
                    Destroy(fragment, _fragmentLifeTime);
                }
            }

            // Phá hủy mục tiêu ban đầu
            Destroy(gameObject);
        }
    }
}