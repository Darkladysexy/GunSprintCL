using GunSprint.Audio;
using UnityEngine;

namespace GunSprint.Data
{
    /// <summary>
    /// Values shared by every enemy of a kind (flyweight): dozens of pooled enemies point to one asset
    /// instead of each carrying its own copy.
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "GunSprint/Data/Enemy Config")]
    public class EnemyConfigSO : ScriptableObject
    {
        [Header("Reward")]
        [SerializeField, Min(0)] private int _coinReward = 100;

        [Header("Death")]
        [Tooltip("Impulse that throws the body back along the bullet direction.")]
        [SerializeField, Min(0f)] private float _deathKnockImpulse = 3f;

        [Tooltip("Seconds the body stays before it shrinks away and returns to the pool.")]
        [SerializeField, Min(0f)] private float _corpseDuration = 0.8f;

        [SerializeField, Min(0.01f)] private float _shrinkDuration = 0.25f;

        [Tooltip("Optional particle effect played where the enemy dies.")]
        [SerializeField] private GameObject _deathEffect;

        [SerializeField, Min(0.1f)] private float _deathEffectLifetime = 1.5f;

        [Header("Shield indicator")]
        [Tooltip("Icon floating above shielded enemies, so players know they need two hits.")]
        [SerializeField] private Sprite _shieldIcon;

        [SerializeField, Min(0.05f)] private float _shieldIconSize = 0.6f;

        [SerializeField] private float _shieldIconHeight = 2.3f;

        [Header("Audio")]
        [SerializeField] private AudioCueSO _hitCue;
        [SerializeField] private AudioCueSO _shieldBreakCue;
        [SerializeField] private AudioCueSO _deathCue;

        public int CoinReward => _coinReward;

        public float DeathKnockImpulse => _deathKnockImpulse;

        public float CorpseDuration => _corpseDuration;

        public float ShrinkDuration => _shrinkDuration;

        public GameObject DeathEffect => _deathEffect;

        public float DeathEffectLifetime => _deathEffectLifetime;

        public Sprite ShieldIcon => _shieldIcon;

        public float ShieldIconSize => _shieldIconSize;

        public float ShieldIconHeight => _shieldIconHeight;

        public AudioCueSO HitCue => _hitCue;

        public AudioCueSO ShieldBreakCue => _shieldBreakCue;

        public AudioCueSO DeathCue => _deathCue;
    }
}
