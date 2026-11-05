using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelector : MonoBehaviour
{
    public int levelNumber = 1;
    public Button levelButton;

    private void Start()
    {
        if (levelButton == null) levelButton = GetComponent<Button>();
        bool unlocked = levelNumber <= SaveSystem.UnlockedLevel;
        if (levelButton != null) levelButton.interactable = unlocked;
    }

    public void LoadLevel()
    {
        if (levelNumber > SaveSystem.UnlockedLevel) return;
        SceneManager.LoadScene("Level" + levelNumber);
    }
}
