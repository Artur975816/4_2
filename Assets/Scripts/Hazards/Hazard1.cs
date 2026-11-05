using UnityEngine;
namespace Assignment6 {
public class Hazard:MonoBehaviour {
 public int damage=1;
 protected virtual void OnHitPlayer(Health h){h.TakeDamage(damage);}
 protected virtual void OnCollisionEnter2D(Collision2D c){if(!c.gameObject.CompareTag("Player"))return;var h=c.gameObject.GetComponent<Health>();if(!h)h=c.gameObject.GetComponentInParent<Health>();if(h)OnHitPlayer(h);if(!(this is Heart))Destroy(gameObject);}
}}
