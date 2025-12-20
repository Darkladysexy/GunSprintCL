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
    [SerializeField] private GameObject _loseCanvasUI;
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
    private const string KEY_TOTAL_COIN = "total_coin";
    private const string KEY_CURRENT_LEVEL = "current_level_index";

    // --- HỆ THỐNG NÂNG CẤP (UPGRADES) ---
    public int BulletUpgradeLevel => PlayerPrefs.GetInt(KEY_UP_BULLET, 0);
    public int PowerUpgradeLevel => PlayerPrefs.GetInt(KEY_UP_POWER, 0);
    public int StartUpgradeLevel => PlayerPrefs.GetInt(KEY_UP_START, 0);

    // Tính toán chỉ số dựa trên Level nâng cấp
    public int GetStartBulletsBase(int baseBullets) => baseBullets + BulletUpgradeLevel;
    public float GunPowerMultiplier() => 1f + 0.02f * PowerUpgradeLevel;
    public float StartingPlaceBonusMeters() => 20f * StartUpgradeLevel;
    // Thêm các thuộc tính mới
    public int TotalCoin => PlayerPrefs.GetInt(KEY_TOTAL_COIN, 0);
    public int CurrentLevelIndex => PlayerPrefs.GetInt(KEY_CURRENT_LEVEL, 0);

    [SerializeField] private TextMeshProUGUI _currentLevelText; // Text hiện "Level 1", "Level 2"...

    // --- KHỞI TẠO ---
    void Awake()
    {
        Instance = this;

        // Tải chế độ chơi
        CurrentMode = (GameMode)PlayerPrefs.GetInt(KEY_MODE, (int)GameMode.Levels);
    }

    void Start()
    {
        // Luôn đặt lại thời gian về 1 khi bắt đầu Scene mới
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        isPaused = false;

        // Tải dữ liệu Coin và Level từ bộ nhớ
        _score = PlayerPrefs.GetInt(KEY_TOTAL_COIN, 0);
        
        CurrentLevelScore = 0;
        IsLevelFinished = false;

        // Khởi tạo UI
        if (_pauseMenuUI) _pauseMenuUI.SetActive(false);
        if (_winCanvasUI) _winCanvasUI.SetActive(false);
        if (_loseCanvasUI) _loseCanvasUI.SetActive(false);
        
        UpdateScoreUI();
        UpdateLevelUI();
        PlayMusic();
    }
    void Update()
    {
        // Nhấn phím R trên bàn phím để reset nhanh khi đang test (chỉ chạy trong Editor)
        #if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetProgress();
        }
        #endif
    }
    // Hàm cập nhật Text Level
    public void UpdateLevelUI()
    {
        if (_currentLevelText != null) 
            _currentLevelText.text = "Level " + (CurrentLevelIndex + 1);
    }
    // --- QUẢN LÝ ĐIỂM SỐ ---
    public void AddScore(int amount)
    {
        // 1. Cộng vào điểm tạm thời của màn (để dùng nhân hệ số khi thắng)
        CurrentLevelScore += amount; 

        // 2. Cộng vào tổng số coin và lưu lại
        _score += amount;
        PlayerPrefs.SetInt(KEY_TOTAL_COIN, _score);
        PlayerPrefs.Save();

        // 3. Cập nhật UI ngay lập tức
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

        // Tính số điểm thưởng thêm từ hệ số nhân (multiplier)
        // Ví dụ: ăn 10 điểm, nhân 5 => tổng nhận 50. Vì đã cộng 10 lúc bắn, giờ cộng nốt 40.
        int bonusAmount = CurrentLevelScore * (multiplier - 1);
        int totalWinFromLevel = CurrentLevelScore * multiplier;
        
        _score += bonusAmount;
        PlayerPrefs.SetInt(KEY_TOTAL_COIN, _score);
        PlayerPrefs.Save();

        UpdateScoreUI();
        ToggleSlowMo(true); 

        if (_winCanvasUI != null)
        {
            _winCanvasUI.SetActive(true);
            if (_winScoreText != null) _winScoreText.text = "+" + totalWinFromLevel.ToString();
        }
    }
    public void GameOver()
    {
        if (IsLevelFinished) return; // Nếu đã thắng thì không tính thua nữa
        
        isPaused = true;
        Time.timeScale = 0f; // Dừng game
        
        if (_loseCanvasUI != null)
        {
            _loseCanvasUI.SetActive(true);
        }
    }
    // Chuyển sang màn chơi tiếp theo (được gọi từ Button trên WinCanvas)
    public void NextLevel()
    {
        IsLevelFinished = false;
        
        // Tăng level index và lưu lại
        int nextLvl = CurrentLevelIndex + 1;
        PlayerPrefs.SetInt(KEY_CURRENT_LEVEL, nextLvl);
        PlayerPrefs.Save();

        UpdateLevelUI();

        if (_winCanvasUI) _winCanvasUI.SetActive(false);
        ToggleSlowMo(false);
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
        // Trước khi đi, lưu lại dữ liệu lần cuối cho chắc chắn
        PlayerPrefs.SetInt(KEY_TOTAL_COIN, _score);
        PlayerPrefs.Save();

        // Chuyển cảnh (Scene mới sẽ tự tạo GameManager mới với các liên kết UI mới)
        SceneManager.LoadScene(sceneName);
    }

    public void Replay()
    {
            // Đảm bảo tắt bảng UI trước khi load lại
        if (_pauseMenuUI) _pauseMenuUI.SetActive(false);
        if (_winCanvasUI) _winCanvasUI.SetActive(false);
        if (_loseCanvasUI) _loseCanvasUI.SetActive(false);
        // IsLevelFinished = false;
        LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoHome()
    {
        if (isPaused) TogglePause();
        LoadScene("MainMenu");
    }

    // Thêm vào trong class GameManager trong file Scripts/Utility/GameManager.cs

    public void ResetProgress()
    {
        // Xóa toàn bộ dữ liệu lưu trữ liên quan đến game
        PlayerPrefs.DeleteKey(KEY_TOTAL_COIN);
        PlayerPrefs.DeleteKey(KEY_CURRENT_LEVEL);
        
        // Nếu bạn muốn xóa cả các nâng cấp (Upgrades), hãy thêm các dòng dưới đây:
        PlayerPrefs.DeleteKey(KEY_UP_BULLET);
        PlayerPrefs.DeleteKey(KEY_UP_POWER);
        PlayerPrefs.DeleteKey(KEY_UP_START);

        PlayerPrefs.Save();

        // Reset các biến tạm thời trong code
        _score = 0;
        CurrentLevelScore = 0;
        
        // Cập nhật lại giao diện ngay lập tức
        UpdateScoreUI();
        UpdateLevelUI();

        Debug.Log("Đã xóa toàn bộ tiến trình game (Coin và Level) để Test.");
        
        // Tùy chọn: Load lại scene hiện tại để mọi thứ reset hoàn toàn
        Replay(); 
    }
}