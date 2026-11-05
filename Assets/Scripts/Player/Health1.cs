using System.Collections;
using UnityEngine;
namespace Assignment6 {
public class Health:MonoBehaviour {
 public int maxHP=3; public float invulnerabilityDuration=2f,blinkInterval=0.2f;
 public int CurrentHP{get;private set;} public bool IsInvincible{get;private set;}
 SpriteRenderer sr; bool dead;
 void Awake(){CurrentHP=maxHP;sr=GetComponentInChildren<SpriteRenderer>();}
 public void TakeDamage(int amount=1){if(IsInvincible||dead||amount<=0)return;CurrentHP=Mathf.Max(0,CurrentHP-amount);A6GameManager.Instance?.RefreshHUD();A6Audio.PlayHurt();var animator=GetComponent<Animator>();if(animator!=null)animator.SetTrigger("Hurt");if(CurrentHP<=0){dead=true;if(GameManager.Instance!=null)GameManager.Instance.LoseLife();else A6GameManager.Instance?.PlayerDied();}else StartCoroutine(Blink());}
 public void Heal(int amount=1){if(dead||amount<=0)return;CurrentHP=Mathf.Min(maxHP,CurrentHP+amount);A6GameManager.Instance?.RefreshHUD();A6Audio.PlayHeal();}
 public void RestoreAfterRespawn(){StopAllCoroutines();dead=false;IsInvincible=true;CurrentHP=Mathf.Max(1,maxHP);if(sr)sr.enabled=true;StartCoroutine(Blink());A6GameManager.Instance?.RefreshHUD();}
 IEnumerator Blink(){IsInvincible=true;float t=0;while(t<invulnerabilityDuration){if(sr)sr.enabled=!sr.enabled;yield return new WaitForSeconds(blinkInterval);t+=blinkInterval;}if(sr)sr.enabled=true;IsInvincible=false;}
}}
