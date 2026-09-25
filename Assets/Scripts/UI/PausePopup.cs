using GunSprint.Core;
using GunSprint.Events;
using UnityEngine;
using UnityEngine.UI;

namespace GunSprint.UI
{
    /// <summary>
    /// Pause menu: resume, restart or go home. The sound toggle is a separate SoundToggleButton.
    /// </summary>
    public class PausePopup : UIPopup
    {
        [SerializeField] private VoidEventChannelSO _resumeRequested;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _homeButton;

        protected override void Awake()
        {
            base.Awake();
            _resumeButton.onClick.AddListener(_resumeRequested.RaiseEvent);
            _restartButton.onClick.AddListener(SceneLoader.ReloadActiveScene);
            _homeButton.onClick.AddListener(SceneLoader.LoadMainMenu);
        }

        private void OnDestroy()
        {
            _resumeButton.onClick.RemoveListener(_resumeRequested.RaiseEvent);
            _restartButton.onClick.RemoveListener(SceneLoader.ReloadActiveScene);
            _homeButton.onClick.RemoveListener(SceneLoader.LoadMainMenu);
        }
    }
}
