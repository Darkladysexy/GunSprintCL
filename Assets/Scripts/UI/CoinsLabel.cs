using GunSprint.Data;
using TMPro;
using UnityEngine;

namespace GunSprint.UI
{
    /// <summary>
    /// Shows the player's coin balance and punches when it grows. Updates only on change.
    /// </summary>
    public class CoinsLabel : MonoBehaviour
    {
        [SerializeField] private PlayerProgressSO _progress;
        [SerializeField] private TMP_Text _label;

        [Tooltip("Optional element (usually the coin icon) that punches when coins are gained.")]
        [SerializeField] private Transform _punchTarget;

        private Vector3 _punchBaseScale = Vector3.one;
        private Coroutine _punch;
        private int _shownCoins;

        private void Awake()
        {
            if (_punchTarget != null)
            {
                _punchBaseScale = _punchTarget.localScale;
            }
        }

        private void OnEnable()
        {
            _progress.CoinsChanged += HandleCoinsChanged;
            _shownCoins = _progress.Coins;
            _label.SetText("{0}", _shownCoins);
        }

        private void OnDisable()
        {
            _progress.CoinsChanged -= HandleCoinsChanged;
            if (_punchTarget != null)
            {
                _punchTarget.localScale = _punchBaseScale;
            }
        }

        private void HandleCoinsChanged(int coins)
        {
            bool hasGained = coins > _shownCoins;
            _shownCoins = coins;
            _label.SetText("{0}", coins);

            if (hasGained && _punchTarget != null && isActiveAndEnabled)
            {
                if (_punch != null)
                {
                    StopCoroutine(_punch);
                }

                _punch = StartCoroutine(UIAnimations.Punch(_punchTarget, _punchBaseScale, 0.25f, 0.2f));
            }
        }
    }
}
