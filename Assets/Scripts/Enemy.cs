using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class Enemy : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator _anim; // Triggers: Attack, Hit, Die, Shield

    [Header("Gameplay")]
    [SerializeField] private bool _hasShield = false;
    [SerializeField] private int _scoreValue = 10;

    [Header("FX (optional)")]
    [SerializeField] private GameObject _hitFx;
    [SerializeField] private GameObject _shieldBreakFx;
    [SerializeField] private GameObject _deathFx;

    [Header("Physics")]
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private float _deathKnockForce = 5f;
    [SerializeField] private bool _kinematicWhileAlive = true;
    [SerializeField] private bool _freezeYWhileAlive = true;

    private Collider _col;
    private bool _dead = false;

    void Awake(){
        _col = GetComponent<Collider>();
        if (_rb == null) _rb = GetComponent<Rigidbody>();
        if (_anim == null) _anim = GetComponentInChildren<Animator>();
        SetupAliveRB();
    }

    void SetupAliveRB(){
        if (_rb == null) return;
        if (_freezeYWhileAlive) _rb.constraints |= RigidbodyConstraints.FreezePositionY;
        _rb.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        if (_kinematicWhileAlive){ _rb.isKinematic = true; _rb.useGravity = false; }
    }

    // ===== API cho Spawner =====
    public void Init(bool hasShield){
        _dead = false;
        _hasShield = hasShield;
        if (_col) _col.enabled = true;

        if (_rb){
            _rb.isKinematic = true; _rb.useGravity = false;
            _rb.linearVelocity = Vector3.zero; _rb.angularVelocity = Vector3.zero;
            SetupAliveRB();
        }

        if (_anim){
            _anim.ResetTrigger("Hit"); _anim.ResetTrigger("Attack");
            _anim.ResetTrigger("Shield"); _anim.ResetTrigger("Die");
            _anim.Play("Idle", 0, 0f);
        }
    }

    // ===== Collision/Trigger với đạn =====
    void OnTriggerEnter(Collider other){
        if (_dead) return;
        if (!TryGetBullet(other, out var _)) return;
        OnHitByBullet(other.ClosestPoint(transform.position));
    }
    void OnCollisionEnter(Collision other){
        if (_dead) return;
        if (!TryGetBullet(other.collider, out var _)) return;
        Vector3 p = other.contactCount > 0 ? other.GetContact(0).point : transform.position;
        OnHitByBullet(p);
    }

    // ===== Public shortcuts =====
    public void PlayAttack(){ if (!_dead && _anim){ _anim.ResetTrigger("Hit"); _anim.SetTrigger("Attack"); } }
    public void PlayHit()   { if (!_dead && _anim){ _anim.ResetTrigger("Attack"); _anim.SetTrigger("Hit"); } }
    public void PlayShield(){ if (!_dead && _anim){ _anim.SetTrigger("Shield"); } }

    public void PlayDie() {
        if (_dead) return;
        _dead = true;

        // QUAN TRỌNG: Tắt Collider ngay lập tức để súng xuyên qua xác
        if (_col) _col.enabled = false; 

        if (_anim) {
            _anim.ResetTrigger("Hit");
            _anim.ResetTrigger("Attack");
            _anim.SetTrigger("Die");
        }

        if (_rb) {
            // Cho phép vật lý hoạt động để ngã xuống
            _rb.isKinematic = false; 
            _rb.useGravity = true;
            
            // Tắt mọi ràng buộc để xác rơi tự do
            _rb.constraints = RigidbodyConstraints.None; 
            
            // Đẩy nhẹ xác lên trên và ra sau để tạo cảm giác trúng đạn
            _rb.AddForce(Vector3.up * _deathKnockForce + Vector3.back * 2f, ForceMode.Impulse);
        }

        OnDied();
    }

    // ===== Helpers =====
    bool TryGetBullet(Component c, out Bullet b){
        b = c.GetComponent<Bullet>();
        if (b == null) b = c.GetComponentInParent<Bullet>();
        if (b == null) b = c.GetComponentInChildren<Bullet>();
        return b != null;
    }

    void OnHitByBullet(Vector3 hitPoint) {
        if (_dead) return; // Nếu đã chết thì không xử lý gì thêm

        if (_hitFx) { 
            var fx = Instantiate(_hitFx, hitPoint, Quaternion.identity); 
            Destroy(fx, 1.2f); 
        }

        // Tắt Slow-mo ngay khi trúng đạn
        if (GameManager.Instance) GameManager.Instance.ToggleSlowMo(false);

        if (_hasShield) {
            _hasShield = false;
            PlayShield();
            if (_shieldBreakFx) { 
                var sh = Instantiate(_shieldBreakFx, transform.position, transform.rotation); 
                Destroy(sh, 1.5f); 
            }
            return;
        }
        PlayDie();
    }

    void OnDied() {
        if (GameManager.Instance) {
            GameManager.Instance.AddScore(_scoreValue);
            GameManager.Instance.NotifyEnemyKilled();
            
            // Đảm bảo tắt Slow-mo một lần nữa khi thực sự chết
            GameManager.Instance.ToggleSlowMo(false);
        }
        if (_deathFx) { 
            var fx = Instantiate(_deathFx, transform.position, transform.rotation); 
            Destroy(fx, 2f); 
        }
        
        // Bắt đầu làm mờ xác rồi mới hủy để tránh biến mất đột ngột
        StartCoroutine(FadeOutAndDestroy());
    }

    IEnumerator FadeOutAndDestroy() {
        yield return new WaitForSeconds(0.1f); // Đợi 0.1 giây sau khi ngã

        Renderer[] rs = GetComponentsInChildren<Renderer>();
        float elapsed = 0;
        float duration = 1f;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            foreach (var r in rs) {
                // Chỉ hoạt động nếu Material dùng Shader hỗ trợ Transparent/Fade
                if (r.material.HasProperty("_Color")) {
                    Color c = r.material.color;
                    c.a = alpha;
                    r.material.color = c;
                }
            }
            yield return null;
        }
        Destroy(gameObject);
    }
}
