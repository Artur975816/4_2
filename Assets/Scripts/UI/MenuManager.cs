using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public void Play() => UnityEngine.SceneManagement.SceneManager.LoadScene("LevelSelect");
    public void OpenSettings() => UnityEngine.SceneManagement.SceneManager.LoadScene("Settings");
    public void BackToMenu() => UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    public void Quit() => Application.Quit();
}
