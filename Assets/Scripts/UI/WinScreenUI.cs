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

    // Cập nhật lại file Assets\Scripts\UI\WinScreenUI.cs
    private void OnNextLevelClicked()
    {
        if (GameManager.Instance != null)
        {
            // Nếu bạn muốn tự động tăng level và load màn mới
            // Bạn có thể gọi trực tiếp logic lưu và chuyển cảnh của GameManager
            int nextLvl = PlayerPrefs.GetInt("current_level_index", 0) + 1;
            PlayerPrefs.SetInt("current_level_index", nextLvl);
            PlayerPrefs.Save();
            
            // Load lại chính scene này để khởi tạo màn mới (do dùng chung 1 scene)
            GameManager.Instance.Replay(); 
        }
    }
}