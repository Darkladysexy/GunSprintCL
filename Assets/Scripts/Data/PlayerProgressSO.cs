using System;
using GunSprint.Core;
using UnityEngine;

namespace GunSprint.Data
{
    /// <summary>
    /// Typed access to the player's persistent progress: coins, level, best distance, upgrades and settings.
    /// Values live in PlayerPrefs (external storage, as ScriptableObject data is not saved in builds);
    /// this asset just gives every scene the same Inspector-assignable entry point plus change events.
    /// Setters only touch memory - call <see cref="Save"/> at checkpoints to write to disk.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerProgress", menuName = "GunSprint/Data/Player Progress")]
    public class PlayerProgressSO : ScriptableObject
    {
        // Keys are unchanged from the first release so existing saves keep working.
        private const string k_CoinsKey = "total_coin";
        private const string k_LevelIndexKey = "current_level_index";
        private const string k_BestDistanceKey = "best_meters";
        private const string k_SoundKey = "Setting_Sound";
        private const string k_ModeKey = "gm_mode";
        private const string k_EquippedGunKey = "gun_equipped";
        private const string k_OwnedGunKeyPrefix = "gun_owned_";

        public event Action<int> CoinsChanged;
        public event Action<bool> SoundChanged;
        public event Action<UpgradeSO> UpgradePurchased;
        public event Action<GunDefinitionSO> GunsChanged;

        public int Coins => PlayerPrefs.GetInt(k_CoinsKey, 0);

        /// <summary>Zero-based index of the next level to play.</summary>
        public int LevelIndex => PlayerPrefs.GetInt(k_LevelIndexKey, 0);

        public float BestDistance => PlayerPrefs.GetFloat(k_BestDistanceKey, 0f);

        public bool IsSoundOn => PlayerPrefs.GetInt(k_SoundKey, 1) == 1;

        public GameMode SelectedMode
        {
            get => PlayerPrefs.GetInt(k_ModeKey, (int)GameMode.Levels) == (int)GameMode.Infinity
                ? GameMode.Infinity
                : GameMode.Levels;
            set => PlayerPrefs.SetInt(k_ModeKey, (int)value);
        }

        public void AddCoins(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            SetCoins(Coins + amount);
        }

        public bool TrySpendCoins(int amount)
        {
            if (amount < 0 || Coins < amount)
            {
                return false;
            }

            SetCoins(Coins - amount);
            return true;
        }

        /// <summary>Id of the equipped gun; empty means the catalog's default gun.</summary>
        public string EquippedGunId => PlayerPrefs.GetString(k_EquippedGunKey, string.Empty);

        public bool IsGunOwned(string gunId)
        {
            return PlayerPrefs.GetInt(k_OwnedGunKeyPrefix + gunId, 0) == 1;
        }

        /// <summary>Buys the gun if the player has enough coins. Does not equip it.</summary>
        public bool TryBuyGun(GunDefinitionSO gun)
        {
            if (IsGunOwned(gun.Id) || !TrySpendCoins(gun.Price))
            {
                return false;
            }

            PlayerPrefs.SetInt(k_OwnedGunKeyPrefix + gun.Id, 1);
            Save();
            GunsChanged?.Invoke(gun);
            return true;
        }

        /// <summary>Equips a gun. The caller checks ownership (see GunCatalogSO.IsOwned).</summary>
        public void EquipGun(GunDefinitionSO gun)
        {
            PlayerPrefs.SetString(k_EquippedGunKey, gun.Id);
            Save();
            GunsChanged?.Invoke(gun);
        }

        public void AdvanceLevel()
        {
            PlayerPrefs.SetInt(k_LevelIndexKey, LevelIndex + 1);
        }

        /// <summary>
        /// Stores the distance if it beats the current record. Returns true for a new record.
        /// </summary>
        public bool TrySubmitDistance(float meters)
        {
            if (meters <= BestDistance)
            {
                return false;
            }

            PlayerPrefs.SetFloat(k_BestDistanceKey, meters);
            return true;
        }

        public void SetSoundOn(bool isOn)
        {
            PlayerPrefs.SetInt(k_SoundKey, isOn ? 1 : 0);
            Save();
            SoundChanged?.Invoke(isOn);
        }

        public int GetUpgradeLevel(UpgradeSO upgrade)
        {
            return PlayerPrefs.GetInt(upgrade.SaveKey, 0);
        }

        public bool CanPurchaseUpgrade(UpgradeSO upgrade)
        {
            int level = GetUpgradeLevel(upgrade);
            return !upgrade.IsMaxLevel(level) && Coins >= upgrade.GetCost(level);
        }

        public bool TryPurchaseUpgrade(UpgradeSO upgrade)
        {
            int level = GetUpgradeLevel(upgrade);
            if (upgrade.IsMaxLevel(level) || !TrySpendCoins(upgrade.GetCost(level)))
            {
                return false;
            }

            PlayerPrefs.SetInt(upgrade.SaveKey, level + 1);
            Save();
            UpgradePurchased?.Invoke(upgrade);
            return true;
        }

        /// <summary>
        /// Writes pending changes to disk. Call at checkpoints (run end, pause, app pause),
        /// never every frame or every kill: disk writes cause hitches on mobile.
        /// </summary>
        public void Save()
        {
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Deletes every saved value. Exposed to the Editor inspector for testing only.
        /// </summary>
        public void ResetAll(UpgradeSO[] upgrades, GunDefinitionSO[] guns)
        {
            PlayerPrefs.DeleteKey(k_EquippedGunKey);
            foreach (GunDefinitionSO gun in guns)
            {
                if (gun != null)
                {
                    PlayerPrefs.DeleteKey(k_OwnedGunKeyPrefix + gun.Id);
                }
            }

            PlayerPrefs.DeleteKey(k_CoinsKey);
            PlayerPrefs.DeleteKey(k_LevelIndexKey);
            PlayerPrefs.DeleteKey(k_BestDistanceKey);
            PlayerPrefs.DeleteKey(k_ModeKey);
            foreach (UpgradeSO upgrade in upgrades)
            {
                if (upgrade != null)
                {
                    PlayerPrefs.DeleteKey(upgrade.SaveKey);
                }
            }

            Save();
            CoinsChanged?.Invoke(Coins);
            GunsChanged?.Invoke(null);
        }

        private void SetCoins(int value)
        {
            PlayerPrefs.SetInt(k_CoinsKey, value);
            CoinsChanged?.Invoke(value);
        }
    }
}
