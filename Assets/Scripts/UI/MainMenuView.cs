using GunSprint.Core;
using GunSprint.Data;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GunSprint.UI
{
    /// <summary>
    /// Main menu: picks a mode and shows the player's progress for each one.
    /// </summary>
    public class MainMenuView : MonoBehaviour
    {
        [SerializeField] private PlayerProgressSO _progress;
        [SerializeField] private Button _playLevelsButton;
        [SerializeField] private Button _playEndlessButton;
        [SerializeField] private Button _quitButton;
        [SerializeField] private Button _shopButton;
        [SerializeField] private ShopView _shopView;
        [SerializeField] private TMP_Text _levelLabel;
        [SerializeField] private TMP_Text _bestLabel;

        private void Awake()
        {
            _playLevelsButton.onClick.AddListener(PlayLevels);
            _playEndlessButton.onClick.AddListener(PlayEndless);
            _quitButton.onClick.AddListener(Application.Quit);
            _shopButton.onClick.AddListener(_shopView.Open);

            // iOS apps must not quit themselves, and the web player cannot.
            bool canQuit = Application.platform != RuntimePlatform.IPhonePlayer
                && Application.platform != RuntimePlatform.WebGLPlayer;
            _quitButton.gameObject.SetActive(canQuit);
        }

        private void OnDestroy()
        {
            _playLevelsButton.onClick.RemoveListener(PlayLevels);
            _playEndlessButton.onClick.RemoveListener(PlayEndless);
            _quitButton.onClick.RemoveListener(Application.Quit);
            _shopButton.onClick.RemoveListener(_shopView.Open);
        }

        private void OnEnable()
        {
            _levelLabel.SetText("LEVEL {0}", _progress.LevelIndex + 1);

            int best = Mathf.FloorToInt(_progress.BestDistance);
            if (best > 0)
            {
                _bestLabel.SetText("BEST {0} m", best);
            }
            else
            {
                _bestLabel.SetText("NEW!");
            }
        }

        private void Update()
        {
            // Android back button: closes the shop first, then leaves the game, as players expect.
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame)
            {
                return;
            }

            if (_shopView.IsShown)
            {
                _shopView.Close();
            }
            else if (_quitButton.gameObject.activeSelf)
            {
                Application.Quit();
            }
        }

        private void PlayLevels()
        {
            StartGame(GameMode.Levels);
        }

        private void PlayEndless()
        {
            StartGame(GameMode.Infinity);
        }

        private void StartGame(GameMode mode)
        {
            _progress.SelectedMode = mode;
            _progress.Save();
            SceneLoader.LoadGameplay();
        }
    }
}
