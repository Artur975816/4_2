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
    public UnityEngine.UI.Text coinsLegacyText;
    public UnityEngine.UI.Text livesLegacyText;
    public UnityEngine.UI.Text messageText;
    Coroutine messageRoutine;

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
            coinsText.text = GameManager.Instance != null
                ? "\u041C\u043E\u043D\u0435\u0442\u044B \u0443\u0440\u043E\u0432\u043D\u044F: " + GameManager.Instance.LevelCoinsCollected + "/" + GameManager.Instance.LevelCoinsRequired
                : "\u041C\u043E\u043D\u0435\u0442\u044B \u0443\u0440\u043E\u0432\u043D\u044F: 0/0";
        if (livesText != null)
            livesText.text = "\u0416\u0438\u0437\u043D\u0438: " + (GameManager.Instance != null ? GameManager.Instance.Lives : 3);
        if (coinsLegacyText != null) coinsLegacyText.text = "\u041C\u043E\u043D\u0435\u0442\u044B \u0443\u0440\u043E\u0432\u043D\u044F: " + (GameManager.Instance != null ? GameManager.Instance.LevelCoinsCollected : 0) + "/" + (GameManager.Instance != null ? GameManager.Instance.LevelCoinsRequired : 0);
        if (livesLegacyText != null) livesLegacyText.text = "\u0416\u0438\u0437\u043D\u0438: " + (GameManager.Instance != null ? GameManager.Instance.Lives : 3);
    }

    public void ShowMessage(string message)
    {
        if (messageText == null) return;
        messageText.text = message;
        messageText.gameObject.SetActive(true);
        if (messageRoutine != null) StopCoroutine(messageRoutine);
        messageRoutine = StartCoroutine(HideMessage());
    }

    System.Collections.IEnumerator HideMessage()
    {
        yield return new WaitForSecondsRealtime(2.2f);
        if (messageText != null) messageText.gameObject.SetActive(false);
    }

    public void ShowPause(bool paused)
    {
        if (pausePanel != null) pausePanel.SetActive(paused);
    }

    public void ShowWin()
    {
        if (winPanel == null) return;
        winPanel.SetActive(true);
        var texts = winPanel.GetComponentsInChildren<UnityEngine.UI.Text>(true);
        if (texts.Length > 0) texts[0].text = "\u0422\u042B \u041F\u041E\u0411\u0415\u0414\u0418\u041B!";
        if (texts.Length > 1) texts[1].text = "\u0412\u043E\u0437\u0432\u0440\u0430\u0449\u0430\u0435\u043C\u0441\u044F \u043A \u0432\u044B\u0431\u043E\u0440\u0443 \u0443\u0440\u043E\u0432\u043D\u044F...";
        if (texts.Length > 2) texts[2].text = "\u0412\u042B\u0411\u0420\u0410\u0422\u042C \u0423\u0420\u041E\u0412\u0415\u041D\u042C";
        var tmpTexts = winPanel.GetComponentsInChildren<TMP_Text>(true);
        if (tmpTexts.Length > 0) tmpTexts[0].text = "\u0422\u042B \u041F\u041E\u0411\u0415\u0414\u0418\u041B!";
        if (tmpTexts.Length > 1) tmpTexts[1].text = "\u0412\u043E\u0437\u0432\u0440\u0430\u0449\u0430\u0435\u043C\u0441\u044F \u043A \u0432\u044B\u0431\u043E\u0440\u0443 \u0443\u0440\u043E\u0432\u043D\u044F...";
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
        if (GameManager.Instance != null) GameManager.Instance.RestartCurrentLevel();
        else { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }
    }

    public void LoadScene(string sceneName)
    {
        if (GameManager.Instance != null) GameManager.Instance.LoadScene(sceneName);
        else { Time.timeScale = 1f; SceneManager.LoadScene(sceneName); }
    }

    public void LoadNextLevel()
    {
        if (GameManager.Instance != null) GameManager.Instance.LoadNextLevel();
        else
        {
            Time.timeScale = 1f;
            string current = SceneManager.GetActiveScene().name;
            string next = current == "Level1" ? "Level2" : current == "Level2" ? "Level3" : "MainMenu";
            SceneManager.LoadScene(next);
        }
    }

    public void LoadLevelSelect() => LoadScene("LevelSelect");

    public void QuitGame() => Application.Quit();
}
