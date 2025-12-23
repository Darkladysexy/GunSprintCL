using UnityEngine;

public class GunSprint : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Bullet _bulletPrefab;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private ParticleSystem _smokeSystem;
    [SerializeField] private LayerMask _targetLayer;
    [SerializeField] private LayerMask _obstacleLayer; 
    [SerializeField] private Animator _gunAnimator;

    [Header("Audio")]
    [SerializeField] private AudioSource _gunAudioSource;
    [SerializeField] private AudioClip _shootSfx;
    [SerializeField] private AudioClip _bounceSfx;

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
    
    private float _maxDistanceReached = 0f;

    private void Awake()
    { 
        _rb = GetComponent<Rigidbody>(); 
        _gunCollider = GetComponent<Collider>();
    }

    private void Update()
    {
        // 1. PHÍM TẮT HỆ THỐNG
        if (Input.GetKeyDown(KeyCode.R)) {
            GameManager.Instance.Replay(); // Chơi lại màn hiện tại
            return;
        }
        if (Input.GetKeyDown(KeyCode.N)) {
            GameManager.Instance.ResetProgress(); // Xóa sạch dữ liệu
            return;
        }

        if (GameManager.Instance == null || GameManager.Instance.isPaused) return;

        // 2. XỬ LÝ TÍNH MÉT LIVE
        float currentProgress = -transform.position.x;
        _maxDistanceReached = Mathf.Max(_maxDistanceReached, currentProgress);

        if (GameManager.Instance.CurrentMode == GameMode.Infinity)
        {
            GameManager.Instance.UpdateMetersUI(_maxDistanceReached);
        }

        // 3. XỬ LÝ LOSE / FINISH
        if (transform.position.y < _killY)
        {
            if (GameManager.Instance.CurrentMode == GameMode.Infinity)
            {
                GameManager.Instance.ShowDoneScreen(_maxDistanceReached);
            }
            else
            {
                GameManager.Instance.GameOver();
            }
            return; 
        }

        if (GameManager.Instance.CurrentMode == GameMode.Infinity && GameManager.Instance.Ammo <= 0)
        {
            if (_rb.linearVelocity.magnitude < 0.1f) 
            {
                GameManager.Instance.ShowDoneScreen(_maxDistanceReached);
            }
        }

        _rb.angularVelocity = new Vector3(0, 0, Mathf.Clamp(_rb.angularVelocity.z, -_maxAngularVelocity, _maxAngularVelocity));

        // 4. XỬ LÝ BẮN SÚNG
        if (Input.GetMouseButtonDown(0))
        {
            if (GameManager.Instance.CurrentMode == GameMode.Infinity)
                if (!GameManager.Instance.TryConsumeAmmo(1)) return;

            if (_gunAudioSource && _shootSfx) _gunAudioSource.PlayOneShot(_shootSfx);

            // SỬA LỖI SLOW-MO: Kiểm tra xem kẻ địch có bị vật cản che khuất không
            LayerMask combinedMask = _targetLayer | _obstacleLayer;
            if (Physics.Raycast(_spawnPoint.position, _spawnPoint.forward, out RaycastHit hit, float.PositiveInfinity, combinedMask))
            {
                // Chỉ bật Slow-mo nếu vật thể đầu tiên tia ngắm chạm vào là Enemy
                if (((1 << hit.collider.gameObject.layer) & _targetLayer) != 0)
                {
                    GameManager.Instance.ToggleSlowMo(true);
                }
            }

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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            _isTouchingFloor = true;
            if (_gunAudioSource && _bounceSfx) _gunAudioSource.PlayOneShot(_bounceSfx);
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