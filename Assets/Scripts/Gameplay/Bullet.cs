using UnityEngine;

public class Bullet : MonoBehaviour {
    [SerializeField] private GameObject _explosionPrefab;
    [SerializeField] private TrailRenderer _trail;

    private Rigidbody _rb;
    private bool _hasExploded = false;
    private Collider _bulletCollider;

    private void Awake() {
        _rb = GetComponent<Rigidbody>();
        _bulletCollider = GetComponent<Collider>();
        if (_trail != null) _trail.Clear();
    }

    public void Init(Vector3 velocity, Collider gunCollider) {
        _rb.linearVelocity = velocity;

        if (_bulletCollider != null && gunCollider != null) {
            Physics.IgnoreCollision(_bulletCollider, gunCollider);
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (_hasExploded) return;
        _hasExploded = true;

        // Nếu đạn đập vào tường và biến mất, hãy đảm bảo tắt Slow-mo
        if (GameManager.Instance != null) {
            GameManager.Instance.ToggleSlowMo(false);
        }

        if (_explosionPrefab != null) {
            var explosion = Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 1f);
        }

        Destroy(gameObject);
}
}