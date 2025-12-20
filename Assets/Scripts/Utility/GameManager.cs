using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    // Singleton Instance để truy cập từ mọi nơi
    public static GameManager Instance;

    [Header("Game State")]
    private int _score;
    public int CurrentLevelScore { get; private set; } // Điểm số tính riêng cho màn chơi hiện tại
    public bool isPaused = false;
    public bool IsLevelFinished { get; private set; } = false; // Trạng thái đã bắn trúng stack nhân điểm

    [Header("UI References")]
    [SerializeField] private GameObject _pauseMenuUI;
    [SerializeField] private GameObject _winCanvasUI;
    [SerializeField] private TextMeshProUGUI _scoreText;     // Text hiển thị tổng điểm trên HUD
    [SerializeField] private TextMeshProUGUI _winScoreText;  // Text hiển thị số điểm nhận được trên Win Canvas

    [Header("Audio")]
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioClip _backgroundMusic;

    [Header("Gameplay Settings")]
    public GameMode CurrentMode { get; private set; } = GameMode.Levels;
    public int Ammo { get; private set; }
    public bool GunLost { get; private set; } = false;

    // --- HẰNG SỐ PLAYERPREFS ---
    private const string KEY_MODE = "gm_mode";
    private const string KEY_UP_BULLET = "up_bullets_lv";
    private const string KEY_UP_POWER = "up_power_lv";
    private const string KEY_UP_START = "up_start_lv";

    // --- HỆ THỐNG NÂNG CẤP (UPGRADES) ---
    public int BulletUpgradeLevel => PlayerPrefs.GetInt(KEY_UP_BULLET, 0);
    public int PowerUpgradeLevel => PlayerPrefs.GetInt(KEY_UP_POWER, 0);
    public int StartUpgradeLevel => PlayerPrefs.GetInt(KEY_UP_START, 0);

    // Tính toán chỉ số dựa trên Level nâng cấp
    public int GetStartBulletsBase(int baseBullets) => baseBullets + BulletUpgradeLevel;
    public float GunPowerMultiplier() => 1f + 0.02f * PowerUpgradeLevel;
    public float StartingPlaceBonusMeters() => 20f * StartUpgradeLevel;

    // --- KHỞI TẠO ---
    void Awake()
    {
        // Thiết lập Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Tải chế độ chơi từ bộ nhớ
        CurrentMode = (GameMode)PlayerPrefs.GetInt(KEY_MODE, (int)GameMode.Levels);
    }

    void Start()
    {
        _score = 0;
        CurrentLevelScore = 0;
        
        // Khởi tạo trạng thái UI
        if (_pauseMenuUI) _pauseMenuUI.SetActive(false);
        if (_winCanvasUI) _winCanvasUI.SetActive(false);
        
        UpdateScoreUI();
        PlayMusic();
    }

    // --- QUẢN LÝ ĐIỂM SỐ ---
    public void AddScore(int amount)
    {
        _score += amount;
        CurrentLevelScore += amount;
        UpdateScoreUI();
    }

    public void ResetLevelScore() => CurrentLevelScore = 0;

    private void UpdateScoreUI()
    {
        if (_scoreText) _scoreText.text = _score.ToString();
    }

    // --- LOGIC CHIẾN THẮNG (WIN) ---
    public void WinLevel(int multiplier)
    {
        if (IsLevelFinished) return;
        IsLevelFinished = true;

        // Tính toán điểm thưởng
        int totalWinFromLevel = CurrentLevelScore * multiplier;
        int bonusAmount = CurrentLevelScore * (multiplier - 1);
        
        _score += bonusAmount;
        UpdateScoreUI();

        // Kích hoạt hiệu ứng quay chậm (Slow-mo) khi thắng
        ToggleSlowMo(true); 

        // Hiển thị giao diện chiến thắng
        if (_winCanvasUI != null)
        {
            _winCanvasUI.SetActive(true);
            if (_winScoreText != null) _winScoreText.text = "+" + totalWinFromLevel.ToString();
        }
    }

    // Chuyển sang màn chơi tiếp theo (được gọi từ Button trên WinCanvas)
    public void NextLevel()
    {
        IsLevelFinished = false;
        if (_winCanvasUI) _winCanvasUI.SetActive(false);
        ToggleSlowMo(false);
        // Lưu ý: Logic tải màn mới hoặc reset vị trí súng sẽ do LevelModeDistance xử lý
    }

    // --- QUẢN LÝ VẬT LÝ SÚNG ---
    public void ResetGunState(GameObject gun, Vector3 targetPos, Vector3 targetRotation)
    {
        GunLost = false;
        gun.transform.position = targetPos;
        gun.transform.rotation = Quaternion.Euler(targetRotation);
        
        Rigidbody rb = gun.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Tạm dừng vật lý để đặt vị trí chính xác
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = false; 
        }
    }

    public void NotifyGunLost() => GunLost = true;

    // --- HỆ THỐNG ĐẠN (AMMO) ---
    public void SetAmmo(int value) => Ammo = Mathf.Max(0, value);
    public void AddAmmo(int amount) => Ammo += amount;
    
    public bool TryConsumeAmmo(int amount = 1)
    {
        if (Ammo < amount) return false;
        Ammo -= amount;
        return true;
    }

    public void NotifyEnemyKilled()
    {
        // Trong chế độ Infinity, giết địch được hồi 2 viên đạn
        if (CurrentMode == GameMode.Infinity) AddAmmo(2);
    }

    // --- ĐIỀU KHIỂN THỜI GIAN & TẠM DỪNG ---
    public void ToggleSlowMo(bool enabled)
    {
        if (!isPaused)
        {
            Time.timeScale = enabled ? 0.2f : 1f;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        if (_pauseMenuUI) _pauseMenuUI.SetActive(isPaused);
    }

    // --- ÂM THANH & CHUYỂN CẢNH ---
    public void ToggleSound()
    {
        if (_musicSource)
        {
            if (_musicSource.isPlaying) _musicSource.Pause();
            else _musicSource.UnPause();
        }
    }

    public void PlayMusic()
    {
        if (_musicSource && _backgroundMusic && !_musicSource.isPlaying)
        {
            _musicSource.clip = _backgroundMusic;
            _musicSource.loop = true;
            _musicSource.Play();
        }
    }

    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        isPaused = false;
        // Xóa instance hiện tại trước khi chuyển cảnh nếu cần thiết (tùy thuộc vào thiết kế logic)
        Destroy(gameObject); 
        SceneManager.LoadScene(sceneName);
    }

    public void Replay()
    {
        if (isPaused) TogglePause();
        LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoHome()
    {
        if (isPaused) TogglePause();
        LoadScene("MainMenu");
    }
}