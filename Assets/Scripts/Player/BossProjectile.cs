using UnityEngine;

public sealed class BossProjectile : MonoBehaviour
{
    Vector2 direction;
    float speed;
    bool hit;
    Rigidbody2D body;

    void Awake() => body = GetComponent<Rigidbody2D>();

    public void Initialize(Vector2 travelDirection, float projectileSpeed)
    {
        direction = travelDirection.normalized;
        speed = projectileSpeed;
        Destroy(gameObject, 5f);
    }

    void FixedUpdate()
    {
        if (!hit && body != null) body.MovePosition(body.position + direction * speed * Time.fixedDeltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hit || !other.CompareTag("Player")) return;
        hit = true;
        var health = other.GetComponent<Assignment6.Health>();
        if (health == null) health = other.GetComponentInParent<Assignment6.Health>();
        if (health != null) health.TakeDamage(1);
        else if (GameManager.Instance != null) GameManager.Instance.LoseLife();
        Destroy(gameObject);
    }
}
