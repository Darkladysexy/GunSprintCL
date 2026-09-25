using UnityEngine;

namespace GunSprint.UI
{
    /// <summary>
    /// Fits this RectTransform to the device safe area, keeping buttons and text away from notches,
    /// rounded corners and the home indicator. Put interactive UI inside it; full-screen backgrounds outside.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeArea : MonoBehaviour
    {
        private RectTransform _rectTransform;
        private Rect _appliedArea;
        private Vector2Int _appliedScreenSize;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            Apply();
        }

        private void Update()
        {
            // Rotation, split screen or resizing can change the safe area at runtime.
            if (Screen.safeArea != _appliedArea || Screen.width != _appliedScreenSize.x || Screen.height != _appliedScreenSize.y)
            {
                Apply();
            }
        }

        private void Apply()
        {
            if (_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
            }

            Rect area = Screen.safeArea;
            _appliedArea = area;
            _appliedScreenSize = new Vector2Int(Screen.width, Screen.height);

            if (Screen.width <= 0 || Screen.height <= 0)
            {
                return;
            }

            Vector2 anchorMin = area.position;
            Vector2 anchorMax = area.position + area.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;
            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;
        }
    }
}
