using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EnemyPrefabs
{
    public GameObject idle;   // đứng yên
    public GameObject sword;  // cầm kiếm (có anim Attack)
    public GameObject basic;  // bản cơ bản (dùng để bật khiên)
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform followTarget; // Camera hoặc súng
    [SerializeField] private EnemyPrefabs prefabs;

    [Header("Spawn Area")]
    [SerializeField] private float spawnDistance = 25f;
    [SerializeField] private float lateralRange = 3.5f;
    [SerializeField] private float spawnY = 0f;

    private readonly List<GameObject> _alive = new();

    public int AliveCount
    {
        get { _alive.RemoveAll(g => g == null); return _alive.Count; }
    }

    public GameObject SpawnOne(bool useSword, bool withShield)
    {
        if (followTarget == null) return null;

        Vector3 basePos = followTarget.position + followTarget.forward * spawnDistance;
        Vector3 spawnPos = basePos + followTarget.right * Random.Range(-lateralRange, lateralRange);
        spawnPos.y = spawnY;

        GameObject prefab =
            useSword && prefabs.sword ? prefabs.sword :
            prefabs.idle             ? prefabs.idle :
            prefabs.basic;

        if (prefab == null) return null;

        Vector3 dir = followTarget.position - spawnPos; dir.y = 0f;
        Quaternion rot = dir.sqrMagnitude > 0.01f ? Quaternion.LookRotation(dir) : Quaternion.identity;

        var go = Instantiate(prefab, spawnPos, rot);
        _alive.Add(go);

        var enemy = go.GetComponent<Enemy>();
        if (enemy != null) enemy.Init(withShield);

        return go;
    }
}
