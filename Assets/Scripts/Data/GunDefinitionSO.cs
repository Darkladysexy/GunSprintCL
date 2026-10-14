using UnityEngine;

namespace GunSprint.Data
{
    /// <summary>The stats shown as bars in the shop.</summary>
    public enum GunStat
    {
        Power,
        Bounce,
        BulletSpeed,
        FireRate,
    }

    /// <summary>
    /// One gun the player can buy and equip: its look (model + flight trail), its price and its
    /// physics (<see cref="GunConfigSO"/>). Adding a gun to the game means adding one of these assets
    /// to the <see cref="GunCatalogSO"/> - no code change.
    /// </summary>
    [CreateAssetMenu(fileName = "Gun", menuName = "GunSprint/Data/Gun Definition")]
    public class GunDefinitionSO : ScriptableObject
    {
        [Tooltip("Stable save key. Never change it after release or players lose the gun.")]
        [SerializeField] private string _id = "gun";

        [SerializeField] private string _displayName = "GUN";

        [SerializeField, Min(0)] private int _price;

        [SerializeField] private GunConfigSO _config;

        [Header("Look")]
        [Tooltip("Visual-only model (meshes). It is scaled to the size of the default gun at runtime.")]
        [SerializeField] private GameObject _modelPrefab;

        [Tooltip("Rotation that makes the model's barrel point along +Z with the top up.")]
        [SerializeField] private Vector3 _modelRotation;

        [Tooltip("Trail effect drawn behind the gun while it flies (optional).")]
        [SerializeField] private GameObject _trailPrefab;

        public string Id => _id;

        public string DisplayName => _displayName;

        public int Price => _price;

        public GunConfigSO Config => _config;

        public GameObject ModelPrefab => _modelPrefab;

        public Quaternion ModelRotation => Quaternion.Euler(_modelRotation);

        public GameObject TrailPrefab => _trailPrefab;

        public float GetStat(GunStat stat)
        {
            switch (stat)
            {
                case GunStat.Power:
                    return _config.RecoilImpulse;
                case GunStat.Bounce:
                    return _config.LandingBounce;
                case GunStat.BulletSpeed:
                    return _config.BulletSpeed;
                case GunStat.FireRate:
                    return 1f / Mathf.Max(0.05f, _config.FireCooldown);
                default:
                    return 0f;
            }
        }
    }
}
