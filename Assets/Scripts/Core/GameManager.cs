using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int Coins { get; private set; }
    public int LevelCoinsCollected { get; private set; }
    public int LevelCoinsRequired { get; private set; }
    public bool HasCollectedAllLevelCoins => LevelCoinsCollected >= LevelCoinsRequired;
    public int Lives { get; private set; } = 3;
    public int JumpsUsed { get; private set; }
    public bool IsPaused { get; private set; }
    public bool IsFinished { get; private set; }
    Vector3 checkpointPosition;
    bool hasCheckpoint;
    string checkpointScene;
    string lastSceneName;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Time.timeScale = 1f;
        lastSceneName = SceneManager.GetActiveScene().name;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string previousScene = lastSceneName;
        lastSceneName = scene.name;
        if (scene.name.StartsWith("Level"))
        {
            Coins = 0;
            LevelCoinsCollected = 0;
            LevelCoinsRequired = FindObjectsByType<Coin>(FindObjectsSortMode.None).Length;
            IsPaused = false;
            IsFinished = false;
            Time.timeScale = 1f;
            if (previousScene == "MainMenu" || previousScene == "LevelSelect") Lives = 3;
        }
        if (!scene.name.StartsWith("Level")) return;
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        checkpointPosition = player.transform.position;
        checkpointScene = scene.name;
        hasCheckpoint = true;
        if (player.GetComponent<Assignment6.Health>() == null) player.AddComponent<Assignment6.Health>();
        if (player.GetComponent<Shooter1>() == null) player.AddComponent<Shooter1>();
        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    public void AddCoin(int amount = 1)
    {
        Coins += amount;
        LevelCoinsCollected = Mathf.Min(LevelCoinsRequired, LevelCoinsCollected + 1);
        if (UIManager.Instance != null) UIManager.Instance.RefreshHUD();
    }

    public void NotifyJump()
    {
        JumpsUsed++;
    }

    public void LoseLife()
    {
        if (IsFinished) return;
        Lives--;
        if (UIManager.Instance != null) UIManager.Instance.RefreshHUD();
        if (Lives <= 0) GameOver();
        else RespawnAtCheckpoint();
    }

    public void RegisterCheckpoint(Vector3 position)
    {
        checkpointPosition = position;
        checkpointScene = SceneManager.GetActiveScene().name;
        hasCheckpoint = true;
    }

    private void RespawnAtCheckpoint()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null || !hasCheckpoint || checkpointScene != SceneManager.GetActiveScene().name)
        {
            RestartCurrentLevel();
            return;
        }
        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) { rb.linearVelocity = Vector2.zero; rb.angularVelocity = 0f; }
        player.transform.position = checkpointPosition;
        var health = player.GetComponent<Assignment6.Health>();
        if (health != null) health.RestoreAfterRespawn();
    }

    public void WinLevel() => WinLevel("LevelSelect");

    public void WinLevel(string destinationScene)
    {
        if (IsFinished) return;
        IsFinished = true;
        Assignment6.A6Audio.PlayWin();
        var player = GameObject.FindGameObjectWithTag("Player");
        var animator = player != null ? player.GetComponent<Animator>() : null;
        if (animator != null) animator.SetTrigger("Victory");
        Time.timeScale = 0f;
        if (UIManager.Instance != null) UIManager.Instance.ShowWin();
        StartCoroutine(ReturnAfterVictory(destinationScene));
    }

    IEnumerator ReturnAfterVictory(string destinationScene)
    {
        yield return new WaitForSecondsRealtime(2.3f);
        Time.timeScale = 1f;
        IsPaused = false;
        IsFinished = false;
        SceneManager.LoadScene(destinationScene);
    }

    public void GameOver()
    {
        if (IsFinished) return;
        IsFinished = true;
        var player = GameObject.FindGameObjectWithTag("Player");
        var animator = player != null ? player.GetComponent<Animator>() : null;
        if (animator != null) animator.SetTrigger("Dead");
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
        Lives = 3;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        IsPaused = false;
        IsFinished = false;
        SceneManager.LoadScene(sceneName);
    }

    public void LoadNextLevel()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        IsFinished = false;
        string current = SceneManager.GetActiveScene().name;
        string next = current == "Level1" ? "Level2" : current == "Level2" ? "Level3" : "";
        if (string.IsNullOrEmpty(next)) ReturnToMenu();
        else SceneManager.LoadScene(next);
    }

    public void ReturnToMenu() => LoadScene("MainMenu");
}
