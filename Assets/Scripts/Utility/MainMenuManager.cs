using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    private const string SceneGame = "Gunsprint";
    private const string KEY_MODE = "gm_mode";

    public void PlayLevels(){
        PlayerPrefs.SetInt(KEY_MODE, (int)GameMode.Levels);
        SceneManager.LoadScene(SceneGame);
    }

    public void PlayInfinity(){
        PlayerPrefs.SetInt(KEY_MODE, (int)GameMode.Infinity);
        SceneManager.LoadScene(SceneGame);
    }

    public void QuitGame() => Application.Quit();
}
