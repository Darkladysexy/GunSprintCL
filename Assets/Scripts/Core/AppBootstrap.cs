using UnityEngine;

namespace GunSprint.Core
{
    /// <summary>
    /// Applies app-wide runtime settings once, before the first scene loads.
    /// </summary>
    public static class AppBootstrap
    {
        // Mobile platforms default to 30 fps; a physics-driven action game needs 60 for responsive taps.
        private const int k_TargetFrameRate = 60;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            Application.targetFrameRate = k_TargetFrameRate;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
    }
}
