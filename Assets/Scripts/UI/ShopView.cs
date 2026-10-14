using System;
using GunSprint.Audio;
using GunSprint.Data;
using GunSprint.Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GunSprint.UI
{
    /// <summary>
    /// Gun shop on the main menu: browse the catalog, compare stats, buy and equip.
    /// The 3D preview is drawn by the scene's GunShowcase, driven through event channels,
    /// so this prefab has no reference to scene objects.
    /// </summary>
    public class ShopView : UIPopup
    {
        [Serializable]
        private struct StatBar
        {
            public GunStat Stat;

            [Tooltip("Left-anchored fill; its right anchor is set to the stat value.")]
            public RectTransform Fill;
        }

        [SerializeField] private PlayerProgressSO _progress;
        [SerializeField] private GunCatalogSO _catalog;

        [Header("Events")]
        [SerializeField] private GunEventChannelSO _previewRequested;
        [SerializeField] private VoidEventChannelSO _shopClosed;

        [Header("Widgets")]
        [SerializeField] private TMP_Text _nameLabel;
        [SerializeField] private TMP_Text _counterLabel;
        [SerializeField] private Button _previousButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private StatBar[] _statBars = new StatBar[0];

        [Header("Action button")]
        [SerializeField] private Button _actionButton;
        [SerializeField] private Image _actionImage;
        [SerializeField] private TMP_Text _actionLabel;
        [SerializeField] private GameObject _priceGroup;
        [SerializeField] private TMP_Text _priceLabel;
        [SerializeField] private Sprite _buySprite;
        [SerializeField] private Sprite _equipSprite;
        [SerializeField] private Sprite _equippedSprite;

        [Tooltip("Menu elements hidden while the shop is open.")]
        [SerializeField] private GameObject[] _hiddenWhileOpen = new GameObject[0];

        [Header("Feedback")]
        [SerializeField] private AudioCueSO _purchaseCue;

        private int _index;

        protected override void Awake()
        {
            base.Awake();
            _previousButton.onClick.AddListener(ShowPrevious);
            _nextButton.onClick.AddListener(ShowNext);
            _closeButton.onClick.AddListener(Close);
            _actionButton.onClick.AddListener(BuyOrEquip);
        }

        private void OnDestroy()
        {
            _previousButton.onClick.RemoveListener(ShowPrevious);
            _nextButton.onClick.RemoveListener(ShowNext);
            _closeButton.onClick.RemoveListener(Close);
            _actionButton.onClick.RemoveListener(BuyOrEquip);
        }

        private void OnEnable()
        {
            _progress.CoinsChanged += HandleCoinsChanged;
        }

        private void OnDisable()
        {
            _progress.CoinsChanged -= HandleCoinsChanged;
        }

        public void Open()
        {
            if (IsShown || _catalog.Count == 0)
            {
                return;
            }

            SetMenuVisible(false);
            _index = Mathf.Max(0, _catalog.IndexOf(_catalog.GetEquipped(_progress)));
            Show();
            Preview();
        }

        public void Close()
        {
            if (!IsShown)
            {
                return;
            }

            Hide();
            SetMenuVisible(true);
            _shopClosed.RaiseEvent();
        }

        protected override void OnShowing()
        {
            Refresh();
        }

        private void ShowPrevious()
        {
            _index = (_index - 1 + _catalog.Count) % _catalog.Count;
            Preview();
        }

        private void ShowNext()
        {
            _index = (_index + 1) % _catalog.Count;
            Preview();
        }

        private void Preview()
        {
            Refresh();
            _previewRequested.RaiseEvent(_catalog.Get(_index));
        }

        private void BuyOrEquip()
        {
            GunDefinitionSO gun = _catalog.Get(_index);
            if (!_catalog.IsOwned(gun, _progress))
            {
                if (!_progress.TryBuyGun(gun))
                {
                    return;
                }

                if (_purchaseCue != null)
                {
                    _purchaseCue.Play();
                }
            }

            // Buying also equips: that is what the player wants nine times out of ten.
            _progress.EquipGun(gun);
            Refresh();
        }

        private void HandleCoinsChanged(int coins)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (_catalog.Count == 0)
            {
                return;
            }

            GunDefinitionSO gun = _catalog.Get(_index);
            _nameLabel.SetText(gun.DisplayName);
            _counterLabel.SetText("{0}/{1}", _index + 1, _catalog.Count);

            foreach (StatBar bar in _statBars)
            {
                float value = gun.GetStat(bar.Stat) / _catalog.GetMaxStat(bar.Stat);
                bar.Fill.anchorMax = new Vector2(Mathf.Clamp(value, 0.08f, 1f), bar.Fill.anchorMax.y);
            }

            bool isOwned = _catalog.IsOwned(gun, _progress);
            bool isEquipped = _catalog.GetEquipped(_progress) == gun;
            _priceGroup.SetActive(!isOwned);
            _actionLabel.gameObject.SetActive(isOwned);

            if (isEquipped)
            {
                _actionImage.sprite = _equippedSprite;
                _actionLabel.SetText("EQUIPPED");
                _actionButton.interactable = false;
            }
            else if (isOwned)
            {
                _actionImage.sprite = _equipSprite;
                _actionLabel.SetText("EQUIP");
                _actionButton.interactable = true;
            }
            else
            {
                _actionImage.sprite = _buySprite;
                _priceLabel.SetText("{0}", gun.Price);
                _actionButton.interactable = _progress.Coins >= gun.Price;
            }
        }

        private void SetMenuVisible(bool isVisible)
        {
            foreach (GameObject item in _hiddenWhileOpen)
            {
                if (item != null)
                {
                    item.SetActive(isVisible);
                }
            }
        }
    }
}
