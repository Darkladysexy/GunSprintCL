using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WinScreenUI : MonoBehaviour
{
    [SerializeField] private Button _nextLevelButton;

    private void Awake()
    {
        if (_nextLevelButton != null)
            _nextLevelButton.onClick.AddListener(OnNextLevelClicked);
    }

    private void OnNextLevelClicked()
    {
        if (GameManager.Instance != null)
        {
            // Gọi hàm này để đặt IsLevelFinished = false
            GameManager.Instance.NextLevel();
        }
    }
}