using UnityEngine;
namespace Assignment6 {
public class BombHazard:Hazard {
 public float fuseSeconds=2f,explosionRadius=2f; public int explosionDamage=2; public GameObject explosionEffect; bool exploded;
 void Start(){Invoke(nameof(Explode),fuseSeconds);}
 public void Explode(){if(exploded)return;exploded=true;A6Audio.PlayExplosion();if(explosionEffect)Instantiate(explosionEffect,transform.position,Quaternion.identity);foreach(var c in Physics2D.OverlapCircleAll(transform.position,explosionRadius)){if(!c.CompareTag("Player"))continue;var h=c.GetComponent<Health>();if(!h)h=c.GetComponentInParent<Health>();if(h)h.TakeDamage(explosionDamage);}Destroy(gameObject);}
 protected override void OnHitPlayer(Health h){h.TakeDamage(explosionDamage);Explode();}
 void OnDrawGizmosSelected(){Gizmos.color=Color.red;Gizmos.DrawWireSphere(transform.position,explosionRadius);}
}}
