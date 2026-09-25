using System.Collections;
using TMPro;
using UnityEngine;

namespace GunSprint.UI
{
    /// <summary>
    /// Tiny tweening helpers for UI juice. Everything runs on unscaled time so it also works
    /// while the game is paused or in slow motion.
    /// </summary>
    public static class UIAnimations
    {
        private const float k_BackOvershoot = 1.70158f;

        public static float EaseOutCubic(float t)
        {
            float inverse = 1f - t;
            return 1f - inverse * inverse * inverse;
        }

        /// <summary>Overshoots slightly before settling: the classic "pop".</summary>
        public static float EaseOutBack(float t)
        {
            float shifted = t - 1f;
            return 1f + (k_BackOvershoot + 1f) * shifted * shifted * shifted + k_BackOvershoot * shifted * shifted;
        }

        /// <summary>Quickly scales the target up and back to its original scale.</summary>
        public static IEnumerator Punch(Transform target, Vector3 baseScale, float strength, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                target.localScale = baseScale * (1f + strength * Mathf.Sin(t * Mathf.PI));
                yield return null;
            }

            target.localScale = baseScale;
        }

        /// <summary>Counts a number up in a label, e.g. format "+{0}". Uses TMP's allocation-free SetText.</summary>
        public static IEnumerator CountUp(TMP_Text label, string format, int from, int to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = EaseOutCubic(Mathf.Clamp01(elapsed / duration));
                label.SetText(format, Mathf.RoundToInt(Mathf.Lerp(from, to, t)));
                yield return null;
            }

            label.SetText(format, to);
        }
    }
}
