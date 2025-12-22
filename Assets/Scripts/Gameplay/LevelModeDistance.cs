using UnityEngine;
using System.Collections;

public class LevelModeDistance : MonoBehaviour
{
    [System.Serializable]
    public class Level {
        public string levelName; 
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

    [Header("Settings")]
    [SerializeField] private float finishOffset = 15f; 

    void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentMode == GameMode.Levels)
            StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        while (true)
        {
            // 1. Lấy chỉ số level thực tế từ bộ nhớ
            int currentLevelProgress = GameManager.Instance.CurrentLevelIndex;
            int levelToLoad;

            // 2. Logic logic chọn Level:
            // Nếu level hiện tại >= 3 (tức là từ Level 4 trở đi, vì index bắt đầu từ 0)
            if (currentLevelProgress >= 3)
            {
                // Chọn ngẫu nhiên index 0, 1 hoặc 2 (tương ứng Lv 1, 2, 3)
                levelToLoad = Random.Range(0, 3);
            }
            else
            {
                // Ngược lại thì đi theo đúng thứ tự tuyến tính
                levelToLoad = currentLevelProgress;
            }

            // Kiểm tra an toàn nếu mảng levels bị trống hoặc index vượt quá
            if (levelToLoad >= levels.Length) levelToLoad = 0;

            // 3. Khởi tạo trạng thái cho Level mới
            GameManager.Instance.ResetLevelScore();
            GameManager.Instance.UpdateLevelUI(); // Vẫn hiện "Level 4, 5..." dựa trên progress thực

            // 4. Dọn dẹp map cũ và reset súng
            if (mapRoot) { foreach (Transform child in mapRoot) Destroy(child.gameObject); }
            if (gunObject) GameManager.Instance.ResetGunState(gunObject, new Vector3(2, 1, 0), new Vector3(0, -90, 0));
            if (finishStackRoot) finishStackRoot.SetActive(false);

            yield return null;

            // 5. Tạo môi trường từ levelToLoad đã chọn
            var lv = levels[levelToLoad];
            if (lv.mapPrefabs.Length > 0) 
            {
                Instantiate(lv.mapPrefabs[Random.Range(0, lv.mapPrefabs.Length)], mapRoot);
            }

            // Đợi hệ thống vật lý sẵn sàng
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            // 6. Sinh quái dựa trên cấu hình của level được chọn
            float mark = lv.firstMark;
            for (int k = 0; k < lv.enemies; k++)
            {
                GameObject enemy = null;
                int attempts = 0;
                while (enemy == null && attempts < 3)
                {
                    enemy = spawner.SpawnAtMeters(mark, null, null);
                    if (enemy == null) {
                        attempts++;
                        yield return new WaitForFixedUpdate();
                    }
                }
                mark += lv.stepMeters;
            }

            // 7. Đặt Finish Stack
            if (finishStackRoot)
            {
                float finalX = -(mark + finishOffset);
                finishStackRoot.transform.position = new Vector3(finalX, 1f, 0f);
            }

            // 8. Chờ hoàn thành
            while (spawner.AliveCount > 0) yield return null;

            if (finishStackRoot)
            {
                finishStackRoot.SetActive(true);
                while (!GameManager.Instance.IsLevelFinished) yield return null;
                while (GameManager.Instance.IsLevelFinished) yield return null;
                finishStackRoot.SetActive(false);
            }
        }
    }
}