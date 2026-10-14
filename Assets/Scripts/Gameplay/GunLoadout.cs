using GunSprint.Data;
using UnityEngine;

namespace GunSprint.Gameplay
{
    /// <summary>
    /// Turns the gun prefab into the gun equipped in the shop: applies its stats and swaps the model.
    /// The built-in model under <see cref="_visual"/> is the size reference, so every gun keeps the
    /// same colliders and feel, and stays visible if a definition has no model.
    /// Runs before the other gun scripts so they start with the right stats.
    /// </summary>
    [DefaultExecutionOrder(-90)]
    [DisallowMultipleComponent]
    public class GunLoadout : MonoBehaviour
    {
        [SerializeField] private PlayerProgressSO _progress;
        [SerializeField] private GunCatalogSO _catalog;

        [Tooltip("Parent of the gun meshes. Animated by GunShotAnimation; holds the default model.")]
        [SerializeField] private Transform _visual;

        public GunDefinitionSO Gun { get; private set; }

        /// <summary>Root of the meshes currently shown.</summary>
        public Transform Model { get; private set; }

        /// <summary>Length of the gun along its barrel, in the gun's local space.</summary>
        public float Length { get; private set; } = 0.35f;

        public Transform Visual => _visual;

        private void Awake()
        {
            Model = _visual;
            if (GunModelFitter.TryGetBounds(_visual, transform, out Bounds reference))
            {
                Length = reference.size.z;
            }

            Gun = _catalog != null ? _catalog.GetEquipped(_progress) : null;
            if (Gun == null)
            {
                return;
            }

            if (Gun.Config != null)
            {
                GetComponent<GunController>().SetConfig(Gun.Config);
                GetComponent<GunMotor>().SetConfig(Gun.Config);
                GetComponent<GunShooter>().SetConfig(Gun.Config);
            }

            if (Gun.ModelPrefab != null && reference.size.z > 0f)
            {
                SwapModel(Gun, reference);
            }
        }

        private void SwapModel(GunDefinitionSO gun, Bounds reference)
        {
            // Hide the default model now (Destroy only happens at the end of the frame).
            for (int i = _visual.childCount - 1; i >= 0; i--)
            {
                GameObject child = _visual.GetChild(i).gameObject;
                child.SetActive(false);
                Destroy(child);
            }

            Model = GunModelFitter.Spawn(gun.ModelPrefab, _visual, gun.ModelRotation);
            GunModelFitter.Fit(Model, transform, reference.size.z, reference.center);

            // Muzzle and smoke are children of the gun root, so root space is their local space.
            GunShooter shooter = GetComponent<GunShooter>();
            Vector3 muzzle = GunModelFitter.FindMuzzle(Model, transform);
            shooter.Muzzle.localPosition = muzzle;
            if (shooter.MuzzleSmoke != null)
            {
                shooter.MuzzleSmoke.transform.localPosition = muzzle;
            }
        }
    }
}
