using UnityEngine;
using UnityEngine.UI;

public class SoundToggleButton : MonoBehaviour
{
    private Image _buttonImage;
    [SerializeField] private Sprite _onSprite;
    [SerializeField] private Sprite _offSprite;
    private bool _isSoundOn = true;

    void Awake()
    {
        _buttonImage = GetComponent<Image>();
    }

    void Start()
    {
        // Luôn đọc từ PlayerPrefs để hiển thị đúng icon Loa khi chuyển cảnh
        int soundSetting = PlayerPrefs.GetInt("Setting_Sound", 1);
        _isSoundOn = (soundSetting == 1);
        _buttonImage.sprite = _isSoundOn ? _onSprite : _offSprite;
    }

    public void ToggleButton()
    {
        _isSoundOn = !_isSoundOn;
        _buttonImage.sprite = _isSoundOn ? _onSprite : _offSprite;

        // Gửi lệnh thay đổi tới Manager tương ứng của Scene đó
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ToggleSound();
        }
        else
        {
            MainMenuManager mainMenu = Object.FindFirstObjectByType<MainMenuManager>();
            if (mainMenu != null)
            {
                mainMenu.ToggleSound();
            }
        }
    }
}