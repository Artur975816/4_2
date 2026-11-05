using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public sealed class PitTrigger : MonoBehaviour
{
    bool triggered;

    void Awake() => GetComponent<Collider2D>().isTrigger = true;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered || !other.CompareTag("Player")) return;
        triggered = true;
        if (GameManager.Instance != null) GameManager.Instance.LoseLife();
        else
        {
            var health = other.GetComponent<Assignment6.Health>();
            if (health != null) health.TakeDamage(int.MaxValue);
        }
        triggered = false;
    }
}
