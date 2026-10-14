using UnityEngine;

namespace GunSprint.Data
{
    /// <summary>
    /// Every gun sold in the shop, in display order. The first gun is free and always owned,
    /// so the player can never end up without a gun.
    /// </summary>
    [CreateAssetMenu(fileName = "GunCatalog", menuName = "GunSprint/Data/Gun Catalog")]
    public class GunCatalogSO : ScriptableObject
    {
        [SerializeField] private GunDefinitionSO[] _guns = new GunDefinitionSO[0];

        public int Count => _guns.Length;

        public GunDefinitionSO DefaultGun => _guns.Length > 0 ? _guns[0] : null;

        public GunDefinitionSO Get(int index)
        {
            return _guns[index];
        }

        public int IndexOf(GunDefinitionSO gun)
        {
            return System.Array.IndexOf(_guns, gun);
        }

        public GunDefinitionSO FindById(string id)
        {
            foreach (GunDefinitionSO gun in _guns)
            {
                if (gun != null && gun.Id == id)
                {
                    return gun;
                }
            }

            return null;
        }

        /// <summary>The equipped gun, or the default gun if the save points to an unknown one.</summary>
        public GunDefinitionSO GetEquipped(PlayerProgressSO progress)
        {
            GunDefinitionSO gun = FindById(progress.EquippedGunId);
            return gun != null ? gun : DefaultGun;
        }

        public bool IsOwned(GunDefinitionSO gun, PlayerProgressSO progress)
        {
            return gun == DefaultGun || gun.Price == 0 || progress.IsGunOwned(gun.Id);
        }

        /// <summary>Highest value of a stat in the catalog, used to scale the shop bars.</summary>
        public float GetMaxStat(GunStat stat)
        {
            float max = 0.0001f;
            foreach (GunDefinitionSO gun in _guns)
            {
                if (gun != null)
                {
                    max = Mathf.Max(max, gun.GetStat(stat));
                }
            }

            return max;
        }
    }
}
