using UnityEngine;

namespace GunSprint.Data
{
    /// <summary>
    /// Layout of one level for the Levels mode. Either a list of track segments chained from the
    /// gun's start (enemies are spread over the segments), or a legacy single map prefab with
    /// enemies at fixed distances.
    /// </summary>
    [CreateAssetMenu(fileName = "Level", menuName = "GunSprint/Data/Level")]
    public class LevelConfigSO : ScriptableObject
    {
        [Header("Segments")]
        [Tooltip("Track segments (prefabs with a FloorSegment) laid out in order. Leave empty to use a map.")]
        [SerializeField] private GameObject[] _segments = new GameObject[0];

        [Tooltip("Empty space between two segments, in meters.")]
        [SerializeField, Min(0f)] private float _segmentGap = 2f;

        [Tooltip("Enemies per segment, as a fraction of the segment's slots (the first segment stays empty).")]
        [SerializeField, Range(0f, 1f)] private float _enemyDensity = 0.5f;

        [SerializeField, Range(0f, 1f)] private float _swordChance = 0.3f;
        [SerializeField, Range(0f, 1f)] private float _shieldChance = 0.3f;

        [Header("Legacy map")]
        [Tooltip("One of these maps is spawned for the level.")]
        [SerializeField] private GameObject[] _mapPrefabs = new GameObject[0];

        [SerializeField, Min(0)] private int _enemyCount = 3;

        [Tooltip("Distance along the track of the first enemy, in meters.")]
        [SerializeField, Min(0f)] private float _firstEnemyDistance = 5f;

        [Tooltip("Distance between two enemies, in meters.")]
        [SerializeField, Min(0.5f)] private float _enemySpacing = 5f;

        [Tooltip("Extra distance between the last enemy and the finish targets.")]
        [SerializeField, Min(0f)] private float _finishOffset;

        public bool UsesSegments => _segments.Length > 0;

        public GameObject[] Segments => _segments;

        public float SegmentGap => _segmentGap;

        public float EnemyDensity => _enemyDensity;

        public float SwordChance => _swordChance;

        public float ShieldChance => _shieldChance;

        public int EnemyCount => _enemyCount;

        public float FirstEnemyDistance => _firstEnemyDistance;

        public float EnemySpacing => _enemySpacing;

        public float FinishOffset => _finishOffset;

        public float FinishDistance => _firstEnemyDistance + _enemyCount * _enemySpacing + _finishOffset;

        public GameObject PickMap(System.Random random)
        {
            return _mapPrefabs.Length == 0 ? null : _mapPrefabs[random.Next(_mapPrefabs.Length)];
        }
    }
}
