using UnityEngine;
using System.Collections;

public class LevelModeDistance : MonoBehaviour
{
    [System.Serializable]
    public class Level {
        public GameObject[] mapPrefabs;
        public int enemies = 10;
        public float firstMark = 18f;
        public float stepMeters = 12f;
    }

    [Header("Refs")]
    [SerializeField] private DistanceEnemySpawner spawner; 
    [SerializeField] private Transform mapRoot;            
    [SerializeField] private GameObject finishStackRoot;   
    [SerializeField] private GameObject gunObject; 
    [SerializeField] private Level[] levels;

    void Start()
    {
        if (GameManager.Instance.CurrentMode == GameMode.Levels)
            StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        for (int i = 0; i < levels.Length; i++)
        {
            GameManager.Instance.ResetLevelScore();
            if (mapRoot) { foreach (Transform child in mapRoot) Destroy(child.gameObject); }
            if (gunObject) GameManager.Instance.ResetGunState(gunObject, new Vector3(2,1,0), new Vector3(0,-90,0));

            yield return new WaitForSeconds(0.2f);

            var lv = levels[i];
            if (lv.mapPrefabs.Length > 0) Instantiate(lv.mapPrefabs[Random.Range(0, lv.mapPrefabs.Length)], mapRoot);

            float mark = lv.firstMark;
            for (int k = 0; k < lv.enemies; k++, mark += lv.stepMeters)
                spawner.SpawnAtMeters(mark, null, null);

            while (spawner.AliveCount > 0) yield return null;

            if (finishStackRoot)
            {
                finishStackRoot.SetActive(true);
                // Chờ cho đến khi bắn trúng stack
                while (!GameManager.Instance.IsLevelFinished) yield return null;
                // Chờ cho đến khi nhấn nút Next Level trên UI
                while (GameManager.Instance.IsLevelFinished) yield return null;
                finishStackRoot.SetActive(false);
            }
        }
    }
}