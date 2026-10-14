using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Coin : MonoBehaviour
{
    public int value = 1;
    public float bobHeight = 0.12f;
    public float bobSpeed = 3f;
    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Update()
    {
        transform.position = startPos + Vector3.up * (Mathf.Sin(Time.time * bobSpeed) * bobHeight);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (GameManager.Instance != null) GameManager.Instance.AddCoin(value);
        SaveSystem.AddTotalCoins(value);
        if (UIManager.Instance != null) UIManager.Instance.RefreshHUD();
        Destroy(gameObject);
    }
}
