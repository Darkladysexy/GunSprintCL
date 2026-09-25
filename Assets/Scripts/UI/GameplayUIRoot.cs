using System.Collections;
using GunSprint.Core;
using GunSprint.Data;
using GunSprint.Events;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GunSprint.UI
{
    /// <summary>
    /// Screen router for the gameplay UI: shows the popup matching the run state and handles the
    /// Android back button / Escape. It only talks to ScriptableObject assets, so the UI prefab has
    /// no reference to any scene object and can be opened and tested on its own.
    /// </summary>
    public class GameplayUIRoot : MonoBehaviour
    {
        [SerializeField] private RunStateSO _runState;
        [SerializeField] private VoidEventChannelSO _pauseRequested;
        [SerializeField] private VoidEventChannelSO _resumeRequested;

        [Header("Screens")]
        [SerializeField] private UIPopup _pausePopup;
        [SerializeField] private UIPopup _levelCompletePopup;
        [SerializeField] private UIPopup _levelFailedPopup;
        [SerializeField] private UIPopup _runSummaryPopup;

        [Tooltip("Real-time delay before a result popup appears, so the player sees the final moment.")]
        [SerializeField, Min(0f)] private float _resultPopupDelay = 0.9f;

        private Coroutine _pendingResult;

        private void Awake()
        {
            _pausePopup.HideImmediate();
            _levelCompletePopup.HideImmediate();
            _levelFailedPopup.HideImmediate();
            _runSummaryPopup.HideImmediate();
        }

        private void OnEnable()
        {
            _runState.StateChanged += HandleStateChanged;
            HandleStateChanged(_runState.State);
        }

        private void OnDisable()
        {
            _runState.StateChanged -= HandleStateChanged;
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame)
            {
                return;
            }

            // Escape is also the Android back button.
            if (_runState.State == GameState.Playing)
            {
                _pauseRequested.RaiseEvent();
            }
            else if (_runState.State == GameState.Paused)
            {
                _resumeRequested.RaiseEvent();
            }
        }

        private void HandleStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.Playing:
                    _pausePopup.Hide();
                    break;
                case GameState.Paused:
                    _pausePopup.Show();
                    break;
                case GameState.LevelComplete:
                    ShowResult(_levelCompletePopup);
                    break;
                case GameState.LevelFailed:
                    ShowResult(_levelFailedPopup);
                    break;
                case GameState.RunFinished:
                    ShowResult(_runSummaryPopup);
                    break;
                default:
                    Debug.LogWarning($"[GameplayUIRoot] Unhandled state {state}.", this);
                    break;
            }
        }

        private void ShowResult(UIPopup popup)
        {
            _pausePopup.Hide();
            if (_pendingResult != null)
            {
                StopCoroutine(_pendingResult);
            }

            _pendingResult = StartCoroutine(ShowAfterDelay(popup));
        }

        private IEnumerator ShowAfterDelay(UIPopup popup)
        {
            yield return new WaitForSecondsRealtime(_resultPopupDelay);
            _pendingResult = null;
            popup.Show();
        }
    }
}
