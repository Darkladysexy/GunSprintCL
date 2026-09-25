using GunSprint.Core;
using GunSprint.Data;
using GunSprint.Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GunSprint.UI
{
    /// <summary>
    /// In-game heads-up display. Shows only what the current mode needs and redraws a label only when
    /// its value changes (no per-frame string building, no needless canvas rebuilds).
    /// </summary>
    public class HudView : MonoBehaviour
    {
        [SerializeField] private RunStateSO _runState;
        [SerializeField] private VoidEventChannelSO _pauseRequested;
        [SerializeField] private VoidEventChannelSO _firePressed;
        [SerializeField] private Button _pauseButton;

        [Header("Levels mode")]
        [SerializeField] private GameObject _levelGroup;
        [SerializeField] private TMP_Text _levelLabel;
        [SerializeField] private TMP_Text _enemiesLabel;

        [Header("Infinity mode")]
        [SerializeField] private GameObject _distanceGroup;
        [SerializeField] private TMP_Text _distanceLabel;
        [SerializeField] private TMP_Text _bestLabel;
        [SerializeField] private GameObject _newBestBadge;
        [SerializeField] private GameObject _ammoGroup;
        [SerializeField] private TMP_Text _ammoLabel;
        [SerializeField] private Color _ammoColor = Color.white;
        [SerializeField] private Color _lowAmmoColor = new Color(1f, 0.35f, 0.3f);
        [SerializeField, Min(0)] private int _lowAmmoThreshold = 2;

        [Header("Onboarding")]
        [Tooltip("Shown until the first shot.")]
        [SerializeField] private GameObject _tapHint;

        private void Awake()
        {
            _pauseButton.onClick.AddListener(_pauseRequested.RaiseEvent);
        }

        private void OnDestroy()
        {
            _pauseButton.onClick.RemoveListener(_pauseRequested.RaiseEvent);
        }

        private void OnEnable()
        {
            _runState.StateChanged += HandleStateChanged;
            _runState.AmmoChanged += HandleAmmoChanged;
            _runState.DistanceChanged += HandleDistanceChanged;
            _runState.EnemiesRemainingChanged += HandleEnemiesRemainingChanged;
            _runState.BestDistanceBeaten += HandleBestDistanceBeaten;
            _firePressed.OnEventRaised += HideTapHint;
            Refresh();
        }

        private void OnDisable()
        {
            _runState.StateChanged -= HandleStateChanged;
            _runState.AmmoChanged -= HandleAmmoChanged;
            _runState.DistanceChanged -= HandleDistanceChanged;
            _runState.EnemiesRemainingChanged -= HandleEnemiesRemainingChanged;
            _runState.BestDistanceBeaten -= HandleBestDistanceBeaten;
            _firePressed.OnEventRaised -= HideTapHint;
        }

        private void Refresh()
        {
            bool isInfinity = _runState.Mode == GameMode.Infinity;
            _levelGroup.SetActive(!isInfinity);
            _distanceGroup.SetActive(isInfinity);
            _ammoGroup.SetActive(isInfinity);

            _levelLabel.SetText("LEVEL {0}", _runState.LevelNumber);
            HandleEnemiesRemainingChanged(_runState.EnemiesRemaining);
            HandleDistanceChanged(_runState.DistanceMeters);
            HandleAmmoChanged(_runState.Ammo);

            bool hasRecord = _runState.BestDistance > 0f;
            _bestLabel.gameObject.SetActive(hasRecord && !_runState.HasBeatenBest);
            _bestLabel.SetText("BEST {0} m", Mathf.FloorToInt(_runState.BestDistance));
            _newBestBadge.SetActive(_runState.HasBeatenBest);

            HandleStateChanged(_runState.State);
        }

        private void HandleStateChanged(GameState state)
        {
            _pauseButton.interactable = state == GameState.Playing;
        }

        private void HandleAmmoChanged(int ammo)
        {
            _ammoLabel.SetText("x{0}", ammo);
            _ammoLabel.color = ammo <= _lowAmmoThreshold ? _lowAmmoColor : _ammoColor;
        }

        private void HandleDistanceChanged(int meters)
        {
            _distanceLabel.SetText("{0} m", meters);
        }

        private void HandleEnemiesRemainingChanged(int remaining)
        {
            _enemiesLabel.SetText("x{0}", remaining);
        }

        private void HandleBestDistanceBeaten()
        {
            _newBestBadge.SetActive(true);
            _bestLabel.gameObject.SetActive(false);
        }

        private void HideTapHint()
        {
            if (_tapHint != null && _tapHint.activeSelf)
            {
                _tapHint.SetActive(false);
            }
        }
    }
}
