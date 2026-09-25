using GunSprint.Data;
using UnityEngine;
using UnityEngine.UI;

namespace GunSprint.UI
{
    /// <summary>
    /// Toggles the sound setting and shows the matching icon. The setting itself is the single
    /// source of truth, so every toggle in every scene always shows the same state.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class SoundToggleButton : MonoBehaviour
    {
        [SerializeField] private PlayerProgressSO _progress;
        [SerializeField] private Image _icon;
        [SerializeField] private Sprite _onSprite;
        [SerializeField] private Sprite _offSprite;

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(Toggle);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(Toggle);
        }

        private void OnEnable()
        {
            _progress.SoundChanged += Refresh;
            Refresh(_progress.IsSoundOn);
        }

        private void OnDisable()
        {
            _progress.SoundChanged -= Refresh;
        }

        private void Toggle()
        {
            _progress.SetSoundOn(!_progress.IsSoundOn);
        }

        private void Refresh(bool isOn)
        {
            if (_icon != null)
            {
                _icon.sprite = isOn ? _onSprite : _offSprite;
            }
        }
    }
}
