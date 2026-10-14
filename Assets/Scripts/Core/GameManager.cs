using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int Coins { get; private set; }
    public int Lives { get; private set; } = 3;
    public bool IsPaused { get; private set; }
    public bool IsFinished { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Time.timeScale = 1f;
    }

    public void AddCoin(int amount = 1) => Coins += amount;

    public void LoseLife()
    {
        if (IsFinished) return;
        Lives--;
        if (Lives <= 0) GameOver();
        else RestartCurrentLevel();
    }

    public void WinLevel()
    {
        IsFinished = true;
        Time.timeScale = 0f;
        if (UIManager.Instance != null) UIManager.Instance.ShowWin();
    }

    public void GameOver()
    {
        IsFinished = true;
        Time.timeScale = 0f;
        if (UIManager.Instance != null) UIManager.Instance.ShowGameOver();
    }

    public void TogglePause()
    {
        if (IsFinished) return;
        IsPaused = !IsPaused;
        Time.timeScale = IsPaused ? 0f : 1f;
        if (UIManager.Instance != null) UIManager.Instance.ShowPause(IsPaused);
    }

    public void RestartCurrentLevel()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        IsFinished = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        IsPaused = false;
        IsFinished = false;
        SceneManager.LoadScene(sceneName);
    }

    public void ReturnToMenu() => LoadScene("MainMenu");
}
