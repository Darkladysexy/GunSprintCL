using System;
using GunSprint.Data;
using GunSprint.Events;
using UnityEngine;

namespace GunSprint.Gameplay
{
    /// <summary>
    /// Coordinates the gun: turns a "fire" input into a shot plus recoil when the run allows it,
    /// and reports when the gun falls off the track. Shooting and physics live in
    /// <see cref="GunShooter"/> and <see cref="GunMotor"/> (single responsibility).
    /// </summary>
    [RequireComponent(typeof(GunMotor), typeof(GunShooter))]
    [DisallowMultipleComponent]
    public class GunController : MonoBehaviour
    {
        [SerializeField] private GunConfigSO _config;
        [SerializeField] private RunStateSO _runState;
        [SerializeField] private VoidEventChannelSO _firePressed;

        [Tooltip("Below this speed (m/s) the gun counts as resting.")]
        [SerializeField, Min(0f)] private float _restingSpeed = 0.1f;

        private GunMotor _motor;
        private GunShooter _shooter;
        private bool _hasFallen;

        public event Action Fired;
        public event Action Fell;

        public GunShooter Shooter => _shooter;

        public bool IsResting => _motor.Body.linearVelocity.sqrMagnitude < _restingSpeed * _restingSpeed;

        public float LastShotTime { get; private set; }

        private void Awake()
        {
            _motor = GetComponent<GunMotor>();
            _shooter = GetComponent<GunShooter>();
        }

        private void OnEnable()
        {
            _firePressed.OnEventRaised += HandleFirePressed;
        }

        private void OnDisable()
        {
            _firePressed.OnEventRaised -= HandleFirePressed;
        }

        private void Update()
        {
            if (!_hasFallen && _runState.IsPlaying && transform.position.y < _config.KillHeight)
            {
                _hasFallen = true;
                Fell?.Invoke();
            }
        }

        public void PlaceAt(Vector3 position, Quaternion rotation)
        {
            _hasFallen = false;
            _motor.Teleport(position, rotation);
        }

        private void HandleFirePressed()
        {
            if (!_runState.IsPlaying || !_runState.TryConsumeAmmo())
            {
                return;
            }

            _shooter.Fire();
            _motor.ApplyRecoil(_shooter.MuzzleForward);
            LastShotTime = Time.time;
            Fired?.Invoke();
        }
    }
}
