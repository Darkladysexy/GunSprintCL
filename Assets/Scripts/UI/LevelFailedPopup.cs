using GunSprint.Core;
using GunSprint.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GunSprint.UI
{
    /// <summary>
    /// Levels mode defeat: retry the same level or go home.
    /// </summary>
    public class LevelFailedPopup : UIPopup
    {
        [SerializeField] private RunStateSO _runState;
        [SerializeField] private TMP_Text _levelLabel;
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _homeButton;

        protected override void Awake()
        {
            base.Awake();
            _retryButton.onClick.AddListener(SceneLoader.ReloadActiveScene);
            _homeButton.onClick.AddListener(SceneLoader.LoadMainMenu);
        }

        private void OnDestroy()
        {
            _retryButton.onClick.RemoveListener(SceneLoader.ReloadActiveScene);
            _homeButton.onClick.RemoveListener(SceneLoader.LoadMainMenu);
        }

        protected override void OnShowing()
        {
            _levelLabel.SetText("LEVEL {0}", _runState.LevelNumber);
        }
    }
}
