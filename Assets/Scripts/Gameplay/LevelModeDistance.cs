using UnityEngine;
using System.Collections;

public class LevelModeDistance : MonoBehaviour
{
    [System.Serializable]
    public class Level {
        public string levelName; // Tên level (tùy chọn)
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
        // Chỉ chạy logic Level nếu đang ở chế độ chơi Levels
        if (GameManager.Instance != null && GameManager.Instance.CurrentMode == GameMode.Levels)
            StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        // Vòng lặp vô tận để người chơi có thể chơi liên tục các level
        while (true)
        {
            // Lấy chỉ số level hiện tại từ GameManager (đã lưu trong PlayerPrefs)
            int currentIdx = GameManager.Instance.CurrentLevelIndex;

            // Nếu người chơi đã vượt quá số lượng level thiết kế, có thể reset về 0 hoặc chơi lại level cuối
            if (currentIdx >= levels.Length)
            {
                currentIdx = 0; 
                // Tùy chọn: GameManager.Instance.ResetProgress(); // Nếu muốn reset hoàn toàn
            }

            // 1. Khởi tạo trạng thái cho Level mới
            GameManager.Instance.ResetLevelScore(); // Reset điểm riêng của màn chơi đó
            GameManager.Instance.UpdateLevelUI();    // Cập nhật text hiển thị "Level X"

            // 2. Dọn dẹp map cũ và reset vị trí súng
            if (mapRoot) { foreach (Transform child in mapRoot) Destroy(child.gameObject); }
            if (gunObject) GameManager.Instance.ResetGunState(gunObject, new Vector3(2, 1, 0), new Vector3(0, -90, 0));

            yield return new WaitForSeconds(0.2f);

            // 3. Tạo môi trường (Map)
            var lv = levels[currentIdx];
            if (lv.mapPrefabs.Length > 0) 
                Instantiate(lv.mapPrefabs[Random.Range(0, lv.mapPrefabs.Length)], mapRoot);

            // 4. Sinh quái dọc đường theo thông số của level hiện tại
            float mark = lv.firstMark;
            for (int k = 0; k < lv.enemies; k++, mark += lv.stepMeters)
                spawner.SpawnAtMeters(mark, null, null);

            // 5. Chờ người chơi tiêu diệt hết quái vật trên đường
            while (spawner.AliveCount > 0) yield return null;

            // 6. Kích hoạt cột mốc kết thúc (Finish Stack)
            if (finishStackRoot)
            {
                finishStackRoot.SetActive(true);
                
                // Chờ cho đến khi đạn bắn trúng stack (GameManager.WinLevel sẽ đặt IsLevelFinished = true)
                while (!GameManager.Instance.IsLevelFinished) yield return null;
                
                // Chờ cho đến khi người chơi nhấn nút "Next Level" trên Win Canvas
                // Nút này sẽ gọi GameManager.NextLevel() để đặt IsLevelFinished = false
                while (GameManager.Instance.IsLevelFinished) yield return null;
                
                finishStackRoot.SetActive(false);
            }
            
            // Sau khi thoát khỏi vòng lặp chờ, code sẽ quay lại đầu 'while(true)' 
            // và lấy CurrentLevelIndex mới để nạp level tiếp theo.
        }
    }
}