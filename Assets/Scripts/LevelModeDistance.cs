using UnityEngine;
using System.Collections;

public class LevelModeDistance : MonoBehaviour
{
    [System.Serializable]
    public class Level {
        public string name = "Lv";
        public GameObject[] mapPrefabs;
        public int enemies = 10;       // số lượng
        public float firstMark = 18f;  // mốc đầu tiên (m)
        public float stepMeters = 12f; // khoảng cách giữa các spawn
        [Range(0,1)] public float swordRate  = 0.15f;
        [Range(0,1)] public float shieldRate = 0.25f;
    }

    [Header("Refs")]
    [SerializeField] private DistanceEnemySpawner spawner; // autoLoop = false
    [SerializeField] private Transform mapRoot;            // nơi đặt prefab map
    [SerializeField] private GameObject finishStackRoot;   // cụm multiplier (inactive sẵn)

    [Header("Levels")]
    [SerializeField] private Level[] levels;

    void Start(){
        if (!GameManager.Instance || GameManager.Instance.CurrentMode != GameMode.Levels){ gameObject.SetActive(false); return; }
        if (!spawner) { enabled = false; return; }
        StartCoroutine(Run());
    }

    IEnumerator Run(){
        for (int i = 0; i < levels.Length; i++){
            var lv = levels[i];

            // 1) random 1 map
            if (lv.mapPrefabs != null && lv.mapPrefabs.Length > 0){
                var mp = lv.mapPrefabs[Random.Range(0, lv.mapPrefabs.Length)];
                if (mp) Instantiate(mp, mapRoot ? mapRoot : transform, false);
            }

            // 2) spawn theo mốc
            float mark = lv.firstMark;
            for (int k = 0; k < lv.enemies; k++, mark += lv.stepMeters){
                bool useSword   = Random.value < lv.swordRate;
                bool withShield = !useSword && Random.value < lv.shieldRate;
                spawner.SpawnAtMeters(mark, useSword, withShield);
            }

            // 3) chờ clear hết
            while (spawner.AliveCount > 0) yield return null;

            // 4) bật khu vực multiplier
            if (finishStackRoot){
                finishStackRoot.SetActive(true);
                yield return new WaitForSeconds(1f);
                finishStackRoot.SetActive(false);
            }

            // 5) dọn map
            if (mapRoot){
                for (int c = mapRoot.childCount - 1; c >= 0; c--)
                    Destroy(mapRoot.GetChild(c).gameObject);
            }
        }
        // TODO: "Level Complete" UI hoặc load màn tiếp
    }
}
