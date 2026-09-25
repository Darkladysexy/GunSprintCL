using UnityEngine;

namespace GunSprint.Data
{
    /// <summary>
    /// One purchasable upgrade (extendable enum pattern): the asset itself identifies the upgrade,
    /// and it also carries its pricing and value curve. Add a new upgrade by creating a new asset.
    /// </summary>
    [CreateAssetMenu(fileName = "Upgrade", menuName = "GunSprint/Data/Upgrade")]
    public class UpgradeSO : ScriptableObject
    {
        [Tooltip("PlayerPrefs key storing the purchased level. Never change it after release.")]
        [SerializeField] private string _saveKey = "upgrade_level";

        [SerializeField] private string _displayName = "UPGRADE";
        [SerializeField] private Sprite _icon;

        [Header("Price")]
        [SerializeField, Min(1)] private int _baseCost = 100;

        [Tooltip("Cost multiplier applied per purchased level (1.1 = +10% per level).")]
        [SerializeField, Min(1f)] private float _costGrowth = 1.1f;

        [Tooltip("0 = unlimited.")]
        [SerializeField, Min(0)] private int _maxLevel;

        [Header("Value")]
        [Tooltip("Value at level 0.")]
        [SerializeField] private float _baseValue;

        [SerializeField] private float _valuePerLevel = 1f;

        [Tooltip("How the value is shown in the UI. {0} is replaced by the value, e.g. \"{0}%\" or \"{0} m\".")]
        [SerializeField] private string _valueFormat = "{0}";

        public string SaveKey => _saveKey;

        public string DisplayName => _displayName;

        public Sprite Icon => _icon;

        public int GetCost(int level)
        {
            return Mathf.RoundToInt(_baseCost * Mathf.Pow(_costGrowth, level));
        }

        public float GetValue(int level)
        {
            return _baseValue + _valuePerLevel * level;
        }

        public string FormatValue(int level)
        {
            return string.Format(_valueFormat, Mathf.RoundToInt(GetValue(level)));
        }

        public bool IsMaxLevel(int level)
        {
            return _maxLevel > 0 && level >= _maxLevel;
        }
    }
}
