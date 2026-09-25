using System.Collections;
using GunSprint.Audio;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GunSprint.UI
{
    /// <summary>
    /// Tactile feedback for buttons: shrinks while pressed and plays a click sound.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField, Range(0.5f, 1f)] private float _pressedScale = 0.92f;
        [SerializeField, Min(0.01f)] private float _duration = 0.08f;
        [SerializeField] private AudioCueSO _clickCue;

        private Button _button;
        private Vector3 _baseScale;
        private Coroutine _scaling;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _baseScale = transform.localScale;
            _button.onClick.AddListener(PlayClick);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(PlayClick);
        }

        private void OnDisable()
        {
            _scaling = null;
            transform.localScale = _baseScale;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_button.interactable)
            {
                ScaleTo(_baseScale * _pressedScale);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ScaleTo(_baseScale);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ScaleTo(_baseScale);
        }

        private void PlayClick()
        {
            if (_clickCue != null)
            {
                _clickCue.Play();
            }
        }

        private void ScaleTo(Vector3 target)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            if (_scaling != null)
            {
                StopCoroutine(_scaling);
            }

            _scaling = StartCoroutine(AnimateScale(target));
        }

        private IEnumerator AnimateScale(Vector3 target)
        {
            Vector3 start = transform.localScale;
            float elapsed = 0f;
            while (elapsed < _duration)
            {
                elapsed += Time.unscaledDeltaTime;
                transform.localScale = Vector3.Lerp(start, target, UIAnimations.EaseOutCubic(Mathf.Clamp01(elapsed / _duration)));
                yield return null;
            }

            transform.localScale = target;
            _scaling = null;
        }
    }
}
