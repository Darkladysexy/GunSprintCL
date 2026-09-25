using UnityEngine;

namespace GunSprint.Core
{
    /// <summary>
    /// The only code allowed to write Time.timeScale.
    /// Pause always wins over slow motion, and slow motion is reference counted so overlapping
    /// requests (several bullets in flight, the win cinematic) can never cancel each other.
    /// </summary>
    public static class TimeScaleService
    {
        private static float s_baseFixedDeltaTime = 0.02f;
        private static float s_slowMotionScale = 1f;
        private static int s_slowMotionRequests;
        private static bool s_isPaused;

        public static bool IsPaused => s_isPaused;

        public static bool IsSlowMotion => s_slowMotionRequests > 0;

        public static void SetPaused(bool isPaused)
        {
            s_isPaused = isPaused;
            Apply();
        }

        /// <summary>
        /// Starts (or joins) slow motion. Every call must be matched by <see cref="EndSlowMotion"/>.
        /// </summary>
        public static void BeginSlowMotion(float scale)
        {
            s_slowMotionScale = Mathf.Clamp(scale, 0.01f, 1f);
            s_slowMotionRequests++;
            Apply();
        }

        public static void EndSlowMotion()
        {
            if (s_slowMotionRequests == 0)
            {
                return;
            }

            s_slowMotionRequests--;
            Apply();
        }

        /// <summary>
        /// Clears pause and every slow motion request. Called before each scene load.
        /// </summary>
        public static void Reset()
        {
            s_slowMotionRequests = 0;
            s_isPaused = false;
            Apply();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            // Captured from the project settings so slow motion can scale the physics step and restore it.
            s_baseFixedDeltaTime = Time.fixedDeltaTime;
            s_slowMotionScale = 1f;
            s_slowMotionRequests = 0;
            s_isPaused = false;
        }

        private static void Apply()
        {
            float scale = s_slowMotionRequests > 0 ? s_slowMotionScale : 1f;
            Time.timeScale = s_isPaused ? 0f : scale;

            // Shrinking the physics step with the time scale keeps slow motion smooth instead of choppy.
            Time.fixedDeltaTime = s_baseFixedDeltaTime * scale;
        }
    }
}
