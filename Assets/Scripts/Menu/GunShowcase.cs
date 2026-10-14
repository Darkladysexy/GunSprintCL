using GunSprint.Data;
using GunSprint.Events;
using GunSprint.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace GunSprint.Menu
{
    /// <summary>
    /// The 3D gun on the main menu. Shows the equipped gun (or the one browsed in the shop) in a
    /// straight side view. The player can drag to turn it; it eases back to the side view when released.
    /// </summary>
    public class GunShowcase : MonoBehaviour
    {
        [SerializeField] private PlayerProgressSO _progress;
        [SerializeField] private GunCatalogSO _catalog;

        [Header("Events")]
        [Tooltip("Raised by the shop with the gun to preview.")]
        [SerializeField] private GunEventChannelSO _previewRequested;

        [SerializeField] private VoidEventChannelSO _shopClosed;

        [Header("Framing")]
        [Tooltip("Length of the gun on screen, in world units.")]
        [SerializeField, Min(0.1f)] private float _displayLength = 0.8f;

        [Tooltip("Rest pose. (0, 90, 0) shows the gun from the side, barrel to the right.")]
        [SerializeField] private Vector3 _restRotation = new Vector3(0f, 90f, 0f);

        [Tooltip("Offset while the shop is open, so the gun sits above the shop panel.")]
        [SerializeField] private Vector3 _shopOffset = new Vector3(0f, 0.42f, 0f);

        [SerializeField, Min(0.1f)] private float _moveSharpness = 8f;

        [Header("Drag")]
        [Tooltip("Degrees turned by dragging across the full screen width.")]
        [SerializeField, Min(0f)] private float _dragDegrees = 400f;

        [SerializeField, Range(0f, 89f)] private float _maxPitch = 40f;

        [Tooltip("How fast the spin from a flick slows down.")]
        [SerializeField, Min(0f)] private float _spinDamping = 4f;

        [Tooltip("Seconds after the last touch before the gun returns to the rest pose.")]
        [SerializeField, Min(0f)] private float _returnDelay = 2f;

        [SerializeField, Min(0.1f)] private float _returnSharpness = 4f;

        [Header("Idle")]
        [SerializeField, Min(0f)] private float _bobHeight = 0.025f;
        [SerializeField, Min(0f)] private float _bobFrequency = 0.5f;

        private Transform _model;
        private GunDefinitionSO _shownGun;
        private Vector3 _homePosition;
        private Vector3 _targetOffset;
        private Vector3 _offset;
        private float _yaw;
        private float _pitch;
        private float _yawVelocity;
        private float _idleTime;
        private bool _isDragging;
        private Vector2 _lastPointer;
        private float _popTimer;

        private void Awake()
        {
            _homePosition = transform.localPosition;
        }

        private void OnEnable()
        {
            _previewRequested.OnEventRaised += HandlePreviewRequested;
            _shopClosed.OnEventRaised += HandleShopClosed;
        }

        private void OnDisable()
        {
            _previewRequested.OnEventRaised -= HandlePreviewRequested;
            _shopClosed.OnEventRaised -= HandleShopClosed;
        }

        private void Start()
        {
            Show(_catalog.GetEquipped(_progress));
            _popTimer = 0f;
        }

        private void Update()
        {
            float deltaTime = Time.unscaledDeltaTime;
            HandleDrag(deltaTime);

            if (!_isDragging)
            {
                _yaw += _yawVelocity * deltaTime;
                _yawVelocity = Mathf.Lerp(_yawVelocity, 0f, 1f - Mathf.Exp(-_spinDamping * deltaTime));
                _idleTime += deltaTime;
                if (_idleTime > _returnDelay)
                {
                    // Return the shortest way round to the side view.
                    float blend = 1f - Mathf.Exp(-_returnSharpness * deltaTime);
                    _yaw = Mathf.LerpAngle(_yaw, 0f, blend);
                    _pitch = Mathf.Lerp(_pitch, 0f, blend);
                    _yawVelocity = 0f;
                }
            }

            transform.localRotation = Quaternion.AngleAxis(_yaw, Vector3.up)
                * Quaternion.AngleAxis(_pitch, Vector3.right)
                * Quaternion.Euler(_restRotation);

            _offset = Vector3.Lerp(_offset, _targetOffset, 1f - Mathf.Exp(-_moveSharpness * deltaTime));
            float bob = Mathf.Sin(Time.unscaledTime * _bobFrequency * 2f * Mathf.PI) * _bobHeight;
            transform.localPosition = _homePosition + _offset + Vector3.up * bob;

            if (_popTimer > 0f && _model != null)
            {
                // Small pop when the gun changes.
                _popTimer = Mathf.Max(0f, _popTimer - deltaTime / 0.25f);
                float scale = 1f - 0.25f * Mathf.Sin(_popTimer * Mathf.PI) * _popTimer;
                transform.localScale = Vector3.one * scale;
            }
        }

        private void HandleDrag(float deltaTime)
        {
            Pointer pointer = Pointer.current;
            if (pointer == null)
            {
                return;
            }

            Vector2 position = pointer.position.ReadValue();
            if (pointer.press.wasPressedThisFrame)
            {
                // Touches that start on a button or panel belong to the UI.
                EventSystem eventSystem = EventSystem.current;
                _isDragging = eventSystem == null || !eventSystem.IsPointerOverGameObject();
                _lastPointer = position;
            }

            if (_isDragging && pointer.press.isPressed)
            {
                Vector2 delta = (position - _lastPointer) / Mathf.Max(1f, Screen.width);
                _lastPointer = position;
                float yawStep = -delta.x * _dragDegrees;
                _yaw += yawStep;
                _pitch = Mathf.Clamp(_pitch + delta.y * _dragDegrees, -_maxPitch, _maxPitch);
                _yawVelocity = deltaTime > 0f ? yawStep / deltaTime : 0f;
                _idleTime = 0f;
            }
            else
            {
                _isDragging = false;
            }
        }

        private void HandlePreviewRequested(GunDefinitionSO gun)
        {
            _targetOffset = _shopOffset;
            Show(gun);
        }

        private void HandleShopClosed()
        {
            _targetOffset = Vector3.zero;
            Show(_catalog.GetEquipped(_progress));
        }

        private void Show(GunDefinitionSO gun)
        {
            if (gun == null || gun == _shownGun || gun.ModelPrefab == null)
            {
                return;
            }

            _shownGun = gun;
            if (_model != null)
            {
                Destroy(_model.gameObject);
            }

            // Build and fit the model in the rest pose, so its size does not depend on the current spin.
            Quaternion rotation = transform.localRotation;
            Vector3 scale = transform.localScale;
            transform.localRotation = Quaternion.Euler(_restRotation);
            transform.localScale = Vector3.one;

            var holder = new GameObject("Model").transform;
            holder.SetParent(transform, false);
            Transform model = GunModelFitter.Spawn(gun.ModelPrefab, holder, gun.ModelRotation);
            GunModelFitter.Fit(model, transform, _displayLength, Vector3.zero);
            _model = holder;

            transform.localRotation = rotation;
            transform.localScale = scale;
            _popTimer = 1f;
        }
    }
}
