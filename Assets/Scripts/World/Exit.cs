using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Exit : MonoBehaviour
{
    private void Start() => GetComponent<Collider2D>().isTrigger = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && GameManager.Instance != null)
        {
            SaveSystem.UnlockLevel(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 2);
            GameManager.Instance.WinLevel();
        }
    }
}
