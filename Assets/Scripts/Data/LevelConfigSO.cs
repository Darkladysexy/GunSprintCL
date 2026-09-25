using UnityEngine;

namespace GunSprint.Data
{
    /// <summary>
    /// Layout of one hand-made level for the Levels mode.
    /// </summary>
    [CreateAssetMenu(fileName = "Level", menuName = "GunSprint/Data/Level")]
    public class LevelConfigSO : ScriptableObject
    {
        [Tooltip("One of these maps is spawned for the level.")]
        [SerializeField] private GameObject[] _mapPrefabs = new GameObject[0];

        [SerializeField, Min(0)] private int _enemyCount = 3;

        [Tooltip("Distance along the track of the first enemy, in meters.")]
        [SerializeField, Min(0f)] private float _firstEnemyDistance = 5f;

        [Tooltip("Distance between two enemies, in meters.")]
        [SerializeField, Min(0.5f)] private float _enemySpacing = 5f;

        [Tooltip("Extra distance between the last enemy and the finish targets.")]
        [SerializeField, Min(0f)] private float _finishOffset;

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
