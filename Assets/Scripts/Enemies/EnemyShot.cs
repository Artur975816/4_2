using UnityEngine;

/// Slow projectile fired by ShooterEnemy. Created at runtime, so no prefab is needed.
public sealed class EnemyShot : MonoBehaviour
{
    Vector2 dir; float speed; int damage; LayerMask wallMask; bool spent;

    public void Init(Vector2 direction, float shotSpeed, int hitDamage, LayerMask walls)
    {
        dir = direction.normalized; speed = shotSpeed; damage = hitDamage; wallMask = walls;
        Destroy(gameObject, 5f);
    }

    void Update() => transform.position += (Vector3)(dir * speed * Time.deltaTime);

    void OnTriggerEnter2D(Collider2D other)
    {
        if (spent) return;
        if (other.CompareTag("Player"))
        {
            spent = true;
            var h = other.GetComponentInParent<Assignment6.Health>();
            if (h != null) h.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (((1 << other.gameObject.layer) & wallMask.value) != 0)
        {
            spent = true;
            Destroy(gameObject);
        }
    }
}
