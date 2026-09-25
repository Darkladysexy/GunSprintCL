using System.Collections;
using UnityEngine;

namespace GunSprint.UI
{
    /// <summary>
    /// Full-screen overlay that fades out when a scene starts, hiding the first-frame pop-in.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class ScreenFader : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] private float _fadeDuration = 0.35f;

        private IEnumerator Start()
        {
            var canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;

            float elapsed = 0f;
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = 1f - UIAnimations.EaseOutCubic(Mathf.Clamp01(elapsed / _fadeDuration));
                yield return null;
            }

            gameObject.SetActive(false);
        }
    }
}
