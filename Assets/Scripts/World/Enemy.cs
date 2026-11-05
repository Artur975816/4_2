using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Min(1)] public int maxHealth = 2;
    int health;
    bool defeated;

    void Awake() => health = Mathf.Max(1, maxHealth);

    public void TakeDamage(int amount)
    {
        if (defeated || amount <= 0) return;
        health = Mathf.Max(0, health - amount);
        if (health == 0) { defeated = true; Destroy(gameObject); }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Player")) return;
        var health = collision.collider.GetComponent<Assignment6.Health>();
        if (health == null) health = collision.collider.GetComponentInParent<Assignment6.Health>();
        if (health != null) health.TakeDamage();
        else if (GameManager.Instance != null) GameManager.Instance.LoseLife();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        var health = other.GetComponent<Assignment6.Health>();
        if (health == null) health = other.GetComponentInParent<Assignment6.Health>();
        if (health != null) health.TakeDamage();
        else if (GameManager.Instance != null) GameManager.Instance.LoseLife();
    }
}
