using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game State")]
    private int _score;
    public bool isPaused = false;
    [SerializeField] private GameObject _pauseMenuUI;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _scoreText;

    [Header("Audio")]
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioClip _backgroundMusic;

    // ==== NEW: Mode / Ammo / Upgrades / Gun lost ====
    public GameMode CurrentMode { get; private set; } = GameMode.Levels;
    public int Ammo { get; private set; }
    public System.Action OnEnemyKilled;
    public bool GunLost { get; private set; } = false;

    const string KEY_MODE      = "gm_mode";
    const string KEY_UP_BULLET = "up_bullets_lv";
    const string KEY_UP_POWER  = "up_power_lv";
    const string KEY_UP_START  = "up_start_lv";

    public int BulletUpgradeLevel => PlayerPrefs.GetInt(KEY_UP_BULLET, 0);
    public int PowerUpgradeLevel  => PlayerPrefs.GetInt(KEY_UP_POWER , 0);
    public int StartUpgradeLevel  => PlayerPrefs.GetInt(KEY_UP_START , 0);

    public int   GetStartBulletsBase(int baseBullets) => baseBullets + BulletUpgradeLevel;
    public float GunPowerMultiplier() => 1f + 0.02f * PowerUpgradeLevel;
    public float StartingPlaceBonusMeters() => 20f * StartUpgradeLevel;

    void Awake(){
        if (Instance == null){ Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
        CurrentMode = (GameMode)PlayerPrefs.GetInt(KEY_MODE, (int)GameMode.Levels);
    }

    void Start(){
        _score = 0;
        if (_pauseMenuUI) _pauseMenuUI.SetActive(false);
        UpdateScoreUI();
        PlayMusic();

        
    }

    // ===== Score / UI =====
    public void AddScore(int a){ _score += a; UpdateScoreUI(); }
    public void MultiplyScore(int m){ _score *= m; UpdateScoreUI(); }
    void UpdateScoreUI(){ if (_scoreText) _scoreText.text = _score.ToString(); }

    // ===== Time / Audio / Pause =====
    public void ToggleSlowMo(bool en)
{
    // Chỉ thay đổi Time.timeScale nếu game KHÔNG bị Pause.
    if (!isPaused)
    {
        Time.timeScale = en ? 0.2f : 1f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }
}
    public void TogglePause(){ isPaused = !isPaused; Time.timeScale = isPaused ? 0f : 1f; if (_pauseMenuUI) _pauseMenuUI.SetActive(isPaused); }
    public void PlayMusic(){ if (_musicSource && _backgroundMusic && !_musicSource.isPlaying){ _musicSource.clip=_backgroundMusic; _musicSource.loop=true; _musicSource.Play(); } }
    public void ToggleSound(){ if (_musicSource){ if (_musicSource.isPlaying) _musicSource.Pause(); else _musicSource.UnPause(); } }

    // ===== Scenes =====
    public void LoadScene(string name){ Time.timeScale=1f; isPaused=false; Destroy(gameObject); SceneManager.LoadScene(name); }
    public void Replay(){ TogglePause(); LoadScene(SceneManager.GetActiveScene().name); }
    public void GoHome(){ TogglePause(); LoadScene("MainMenu"); }
    public void Quit(){ Application.Quit(); 
        UnityEditor.EditorApplication.isPlaying = false; }

    // ===== Ammo (Infinity) =====
    public void SetAmmo(int v){ Ammo = Mathf.Max(0, v); /* TODO: update UI nếu có */ }
    public bool TryConsumeAmmo(int n=1){ if (Ammo < n) return false; Ammo -= n; /* UI */ return true; }
    public void AddAmmo(int n){ Ammo += n; /* UI */ }

    public void NotifyEnemyKilled(){ OnEnemyKilled?.Invoke(); if (CurrentMode==GameMode.Infinity) AddAmmo(2); }
    public void NotifyGunLost(){ GunLost = true; }
}
