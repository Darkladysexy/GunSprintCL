using UnityEngine;

public class GunSprint : MonoBehaviour
{
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

    [Header("Out of bounds (NEW)")]
    [SerializeField] private float _killY = -5f; // rơi dưới mức này thì thua (Infinity)

    private Rigidbody _rb;
    private float _lastFired;
    private Collider _gunCollider;
    private bool _isTouchingFloor = false;

    private void Awake(){ _rb = GetComponent<Rigidbody>(); _gunCollider = GetComponent<Collider>(); }

    private void Update()
    {
        if (GameManager.Instance && GameManager.Instance.isPaused) return;

        // NEW: báo rơi map trong Infinity
        if (transform.position.y < _killY)
            {
                if (GameManager.Instance != null)
                {
                    if (GameManager.Instance.CurrentMode == GameMode.Infinity)
                    {
                        GameManager.Instance.NotifyGunLost();
                    }
                    else // Chế độ Levels
                    {
                        GameManager.Instance.GameOver();
                    }
                }
            }

        _rb.angularVelocity = new Vector3(0, 0, Mathf.Clamp(_rb.angularVelocity.z, -_maxAngularVelocity, _maxAngularVelocity));

        if (Input.GetMouseButtonDown(0))
        {
            // NEW: trừ đạn khi Infinity
            if (GameManager.Instance && GameManager.Instance.CurrentMode == GameMode.Infinity)
                if (!GameManager.Instance.TryConsumeAmmo(1)) return;

            var hitsTarget = Physics.Raycast(_spawnPoint.position, _spawnPoint.forward, float.PositiveInfinity, _targetLayer);
            if (hitsTarget && GameManager.Instance) GameManager.Instance.ToggleSlowMo(true);

            var bullet = Instantiate(_bulletPrefab, _spawnPoint.position, _spawnPoint.rotation);
            Debug.Log("Ban thanh cong");
            // NEW: cộng power theo upgrade
            float powerMul = GameManager.Instance ? GameManager.Instance.GunPowerMultiplier() : 1f;
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
