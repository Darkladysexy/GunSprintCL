using UnityEngine;

public class InfinityModeDistance : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private DistanceEnemySpawner spawner; // autoLoop = true
    [SerializeField] private Transform startRoot;          // (tuỳ chọn) để cộng Starting Place

    [Header("Config")]
    [SerializeField] private int baseStartBullets = 8;

    void Start(){
        if (!GameManager.Instance || GameManager.Instance.CurrentMode != GameMode.Infinity){ gameObject.SetActive(false); return; }
        if (!spawner){ enabled = false; return; }
        spawner.autoLoop = true;
        int startBullets = GameManager.Instance.GetStartBulletsBase(baseStartBullets);
        GameManager.Instance.SetAmmo(startBullets);
        if (startRoot) startRoot.position += startRoot.forward * GameManager.Instance.StartingPlaceBonusMeters();

        spawner.enabled = true; // autoLoop đã bật trong Inspector
        enabled = true;
    }

    void Update(){
        if (!GameManager.Instance) return;

        bool noAmmo   = GameManager.Instance.Ammo <= 0;
        bool noneAlive= spawner.AliveCount == 0;
        bool gunLost  = GameManager.Instance.GunLost;

        if ((noAmmo && noneAlive) || gunLost){
            // TODO: GameOver UI
            spawner.enabled = false;
            enabled = false;
        }
    }
}
