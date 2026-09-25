using GunSprint.Core;
using GunSprint.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GunSprint.UI
{
    /// <summary>
    /// Infinity mode results: distance, record, coins earned and the upgrade shop.
    /// </summary>
    public class RunSummaryPopup : UIPopup
    {
        [SerializeField] private RunStateSO _runState;
        [SerializeField] private TMP_Text _distanceLabel;
        [SerializeField] private TMP_Text _bestLabel;
        [SerializeField] private GameObject _newBestBadge;
        [SerializeField] private TMP_Text _coinsEarnedLabel;
        [SerializeField] private Button _playAgainButton;
        [SerializeField] private Button _homeButton;
        [SerializeField, Min(0f)] private float _countUpDuration = 0.8f;

        protected override void Awake()
        {
            base.Awake();
            _playAgainButton.onClick.AddListener(SceneLoader.ReloadActiveScene);
            _homeButton.onClick.AddListener(SceneLoader.LoadMainMenu);
        }

        private void OnDestroy()
        {
            _playAgainButton.onClick.RemoveListener(SceneLoader.ReloadActiveScene);
            _homeButton.onClick.RemoveListener(SceneLoader.LoadMainMenu);
        }

        protected override void OnShowing()
        {
            int meters = _runState.DistanceMeters;
            _newBestBadge.SetActive(_runState.IsNewBest);
            _bestLabel.gameObject.SetActive(!_runState.IsNewBest && _runState.BestDistance > 0f);
            _bestLabel.SetText("BEST {0} m", Mathf.FloorToInt(_runState.BestDistance));
            _coinsEarnedLabel.SetText("+{0}", _runState.CoinsEarned);

            _distanceLabel.SetText("0 m");
            StartCoroutine(UIAnimations.CountUp(_distanceLabel, "{0} m", 0, meters, _countUpDuration));
        }
    }
}
