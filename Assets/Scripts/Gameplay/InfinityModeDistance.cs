using UnityEngine;

public class InfinityModeDistance : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private DistanceEnemySpawner spawner; 
    [SerializeField] private Transform gunTransform;      

    [Header("Floor Config")]
    [SerializeField] private GameObject _floorPrefab;
    [SerializeField] private Transform _floorRoot;        
    [Tooltip("Chiều dài thực tế của block sàn trong Prefab")]
    [SerializeField] private float _actualFloorSize = 20f; 
    [Tooltip("Khoảng cách trống giữa các sàn (5 đơn vị)")]
    [SerializeField] private float _gap = 5f;
    
    [Header("Spawn Settings")]
    [SerializeField] private float _spawnAheadDistance = 40f; 
    private float _lastFloorX = 0f;
    private float _stepDistance; // Tổng khoảng cách mỗi bước nhảy

    void Start(){
        if (!GameManager.Instance || GameManager.Instance.CurrentMode != GameMode.Infinity){ 
            gameObject.SetActive(false); 
            return; 
        }
        
        if (!spawner || !gunTransform){ enabled = false; return; }

        // Tính toán bước nhảy: Chiều dài sàn + Khoảng trống mong muốn
        _stepDistance = _actualFloorSize + _gap;

        spawner.autoLoop = true;
        int startBullets = GameManager.Instance.GetStartBulletsBase(5);
        GameManager.Instance.SetAmmo(startBullets);

        float bonus = GameManager.Instance.StartingPlaceBonusMeters();
        gunTransform.position = new Vector3(-bonus, gunTransform.position.y, gunTransform.position.z);

        // Bắt đầu spawn từ 0
        _lastFloorX = 0f;

        // Sinh các đoạn sàn ban đầu
        for(int i = 0; i < 8; i++) SpawnFloor();

        spawner.enabled = true;
        enabled = true;
    }

    void Update(){
        if (!GameManager.Instance || GameManager.Instance.isPaused) return;

        if (GameManager.Instance.Ammo <= 0 && spawner.AliveCount == 0) {
            FinishRun();
            return;
        }

        // Sinh sàn mới khi súng tiến về phía X âm
        if (gunTransform.position.x < _lastFloorX + _spawnAheadDistance) {
            SpawnFloor();
        }
    }

    void SpawnFloor() {
        if (!_floorPrefab) return;

        // Position Y = -10 và Rotation Y = 90
        Vector3 pos = new Vector3(_lastFloorX, -10f, 0f); 
        Quaternion rot = Quaternion.Euler(0, 90, 0);
        
        Instantiate(_floorPrefab, pos, rot, _floorRoot);
        
        // Trừ đi tổng (Chiều dài sàn + 5) để tạo khoảng trống 5 đơn vị đến sàn tiếp theo
        _lastFloorX -= _stepDistance; 
    }

    void FinishRun() {
        spawner.enabled = false;
        float finalMeters = Mathf.Max(0, -gunTransform.position.x);
        GameManager.Instance.ShowDoneScreen(finalMeters);
        enabled = false;
    }
}