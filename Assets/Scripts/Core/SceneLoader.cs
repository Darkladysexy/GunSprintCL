using UnityEngine.SceneManagement;

namespace GunSprint.Core
{
    /// <summary>
    /// Single place that knows scene names and how to switch between them.
    /// </summary>
    public static class SceneLoader
    {
        public const string k_MainMenuScene = "MainMenu";
        public const string k_GameplayScene = "GunSprint";

        public static void LoadMainMenu()
        {
            Load(k_MainMenuScene);
        }

        public static void LoadGameplay()
        {
            Load(k_GameplayScene);
        }

        public static void ReloadActiveScene()
        {
            Load(SceneManager.GetActiveScene().name);
        }

        private static void Load(string sceneName)
        {
            // Time.timeScale survives scene loads, so a scene must never start paused or in slow motion.
            TimeScaleService.Reset();
            SceneManager.LoadScene(sceneName);
        }
    }
}
