using System.Collections;
using UnityEngine;

namespace GunSprint.UI
{
    /// <summary>
    /// Base class for modal popups: fades the whole popup (dim backdrop included) and pops its content in.
    /// While hidden the GameObject is inactive, so it costs nothing and its canvas is not rebuilt.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class UIPopup : MonoBehaviour
    {
        [Tooltip("The panel that scales in. The root (with the dim backdrop) only fades.")]
        [SerializeField] private RectTransform _content;

        [SerializeField, Min(0.01f)] private float _showDuration = 0.3f;
        [SerializeField, Min(0.01f)] private float _hideDuration = 0.15f;

        [Tooltip("Scale the content pops in from.")]
        [SerializeField, Range(0f, 1f)] private float _startScale = 0.7f;

        private CanvasGroup _canvasGroup;
        private Coroutine _transition;

        public bool IsShown { get; private set; }

        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public void Show()
        {
            IsShown = true;
            gameObject.SetActive(true);
            OnShowing();
            StartTransition(true);
        }

        public void Hide()
        {
            if (!IsShown)
            {
                return;
            }

            IsShown = false;
            if (gameObject.activeInHierarchy)
            {
                StartTransition(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        public void HideImmediate()
        {
            IsShown = false;
            if (_transition != null)
            {
                StopCoroutine(_transition);
                _transition = null;
            }

            gameObject.SetActive(false);
        }

        /// <summary>Refresh the popup's content here; called right before it appears.</summary>
        protected virtual void OnShowing()
        {
        }

        private void StartTransition(bool isShowing)
        {
            if (_transition != null)
            {
                StopCoroutine(_transition);
            }

            _transition = StartCoroutine(Animate(isShowing));
        }

        private IEnumerator Animate(bool isShowing)
        {
            // Block clicks underneath immediately, but only accept clicks once fully visible.
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = false;

            float duration = isShowing ? _showDuration : _hideDuration;
            float fromAlpha = isShowing ? 0f : _canvasGroup.alpha;
            float toAlpha = isShowing ? 1f : 0f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                _canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, UIAnimations.EaseOutCubic(t));
                if (_content != null)
                {
                    float scale = isShowing
                        ? Mathf.LerpUnclamped(_startScale, 1f, UIAnimations.EaseOutBack(t))
                        : Mathf.Lerp(1f, _startScale, t);
                    _content.localScale = new Vector3(scale, scale, 1f);
                }

                yield return null;
            }

            _canvasGroup.alpha = toAlpha;
            if (_content != null)
            {
                _content.localScale = Vector3.one;
            }

            _canvasGroup.interactable = isShowing;
            _canvasGroup.blocksRaycasts = isShowing;
            _transition = null;

            if (!isShowing)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
