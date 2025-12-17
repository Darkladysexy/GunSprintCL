using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

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
        if (GameManager.Instance != null && GameManager.Instance.isPaused == false) {
             _isSoundOn = true;
             _buttonImage.sprite = _onSprite;
        } else {
             _isSoundOn = false;
             _buttonImage.sprite = _offSprite;
        }
    }

    public void ToggleButton()
    {
        _isSoundOn = !_isSoundOn;
        _buttonImage.sprite = _isSoundOn ? _onSprite : _offSprite;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ToggleSound();
        }
    }
}