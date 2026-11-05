using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
public sealed class BossController : MonoBehaviour
{
    [Min(1)] public int maxHealth = 12;
    [Min(.2f)] public float attackInterval = 2f;
    [Min(1f)] public float projectileSpeed = 5f;
    [Min(0.1f)] public float jumpHeight = 2.5f;
    [Min(.5f)] public float jumpInterval = 4.5f;
    public float arenaLeft = -8f, arenaRight = 8f;
    public Slider healthBar;
    public Transform target;
    int currentHealth;
    float nextAttack;
    float nextJump;
    float baseY;
    float jumpVelocity;
    bool jumping;
    bool dead;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    void Awake()
    {
        currentHealth = Mathf.Max(1, maxHealth);
        var collider = GetComponent<Collider2D>();
        collider.isTrigger = true;
    }

    void Start()
    {
        baseY = transform.position.y;
        if (target == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
        }
        UpdateBar();
        nextAttack = Time.time + 1f;
        nextJump = Time.time + 2.5f;
    }

    void Update()
    {
        if (dead) return;
        var p = transform.position;
        p.x = Mathf.Clamp(p.x + Mathf.Sin(Time.time * .8f) * Time.deltaTime * .55f, arenaLeft + 1f, arenaRight - 1f);
        if (!jumping && Time.time >= nextJump)
        {
            jumping = true;
            jumpVelocity = Mathf.Sqrt(2f * Mathf.Abs(Physics2D.gravity.y) * jumpHeight);
        }
        if (jumping)
        {
            jumpVelocity += Physics2D.gravity.y * Time.deltaTime;
            p.y += jumpVelocity * Time.deltaTime;
            if (p.y <= baseY && jumpVelocity < 0f)
            {
                p.y = baseY;
                jumping = false;
                jumpVelocity = 0f;
                nextJump = Time.time + jumpInterval;
                transform.position = p;
                ReleaseShockwaves(p.x);
            }
        }
        transform.position = p;
        if (target != null && Time.time >= nextAttack)
        {
            nextAttack = Time.time + attackInterval;
            FireAtPlayer();
        }
    }

    public void SetHealthBar(Slider slider) { healthBar = slider; UpdateBar(); }

    public void TakeDamage(int amount)
    {
        if (dead || amount <= 0) return;
        currentHealth = Mathf.Max(0, currentHealth - amount);
        UpdateBar();
        var renderer = GetComponent<SpriteRenderer>();
        if (renderer != null) StartCoroutine(HitFlash(renderer));
        if (currentHealth == 0) Die();
    }

    System.Collections.IEnumerator HitFlash(SpriteRenderer renderer)
    {
        Color old = renderer.color;
        renderer.color = Color.white;
        yield return new WaitForSeconds(.08f);
        if (renderer != null) renderer.color = old;
    }

    void UpdateBar()
    {
        if (healthBar == null) return;
        healthBar.maxValue = Mathf.Max(1, maxHealth);
        healthBar.SetValueWithoutNotify(currentHealth);
        if (healthBar.fillRect != null)
        {
            var fill = healthBar.fillRect.GetComponent<Image>();
            if (fill != null) fill.fillAmount = currentHealth / (float)Mathf.Max(1, maxHealth);
        }
    }

    void ReleaseShockwaves(float x)
    {
        SpawnShockwave(x, -1f);
        SpawnShockwave(x, 1f);
    }

    void SpawnShockwave(float x, float direction)
    {
        float boundary = direction < 0f ? arenaLeft : arenaRight;
        float distance = Mathf.Max(0.5f, direction < 0f ? x - boundary : boundary - x);
        var wave = new GameObject("Warden landing shockwave");
        wave.transform.position = new Vector3(x, baseY - .65f, 0f);
        wave.transform.localScale = new Vector3(1.4f, .28f, 1f);
        var source = GetComponent<SpriteRenderer>();
        var sr = wave.AddComponent<SpriteRenderer>();
        if (source != null) sr.sprite = source.sprite;
        sr.color = new Color(1f, .3f, .48f, .95f);
        sr.sortingOrder = source != null ? source.sortingOrder + 1 : 5;
        var collider = wave.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(1f, 1.3f);
        var body = wave.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        wave.AddComponent<BossShockwave>().Initialize(new Vector2(direction, 0f), projectileSpeed * 1.3f, distance);
    }

    void FireAtPlayer()
    {
        var go = new GameObject("Warden orb");
        go.transform.position = transform.position + Vector3.left * .6f;
        go.transform.localScale = Vector3.one * .38f;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = GetComponent<SpriteRenderer>() != null ? GetComponent<SpriteRenderer>().sprite : null;
        sr.color = new Color(.96f, .35f, .45f);
        sr.sortingOrder = 4;
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        var projectile = go.AddComponent<BossProjectile>();
        projectile.Initialize((target.position - go.transform.position).normalized, projectileSpeed);
    }

    void Die()
    {
        if (dead) return;
        dead = true;
        foreach (var c in GetComponents<Collider2D>()) c.enabled = false;
        foreach (var projectile in FindObjectsByType<BossProjectile>(FindObjectsSortMode.None)) Destroy(projectile.gameObject);
        if (GameManager.Instance != null) GameManager.Instance.WinLevel();
        Destroy(gameObject, 1.2f);
    }
}
