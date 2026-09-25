using GunSprint.Audio;
using GunSprint.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GunSprint.UI
{
    /// <summary>
    /// One upgrade in the shop: shows its level, current value and price, and buys it on click.
    /// The buy button is disabled when the player cannot afford it, so a tap is never "wasted".
    /// </summary>
    public class UpgradeCardView : MonoBehaviour
    {
        [SerializeField] private UpgradeSO _upgrade;
        [SerializeField] private PlayerProgressSO _progress;

        [Header("Widgets")]
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _nameLabel;
        [SerializeField] private TMP_Text _levelLabel;
        [SerializeField] private TMP_Text _valueLabel;
        [SerializeField] private TMP_Text _costLabel;
        [SerializeField] private Button _buyButton;

        [Header("Feedback")]
        [SerializeField] private AudioCueSO _purchaseCue;

        private Vector3 _baseScale;
        private Coroutine _punch;

        private void Awake()
        {
            _baseScale = transform.localScale;
            _buyButton.onClick.AddListener(Buy);
        }

        private void OnDestroy()
        {
            _buyButton.onClick.RemoveListener(Buy);
        }

        private void OnEnable()
        {
            _progress.CoinsChanged += HandleCoinsChanged;
            Refresh();
        }

        private void OnDisable()
        {
            _progress.CoinsChanged -= HandleCoinsChanged;
            transform.localScale = _baseScale;
        }

        private void HandleCoinsChanged(int coins)
        {
            Refresh();
        }

        private void Refresh()
        {
            int level = _progress.GetUpgradeLevel(_upgrade);
            if (_upgrade.Icon != null)
            {
                _icon.sprite = _upgrade.Icon;
            }

            _nameLabel.SetText(_upgrade.DisplayName);
            _levelLabel.SetText("LV {0}", level + 1);
            _valueLabel.SetText(_upgrade.FormatValue(level));

            bool isMax = _upgrade.IsMaxLevel(level);
            if (isMax)
            {
                _costLabel.SetText("MAX");
            }
            else
            {
                _costLabel.SetText("{0}", _upgrade.GetCost(level));
            }

            _buyButton.interactable = _progress.CanPurchaseUpgrade(_upgrade);
        }

        private void Buy()
        {
            if (!_progress.TryPurchaseUpgrade(_upgrade))
            {
                return;
            }

            if (_purchaseCue != null)
            {
                _purchaseCue.Play();
            }

            Refresh();
            if (_punch != null)
            {
                StopCoroutine(_punch);
            }

            _punch = StartCoroutine(UIAnimations.Punch(transform, _baseScale, 0.12f, 0.25f));
        }
    }
}
