using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    private const string SceneGame = "Gunsprint";
    private const string KEY_MODE = "gm_mode";
    private const string KEY_SOUND_SETTING = "Setting_Sound";

    [Header("Audio")]
    [SerializeField] private AudioSource _bgmSource;

    void Start() {
        // Áp dụng cài đặt âm thanh ngay khi vào Main Menu
        int soundSetting = PlayerPrefs.GetInt(KEY_SOUND_SETTING, 1);
        if (_bgmSource != null) {
            _bgmSource.mute = (soundSetting == 0);
            if (soundSetting == 1) {
                if (!_bgmSource.isPlaying) _bgmSource.Play();
            } else {
                _bgmSource.Stop();
            }
        }
    }

    public void ToggleSound() { 
        if (_bgmSource != null) { 
            bool isOn = !_bgmSource.isPlaying;
            if (isOn) _bgmSource.Play(); 
            else _bgmSource.Stop();
            _bgmSource.mute = !isOn;

            // Lưu cài đặt để Game Scene có thể đọc được
            PlayerPrefs.SetInt(KEY_SOUND_SETTING, isOn ? 1 : 0);
            PlayerPrefs.Save();
        } 
    }

    public void PlayLevels(){
        PlayerPrefs.SetInt(KEY_MODE, (int)GameMode.Levels);
        SceneManager.LoadScene(SceneGame);
    }

    public void PlayInfinity(){
        PlayerPrefs.SetInt(KEY_MODE, (int)GameMode.Infinity);
        SceneManager.LoadScene(SceneGame);
    }

    public void QuitGame() => Application.Quit();
}