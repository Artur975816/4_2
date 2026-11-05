using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public sealed class HazardTrigger : MonoBehaviour
{
    [Min(1)] public int damage = 1;

    void Awake() => GetComponent<Collider2D>().isTrigger = true;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        var health = other.GetComponent<Assignment6.Health>();
        if (health == null) health = other.GetComponentInParent<Assignment6.Health>();
        if (health != null) health.TakeDamage(damage);
        else if (GameManager.Instance != null) GameManager.Instance.LoseLife();
    }
}
