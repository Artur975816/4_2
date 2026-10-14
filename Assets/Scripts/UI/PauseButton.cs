using UnityEngine;

public class PauseButton : MonoBehaviour
{
    public void TogglePause()
    {
        if (GameManager.Instance != null) GameManager.Instance.TogglePause();
    }
}
