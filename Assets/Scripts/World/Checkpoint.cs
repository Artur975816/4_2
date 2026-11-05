using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public sealed class Checkpoint : MonoBehaviour
{
    bool activated;

    void Awake() => GetComponent<Collider2D>().isTrigger = true;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (activated || !other.CompareTag("Player") || GameManager.Instance == null) return;
        activated = true;
        GameManager.Instance.RegisterCheckpoint(other.transform.position);
        var renderer = GetComponent<SpriteRenderer>();
        if (renderer != null) renderer.color = new Color(.42f, .95f, .67f);
    }
}
