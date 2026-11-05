using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Exit : MonoBehaviour
{
    private bool activated;

    private void Start() => GetComponent<Collider2D>().isTrigger = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated || !other.CompareTag("Player") || GameManager.Instance == null) return;
        if (!GameManager.Instance.HasCollectedAllLevelCoins)
        {
            if (UIManager.Instance != null) UIManager.Instance.ShowMessage("\u0421\u043E\u0431\u0435\u0440\u0438\u0442\u0435 \u0432\u0441\u0435 \u043C\u043E\u043D\u0435\u0442\u044B, \u0447\u0442\u043E\u0431\u044B \u043E\u0442\u043A\u0440\u044B\u0442\u044C \u043F\u043E\u0440\u0442\u0430\u043B!");
            return;
        }
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        int nextLevel = scene == "Level1" ? 2 : scene == "Level2" ? 3 : 0;
        if (nextLevel > 0) SaveSystem.UnlockLevel(nextLevel);
        activated = true;
        GameManager.Instance.WinLevel(nextLevel > 0 ? "Level" + nextLevel : "LevelSelect");
    }
}
