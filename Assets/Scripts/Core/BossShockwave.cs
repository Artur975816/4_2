using UnityEngine;

[RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
public sealed class BossShockwave : MonoBehaviour
{
    Rigidbody2D body;
    Vector2 direction;
    float speed;
    float remainingDistance;
    bool hit;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        GetComponent<Collider2D>().isTrigger = true;
    }

    public void Initialize(Vector2 travelDirection, float travelSpeed, float maxDistance)
    {
        direction = new Vector2(Mathf.Sign(travelDirection.x), 0f);
        speed = Mathf.Max(.1f, travelSpeed);
        remainingDistance = Mathf.Max(.1f, maxDistance);
    }

    void FixedUpdate()
    {
        if (hit) return;
        float step = speed * Time.fixedDeltaTime;
        body.MovePosition(body.position + direction * step);
        remainingDistance -= step;
        if (remainingDistance <= 0f) Destroy(gameObject);
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
