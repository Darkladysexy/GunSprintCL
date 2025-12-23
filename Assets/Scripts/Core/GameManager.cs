using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game State")]
    private int _score;
    public int CurrentLevelScore { get; private set; }
    public bool isPaused = false;
    public bool IsLevelFinished { get; private set; } = false;

    [Header("Audio Clips")]
    [SerializeField] private AudioSource _sfxSource; 
    [SerializeField] public AudioClip ClickSfx;
    [SerializeField] private AudioClip _upgradeSfx;
    [SerializeField] private AudioClip _winSfx;
    [SerializeField] private AudioClip _loseSfx;
    [SerializeField] private AudioClip _slowMoSfx;
    [SerializeField] private AudioClip _reloadAmmoSfx;
    
        public bool IsMusicPlaying() {
        return _musicSource != null && _musicSource.isPlaying;
    }

    [Header("UI Display Mode")]
    [SerializeField] private GameObject _levelUIContainer;    
    [SerializeField] private GameObject _infinityUIContainer; 
    [SerializeField] private TextMeshProUGUI _liveAmmoText;

    [Header("UI References")]
    [SerializeField] private GameObject _pauseMenuUI;
    [SerializeField] private GameObject _winCanvasUI;
    [SerializeField] private GameObject _loseCanvasUI;
    [SerializeField] private TextMeshProUGUI _scoreText;     
    [SerializeField] private TextMeshProUGUI _winScoreText;  

    [Header("Infinity Mode UI")]
    [SerializeField] private GameObject _doneCanvasUI; 
    [SerializeField] private TextMeshProUGUI _metersText; 
    [SerializeField] private TextMeshProUGUI _bestMetersText; 
    [SerializeField] private TextMeshProUGUI _liveBestMetersText; 
    [SerializeField] private GameObject _newRecordLabel;    
    [SerializeField] private TextMeshProUGUI _totalCoinUpgradeText;
    
    [Header("Upgrade Buttons (Lv & Amount)")]
    [SerializeField] private TextMeshProUGUI _bulletLevelText;
    [SerializeField] private TextMeshProUGUI _bulletAmountText;
    [SerializeField] private TextMeshProUGUI _powerLevelText;
    [SerializeField] private TextMeshProUGUI _powerAmountText;
    [SerializeField] private TextMeshProUGUI _startLevelText;
    [SerializeField] private TextMeshProUGUI _startAmountText;

    [Header("Upgrade Costs")]
    [SerializeField] private TextMeshProUGUI _bulletCostText;
    [SerializeField] private TextMeshProUGUI _powerCostText;
    [SerializeField] private TextMeshProUGUI _startCostText;

    [Header("Audio")]
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioClip _backgroundMusic;

    [Header("Gameplay Settings")]
    public GameMode CurrentMode { get; private set; } = GameMode.Levels;
    public int Ammo { get; private set; }
    public bool GunLost { get; private set; } = false;

    private const string KEY_MODE = "gm_mode";
    private const string KEY_UP_BULLET = "up_bullets_lv";
    private const string KEY_UP_POWER = "up_power_lv";
    private const string KEY_UP_START = "up_start_lv";
    private const string KEY_TOTAL_COIN = "total_coin";
    private const string KEY_CURRENT_LEVEL = "current_level_index";
    private const string KEY_BEST_METERS = "best_meters";
    private const string KEY_SOUND_SETTING = "Setting_Sound";
    public int BulletUpgradeLevel => PlayerPrefs.GetInt(KEY_UP_BULLET, 0);
    public int PowerUpgradeLevel => PlayerPrefs.GetInt(KEY_UP_POWER, 0);
    public int StartUpgradeLevel => PlayerPrefs.GetInt(KEY_UP_START, 0);
    public int TotalCoin => PlayerPrefs.GetInt(KEY_TOTAL_COIN, 0);
    public int CurrentLevelIndex => PlayerPrefs.GetInt(KEY_CURRENT_LEVEL, 0);

    private float _cachedBestMeters;
    public float GetBestMeters() => PlayerPrefs.GetFloat(KEY_BEST_METERS, 0f);
    [SerializeField] private TextMeshProUGUI _currentLevelText;

    void Awake()
    {
        Instance = this;
        CurrentMode = (GameMode)PlayerPrefs.GetInt(KEY_MODE, (int)GameMode.Levels);
    }

    void Start()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        isPaused = false;

        _score = PlayerPrefs.GetInt(KEY_TOTAL_COIN, 0);
        CurrentLevelScore = 0;
        IsLevelFinished = false;

        _cachedBestMeters = GetBestMeters(); 

        if (CurrentMode == GameMode.Levels)
        {
            if (_levelUIContainer) _levelUIContainer.SetActive(true);
            if (_infinityUIContainer) _infinityUIContainer.SetActive(false);
            UpdateLevelUI();
        }
        else
        {
            if (_levelUIContainer) _levelUIContainer.SetActive(false);
            if (_infinityUIContainer) _infinityUIContainer.SetActive(true);
            UpdateMetersUI(0);
            
            
            if (_liveBestMetersText) _liveBestMetersText.text = "Best: " + Mathf.FloorToInt(_cachedBestMeters) + "m";
            if (_newRecordLabel) _newRecordLabel.SetActive(false);
        }

        if (_pauseMenuUI) _pauseMenuUI.SetActive(false);
        if (_winCanvasUI) _winCanvasUI.SetActive(false);
        if (_loseCanvasUI) _loseCanvasUI.SetActive(false);
        if (_doneCanvasUI) _doneCanvasUI.SetActive(false);
        int soundSetting = PlayerPrefs.GetInt(KEY_SOUND_SETTING, 1);
            ApplySoundSetting(soundSetting == 1);
        UpdateScoreUI(); 
        // PlayMusic();
    }
    public void PlaySFX(AudioClip clip) {
        if (_sfxSource && clip) _sfxSource.PlayOneShot(clip);
    }

    // --- HỆ THỐNG NÂNG CẤP ---
    public int GetUpgradeCost(int basePrice, int currentLevel) => Mathf.RoundToInt(basePrice * Mathf.Pow(1.1f, currentLevel));

    public void UpgradeBullet() {
        int cost = GetUpgradeCost(100, BulletUpgradeLevel);
        if (_score >= cost) {
            _score -= cost;
            PlayerPrefs.SetInt(KEY_UP_BULLET, BulletUpgradeLevel + 1);
            PlaySFX(_upgradeSfx);
            SaveAndRefreshUI();
        }
    }

    public void UpgradePower() {
        int cost = GetUpgradeCost(150, PowerUpgradeLevel);
        if (_score >= cost) {
            _score -= cost;
            PlayerPrefs.SetInt(KEY_UP_POWER, PowerUpgradeLevel + 1);
            PlaySFX(_upgradeSfx);
            SaveAndRefreshUI();
        }
    }

    public void UpgradeStart() {
        int cost = GetUpgradeCost(100, StartUpgradeLevel);
        if (_score >= cost) {
            _score -= cost;
            PlayerPrefs.SetInt(KEY_UP_START, StartUpgradeLevel + 1);
            PlaySFX(_upgradeSfx);
            SaveAndRefreshUI();
        }
    }

    private void SaveAndRefreshUI() {
        PlayerPrefs.SetInt(KEY_TOTAL_COIN, _score);
        PlayerPrefs.Save();
        UpdateScoreUI();
        UpdateUpgradeUI();
    }

    private void UpdateUpgradeUI() {
        if (_totalCoinUpgradeText) _totalCoinUpgradeText.text = _score.ToString();
        if (_bulletLevelText) _bulletLevelText.text = "Lv." + (BulletUpgradeLevel + 1);
        if (_powerLevelText) _powerLevelText.text = "Lv." + (PowerUpgradeLevel + 1);
        if (_startLevelText) _startLevelText.text = "Lv." + (StartUpgradeLevel + 1);

        if (_bulletAmountText) _bulletAmountText.text = GetStartBulletsBase(5).ToString();
        if (_powerAmountText) _powerAmountText.text = Mathf.RoundToInt(GunPowerMultiplier() * 100) + "%";
        if (_startAmountText) _startAmountText.text = StartingPlaceBonusMeters() + "m";

        if (_bulletCostText) _bulletCostText.text = GetUpgradeCost(100, BulletUpgradeLevel).ToString();
        if (_powerCostText) _powerCostText.text = GetUpgradeCost(150, PowerUpgradeLevel).ToString();
        if (_startCostText) _startCostText.text = GetUpgradeCost(100, StartUpgradeLevel).ToString();
    }

    // --- INFINITY LOGIC ---
    public void ShowDoneScreen(float meters)
    {
        isPaused = true;
        PlaySFX(_winSfx);
        Time.timeScale = 0f;
        if (_infinityUIContainer) _infinityUIContainer.SetActive(false);

        float oldBest = GetBestMeters();
        bool isNewRecord = meters > oldBest;

        if (isNewRecord) {
            PlayerPrefs.SetFloat(KEY_BEST_METERS, meters);
            PlayerPrefs.Save();
        }

        if (_doneCanvasUI != null) {
            _doneCanvasUI.SetActive(true);
            if (_metersText) _metersText.text = Mathf.FloorToInt(meters) + "m";
            
            if (_bestMetersText) {
                _bestMetersText.text = isNewRecord ? "NEW BEST!" : "Best: " + Mathf.FloorToInt(GetBestMeters()) + "m";
                _bestMetersText.color = isNewRecord ? Color.yellow : Color.white;
            }
            UpdateUpgradeUI(); 
        }
    }

    
    public void UpdateMetersUI(float currentMeters) {
        if (_infinityUIContainer != null) {
            var txtMeters = _infinityUIContainer.GetComponentInChildren<TextMeshProUGUI>();
            if (txtMeters) txtMeters.text = Mathf.FloorToInt(currentMeters).ToString() + "m";
            
            // Kiểm tra phá kỷ lục Live
            if (currentMeters > _cachedBestMeters && _cachedBestMeters > 0) {
                if (_newRecordLabel && !_newRecordLabel.activeSelf) {
                    _newRecordLabel.SetActive(true);
                }
                if (_liveBestMetersText) _liveBestMetersText.text = "Best: " + Mathf.FloorToInt(currentMeters) + "m";
            }
        }
    }

    // --- GAME LOGIC ---
    public void AddScore(int amount) {
        CurrentLevelScore += amount; 
        _score += amount;
        PlayerPrefs.SetInt(KEY_TOTAL_COIN, _score);
        PlayerPrefs.Save();
        UpdateScoreUI();
    }

    private void UpdateScoreUI() {
        if (_scoreText) _scoreText.text = _score.ToString();
    }

    public void UpdateLevelUI() {
        if (_currentLevelText != null) 
            _currentLevelText.text = "Level " + (CurrentLevelIndex + 1);
    }

    public void UpdateLiveAmmoUI() {
        if (_liveAmmoText != null) _liveAmmoText.text = Ammo.ToString();
    }

    public void WinLevel(int multiplier) {
        PlaySFX(_winSfx);
        if (IsLevelFinished) return;
        IsLevelFinished = true;
        int totalWinFromLevel = CurrentLevelScore * multiplier;
        _score += (CurrentLevelScore * (multiplier - 1));
        PlayerPrefs.SetInt(KEY_TOTAL_COIN, _score);
        PlayerPrefs.Save();
        UpdateScoreUI();
        ToggleSlowMo(true); 
        if (_winCanvasUI != null) {
            _winCanvasUI.SetActive(true);
            if (_winScoreText != null) _winScoreText.text = "+" + totalWinFromLevel.ToString();
        }
    }

    public void GameOver() {
        if (IsLevelFinished) return;
        isPaused = true;
        Time.timeScale = 0f;
        if (_loseCanvasUI != null) _loseCanvasUI.SetActive(true);
        PlaySFX(_loseSfx);
    }

    public void LoadScene(string sceneName) {
        PlayerPrefs.SetInt(KEY_TOTAL_COIN, _score);
        PlayerPrefs.Save();
        SceneManager.LoadScene(sceneName);
    }

    public void Replay() {
        if (_pauseMenuUI) _pauseMenuUI.SetActive(false);
        if (_winCanvasUI) _winCanvasUI.SetActive(false);
        if (_loseCanvasUI) _loseCanvasUI.SetActive(false);
        if (_doneCanvasUI) _doneCanvasUI.SetActive(false);
        LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoHome() {
        if (isPaused) TogglePause();
        LoadScene("MainMenu");
    }

    public void ToggleSlowMo(bool enabled) {
    if (!isPaused) {
        Time.timeScale = enabled ? 0.2f : 1f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        if (enabled) PlaySFX(_slowMoSfx); 
    }
}

    public void TogglePause() {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        if (_pauseMenuUI) _pauseMenuUI.SetActive(isPaused);
    }

    public void ResetProgress() {
        PlayerPrefs.DeleteAll();
        _score = 0;
        Replay(); 
    }

    public int GetStartBulletsBase(int baseBullets) => baseBullets + BulletUpgradeLevel;
    public float GunPowerMultiplier() => 1f + (0.05f * PowerUpgradeLevel);
    public float StartingPlaceBonusMeters() => 15f * StartUpgradeLevel;

    public void ResetLevelScore() => CurrentLevelScore = 0;

    public void ResetGunState(GameObject gun, Vector3 targetPos, Vector3 targetRotation) {
        GunLost = false;
        gun.transform.position = targetPos;
        gun.transform.rotation = Quaternion.Euler(targetRotation);
        Rigidbody rb = gun.GetComponent<Rigidbody>();
        if (rb != null) {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = false; 
        }
    }

    public void NotifyGunLost() => GunLost = true;

    public void SetAmmo(int value) {
        Ammo = Mathf.Max(0, value);
        UpdateLiveAmmoUI();
    }

    public void AddAmmo(int amount) {
        Ammo += amount;
        UpdateLiveAmmoUI();
    }

    public bool TryConsumeAmmo(int amount = 1) {
        if (Ammo < amount) return false;
        Ammo -= amount;
        UpdateLiveAmmoUI();
        return true;
    }

    public void NotifyEnemyKilled() { if (CurrentMode == GameMode.Infinity) AddAmmo(2); PlaySFX(_reloadAmmoSfx); }

    public void PlayMusic() {
        if (_musicSource && _backgroundMusic && !_musicSource.isPlaying) {
            _musicSource.clip = _backgroundMusic;
            _musicSource.loop = true;
            _musicSource.Play();
        }
    }

    public void ToggleSound() { 
        if (_musicSource) { 
            bool newState = !_musicSource.isPlaying;
            ApplySoundSetting(newState);
            // Lưu lại vào máy
            PlayerPrefs.SetInt(KEY_SOUND_SETTING, newState ? 1 : 0);
            PlayerPrefs.Save();
        } 
    }
    private void ApplySoundSetting(bool isOn) {
        if (_musicSource) {
            if (isOn) _musicSource.UnPause(); 
            else _musicSource.Pause();
            _musicSource.mute = !isOn; 
        }
        
        if (_sfxSource) _sfxSource.mute = !isOn;
    }
    public bool IsSoundOn() {
        return PlayerPrefs.GetInt(KEY_SOUND_SETTING, 1) == 1;
    }
}