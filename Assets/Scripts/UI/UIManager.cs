using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels")]
    public GameObject pausePanel;
    public GameObject winPanel;
    public GameObject gameOverPanel;
    public TMP_Text coinsText;
    public TMP_Text livesText;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        HideAllResultPanels();
        RefreshHUD();
    }

    private void HideAllResultPanels()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    public void RefreshHUD()
    {
        if (coinsText != null)
            coinsText.text = "Монеты: " + (GameManager.Instance != null ? GameManager.Instance.Coins : 0)
                + "  |  Всего: " + SaveSystem.TotalCoins;
        if (livesText != null)
            livesText.text = "Жизни: " + (GameManager.Instance != null ? GameManager.Instance.Lives : 3);
    }

    public void ShowPause(bool paused)
    {
        if (pausePanel != null) pausePanel.SetActive(paused);
    }

    public void ShowWin()
    {
        if (winPanel != null) winPanel.SetActive(true);
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    public void Resume()
    {
        if (GameManager.Instance != null) GameManager.Instance.TogglePause();
        else Time.timeScale = 1f;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public void LoadNextLevel()
    {
        Time.timeScale = 1f;
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        if (next < SceneManager.sceneCountInBuildSettings) SceneManager.LoadScene(next);
        else LoadScene("MainMenu");
    }

    public void QuitGame() => Application.Quit();
}
