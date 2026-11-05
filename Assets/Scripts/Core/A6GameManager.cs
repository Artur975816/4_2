using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
namespace Assignment6 {
public class A6GameManager:MonoBehaviour {
 public static A6GameManager Instance{get;private set;}
 public float secondsToWin=60f; public GameObject gate,winPanel,gameOverPanel; public TMP_Text timerText,hpText; public Spawner spawner; float elapsed; bool finished;
 void Awake(){if(Instance&&Instance!=this){Destroy(gameObject);return;}Instance=this;Time.timeScale=1f;}
 void Start(){if(winPanel)winPanel.SetActive(false);if(gameOverPanel)gameOverPanel.SetActive(false);if(gate)gate.SetActive(false);RefreshHUD();}
 void Update(){if(finished)return;elapsed+=Time.deltaTime;float left=Mathf.Max(0,secondsToWin-elapsed);if(timerText)timerText.text="Время: "+left.ToString("0");if(left<=0)Win();}
 public void RefreshHUD(){var h=FindFirstObjectByType<Health>();if(h&&hpText)hpText.text="HP: "+h.CurrentHP+"/"+h.maxHP;}
 public void PlayerDied(){if(finished)return;finished=true;if(spawner)spawner.StopSpawning();if(gameOverPanel)gameOverPanel.SetActive(true);else Invoke(nameof(RestartScene),1.5f);}
 void Win(){finished=true;if(spawner)spawner.StopSpawning();if(gate)gate.SetActive(true);if(winPanel)winPanel.SetActive(true);A6Audio.PlayWin();}
 public void RestartScene(){Time.timeScale=1f;SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);}
}}
