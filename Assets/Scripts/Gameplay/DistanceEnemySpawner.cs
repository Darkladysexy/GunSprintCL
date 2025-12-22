using System.Collections.Generic;
using UnityEngine;

[System.Serializable] public class EnemyPrefabsSet { public GameObject idle, sword, basic; }

public class DistanceEnemySpawner : MonoBehaviour
{
    // ---------- References ----------
    [Header("Refs")]
    [SerializeField] private Transform follow;        // Gun để đo quãng đường
    [SerializeField] private Transform pathA;         // Mốc đầu đường (center-line)
    [SerializeField] private Transform pathB;         // Mốc thứ hai (định hướng A→B)
    [SerializeField] private EnemyPrefabsSet prefabs;

    // ---------- Auto spawn (Infinity) ----------
    [Header("Auto spawn (Infinity)")]
    [SerializeField] public bool autoLoop = false;    // Infinity: true / Levels: false
    [SerializeField] private float firstSpawnAt = 18f;
    [SerializeField] private float spawnStep    = 12f;
    [SerializeField] private float spawnAhead   = 24f;

    // ---------- Lateral (ngang đường) ----------
    [Header("Lateral")]
    [Tooltip("0 = spawn đúng center-line A→B. >0 = cho phép ngẫu nhiên trái/phải theo right của A→B")]
    [SerializeField] private float lateralRange = 0f; // để 0 nếu bạn muốn đúng trục

    // ---------- Ground snap ----------
    public enum DownMode { WorldY, SpawnerUp, Custom }
    [Header("Ground snap")]
    [SerializeField] private bool useGroundRaycast = true;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float rayHeight = 50f;
    [SerializeField] private DownMode rayDownMode = DownMode.WorldY;
    [SerializeField] private Vector3 customDown = Vector3.down;

    // ---------- Types & Limits ----------
    [Header("Types")]
    [Range(0,1)] public float swordRate  = 0.20f;
    [Range(0,1)] public float shieldRate = 0.25f;
    [Header("Limits")]
    [SerializeField] private int maxAlive = 16;

    // ---------- Preview ----------
    [Header("Preview (Scene view)")]
    [SerializeField] private bool showPreview = true;
    [SerializeField] private float previewStart = 18f;
    [SerializeField] private int previewCount = 4;
    [SerializeField] private float previewStepMeters = 12f;

    private readonly List<GameObject> _alive = new();
    private float _nextMark;

    public int AliveCount { get { _alive.RemoveAll(g => g == null); return _alive.Count; } }

    void Start(){
        if (!follow || !pathA || !pathB){ enabled = false; return; }

        // SỬA TẠI ĐÂY: 
        // Lấy vị trí hiện tại của súng trên trục tiến độ (mét).
        // Nếu súng bắt đầu ở -80m (Start Upgrade), giá trị này sẽ là 80m.
        float currentGunMeters = ProjectedMeters(follow.position);

        // Đặt mốc sinh quái tiếp theo bắt đầu từ vị trí súng + khoảng cách xuất hiện đầu tiên.
        // Điều này giúp bỏ qua việc spawn quái ở đoạn đường súng đã "nhảy" qua.
        _nextMark = currentGunMeters + firstSpawnAt;
    }

    void Update(){
        for (int i = _alive.Count - 1; i >= 0; i--) if (_alive[i] == null) _alive.RemoveAt(i);
        if (!autoLoop) return;

        float progress = ProjectedMeters(follow.position); // tiến độ của súng dọc A→B
        
        // Chỉ sinh quái khi súng tiến tới gần mốc _nextMark tiếp theo
        while (progress >= _nextMark && _alive.Count < maxAlive){
            SpawnAtMeters(_nextMark + spawnAhead, null, null);
            _nextMark += spawnStep;
        }
    }

    // ===== API cho Levels & Infinity =====
    public GameObject SpawnAtMeters(float metersAlong, bool? forceSword, bool? forceShield) {
        // Ép Unity cập nhật trạng thái vật lý mới nhất của Map vừa sinh ra
        Physics.SyncTransforms(); 

        Vector3 f = PathForward();
        Vector3 r = Vector3.Cross(Vector3.up, f);
        Vector3 pos = PathOrigin() + f * metersAlong + r * Random.Range(-lateralRange, lateralRange);

        if (useGroundRaycast) {
            Vector3 down = DownDir();
            Vector3 up = -down;
            // Tăng rayHeight lên cao (100) và bắn tia dài hơn (200) để đảm bảo luôn trúng sàn
            Vector3 start = pos + up * 100f; 

            if (Physics.Raycast(start, down, out var hit, 200f, groundMask)) {
                pos.y = hit.point.y;
            } else {
                // Nếu không thấy sàn bên dưới mốc này, không sinh quái để tránh quái rơi vào hư vô
                Debug.LogWarning($"[Spawner] Không thấy sàn tại {metersAlong}m. Kiểm tra Layer của Map!");
                return null; 
            }
        }

        bool useSword = forceSword ?? (Random.value < swordRate);
        bool withShield = forceShield ?? (!useSword && Random.value < shieldRate);
        var prefab = useSword && prefabs.sword ? prefabs.sword :
                    prefabs.idle ? prefabs.idle : prefabs.basic;

        if (!prefab) return null;

        var go = Instantiate(prefab, pos, Quaternion.LookRotation(f));
        _alive.Add(go);
        var enemy = go.GetComponent<Enemy>();
        if (enemy) enemy.Init(withShield);
        return go;
    }

    // ===== Math helpers =====
    Vector3 PathOrigin(){ return new Vector3(pathA.position.x, 0f, pathA.position.z); }
    
    Vector3 PathForward(){
        Vector3 dir = pathB.position - pathA.position; dir.y = 0f;
        return dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector3.forward;
    }
    
    float ProjectedMeters(Vector3 worldPos){
        Vector3 f = PathForward();
        Vector3 d = worldPos - pathA.position; d.y = 0f;
        return Vector3.Dot(d, f);
    }
    
    Vector3 DownDir(){
        return rayDownMode switch {
            DownMode.SpawnerUp => -transform.up,
            DownMode.Custom    => (customDown.sqrMagnitude > 0.0001f ? customDown.normalized : Vector3.down),
            _                  => Vector3.down
        };
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected(){
        if (!pathA || !pathB) return;
        Vector3 f = PathForward();
        Vector3 a = pathA.position;
        Gizmos.color = Color.cyan; Gizmos.DrawLine(a, a + f * 120f); Gizmos.DrawSphere(a, 0.12f);

        if (!showPreview) return;
        float m = previewStart;
        for (int i = 0; i < previewCount; i++, m += previewStepMeters){
            Vector3 r = Vector3.Cross(Vector3.up, f);
            Vector3 p = a + f * m; 
            Gizmos.color = Color.green; Gizmos.DrawSphere(p, 0.15f);
            if (useGroundRaycast){
                Vector3 down = DownDir(), up = -down;
                Gizmos.color = Color.yellow; Gizmos.DrawLine(p + up * rayHeight, p + down * rayHeight);
            }
        }
    }
#endif
}