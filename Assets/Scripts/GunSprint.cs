using UnityEngine;

public class GunSprint : MonoBehaviour
{
    // ... (Giữ nguyên các SerializeField cũ) ...
    [SerializeField] private Bullet _bulletPrefab;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private ParticleSystem _smokeSystem;
    [SerializeField] private LayerMask _targetLayer;
    [SerializeField] private Animator _gunAnimator;

    [Header("Physics and Movement")]
    [SerializeField] private float _bulletSpeed = 12;
    [SerializeField] private float _torque = 120;
    [SerializeField] private float _maxTorqueBonus = 150;
    [SerializeField] private float _maxAngularVelocity = 10;
    [SerializeField] private float _forceAmount = 600;
    [SerializeField] private float _maxUpAssist = 30;
    [SerializeField] private float _smokeLength = 1f;
    [SerializeField] private float _maxY = 10;
    [SerializeField] private float _floorBounceForce = 1f;

    [Header("Out of bounds")]
    [SerializeField] private float _killY = -10f; 

    private Rigidbody _rb;
    private float _lastFired;
    private Collider _gunCollider;
    private bool _isTouchingFloor = false;
    
    // Biến để lưu quãng đường xa nhất đạt được trong lượt này
    private float _maxDistanceReached = 0f;
    private float _startPosX;

    private void Awake()
    { 
        _rb = GetComponent<Rigidbody>(); 
        _gunCollider = GetComponent<Collider>();
        _startPosX = transform.position.x;
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.isPaused) return;

        // 1. XỬ LÝ TÍNH MÉT LIVE
        // Thay vì dùng Mathf.Abs, ta dùng dấu trừ phía trước position.x
        // Vì súng đi về hướng X âm, nên -position.x sẽ ra số dương
        float currentProgress = -transform.position.x;
        
        // Sử dụng Mathf.Max để đảm bảo:
        // - Nếu đi lùi (X dương), currentProgress sẽ âm -> không cập nhật _maxDistanceReached
        // - Quãng đường chỉ có tăng, không giảm khi súng bị nảy ngược lại
        _maxDistanceReached = Mathf.Max(_maxDistanceReached, currentProgress);

        if (GameManager.Instance.CurrentMode == GameMode.Infinity)
        {
            GameManager.Instance.UpdateMetersUI(_maxDistanceReached);
        }

        // 2. XỬ LÝ LOSE / FINISH
        if (transform.position.y < _killY)
        {
            if (GameManager.Instance.CurrentMode == GameMode.Infinity)
            {
                // Truyền giá trị lớn nhất đạt được vào màn hình kết thúc
                GameManager.Instance.ShowDoneScreen(_maxDistanceReached);
            }
            else
            {
                GameManager.Instance.GameOver();
            }
            return; 
        }

        // Kiểm tra hết đạn trong chế độ Infinity
        if (GameManager.Instance.CurrentMode == GameMode.Infinity && GameManager.Instance.Ammo <= 0)
        {
            // Nếu súng đã dừng hẳn (hoặc rơi) và hết đạn thì mới kết thúc
            if (_rb.linearVelocity.magnitude < 0.1f) 
            {
                GameManager.Instance.ShowDoneScreen(_maxDistanceReached);
            }
        }

        // ... (Phần xử lý bắn súng và vật lý giữ nguyên bên dưới) ...
        _rb.angularVelocity = new Vector3(0, 0, Mathf.Clamp(_rb.angularVelocity.z, -_maxAngularVelocity, _maxAngularVelocity));

        if (Input.GetMouseButtonDown(0))
        {
            if (GameManager.Instance.CurrentMode == GameMode.Infinity)
                if (!GameManager.Instance.TryConsumeAmmo(1)) return;

            var hitsTarget = Physics.Raycast(_spawnPoint.position, _spawnPoint.forward, float.PositiveInfinity, _targetLayer);
            if (hitsTarget) GameManager.Instance.ToggleSlowMo(true);

            var bullet = Instantiate(_bulletPrefab, _spawnPoint.position, _spawnPoint.rotation);
            float powerMul = GameManager.Instance.GunPowerMultiplier();
            bullet.Init(_spawnPoint.forward * (_bulletSpeed * powerMul), _gunCollider);

            _smokeSystem.Play();
            _lastFired = Time.time;
            if (_gunAnimator != null) _gunAnimator.SetTrigger("Recoil");

            _rb.AddForce(-transform.forward * _forceAmount);

            if (_isTouchingFloor)
            {
                _rb.AddForce(Vector3.up * _floorBounceForce, ForceMode.Impulse);
                var assistPoint = Mathf.InverseLerp(0, _maxY, _rb.position.y);
                var assistAmount = Mathf.Lerp(_maxUpAssist, 0, assistPoint);
                _rb.AddForce(Vector3.up * assistAmount);
            }

            var angularPoint = Mathf.InverseLerp(0, _maxAngularVelocity, Mathf.Abs(_rb.angularVelocity.z));
            var amount = Mathf.Lerp(0, _maxTorqueBonus, angularPoint);
            var torque = _torque + amount;
            var dir = Vector3.Dot(_spawnPoint.forward, Vector3.right) < 0 ? Vector3.back : Vector3.forward;
            _rb.AddTorque(dir * torque);
        }

        if (_smokeSystem.isPlaying && _lastFired + _smokeLength < Time.time)
            _smokeSystem.Stop();
    }

    // ... (Giữ nguyên OnCollisionEnter và OnCollisionExit) ...
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            _isTouchingFloor = true;
            var bounceForce = collision.relativeVelocity.magnitude * 0.2f;
            _rb.AddForce(Vector3.up * bounceForce, ForceMode.Impulse);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
            _isTouchingFloor = false;
    }
}