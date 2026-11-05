using UnityEngine;

public sealed class Bullet : MonoBehaviour
{
    Vector2 direction;
    float speed;
    int damage;
    float lifetime = 3f;
    bool spent;
    Rigidbody2D body;

    void Awake() => body = GetComponent<Rigidbody2D>();

    public void Initialize(Vector2 travelDirection, float projectileSpeed, int hitDamage)
    {
        direction = travelDirection.normalized;
        speed = projectileSpeed;
        damage = Mathf.Max(1, hitDamage);
        Destroy(gameObject, lifetime);
    }

    void FixedUpdate()
    {
        if (spent) return;
        if (body != null) body.MovePosition(body.position + direction * (speed * Time.fixedDeltaTime));
        else transform.position += (Vector3)(direction * (speed * Time.fixedDeltaTime));
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (spent || other.CompareTag("Player")) return;
        var boss = other.GetComponentInParent<BossController>();
        if (boss != null) { spent = true; boss.TakeDamage(damage); Destroy(gameObject); return; }
        var enemy = other.GetComponentInParent<Enemy>();
        if (enemy != null) { spent = true; enemy.TakeDamage(damage); Destroy(gameObject); return; }
        if (!other.isTrigger) { spent = true; Destroy(gameObject); }
    }
}
