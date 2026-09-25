using GunSprint.Core;
using GunSprint.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GunSprint.UI
{
    /// <summary>
    /// Levels mode victory: shows the reward with its multiplier and leads to the next level.
    /// </summary>
    public class LevelCompletePopup : UIPopup
    {
        [SerializeField] private RunStateSO _runState;
        [SerializeField] private TMP_Text _titleLabel;
        [SerializeField] private TMP_Text _coinsLabel;
        [SerializeField] private TMP_Text _multiplierLabel;
        [SerializeField] private Button _nextButton;
        [SerializeField, Min(0f)] private float _countUpDuration = 0.8f;

        protected override void Awake()
        {
            base.Awake();
            _nextButton.onClick.AddListener(SceneLoader.ReloadActiveScene);
        }

        private void OnDestroy()
        {
            _nextButton.onClick.RemoveListener(SceneLoader.ReloadActiveScene);
        }

        protected override void OnShowing()
        {
            _titleLabel.SetText("LEVEL {0}", _runState.LevelNumber);
            _multiplierLabel.SetText("x{0}", _runState.Multiplier);
            _coinsLabel.SetText("+0");
            StartCoroutine(UIAnimations.CountUp(_coinsLabel, "+{0}", 0, _runState.CoinsEarned, _countUpDuration));
        }
    }
}
