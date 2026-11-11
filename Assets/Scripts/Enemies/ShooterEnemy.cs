using UnityEngine;
using TMPro;

/// SHOOTER: Idle -> KeepDistance -> Aim (laser) -> Cooldown
public class ShooterEnemy : MonoBehaviour
{
    enum State { Idle, KeepDistance, Aim, Cooldown }
    State state; float timer; float facing = -1f;
    Vector2 aimDir; float homeX; LineRenderer laser; Color baseColor;

    [Header("Refs")]
    [SerializeField] Transform player;
    [SerializeField] LayerMask obstacleMask;      // tick only Walls
    [SerializeField] SpriteRenderer sprite;
    [SerializeField] TMP_Text stateLabel;
    [SerializeField] Transform muzzle;            // optional, empty child at the "gun"
    [SerializeField] Vector2 eyeOffset = new Vector2(0f, 0.3f);
    [SerializeField] float startFacing = -1f;     // -1 = looks left, 1 = looks right

    [Header("Sight")]
    [SerializeField] float viewRange = 9f;
    [SerializeField] float fov = 120f;
    [SerializeField] float loseTime = 2f;

    [Header("Movement")]
    [SerializeField] float keepDistance = 5f;
    [SerializeField] float distanceTolerance = 1f;
    [SerializeField] float moveSpeed = 1.8f;
    [SerializeField] float leash = 3f;            // max distance from the start point (so it never leaves its platform)

    [Header("Attack")]
    [SerializeField] float shootRange = 7f;
    [SerializeField] float aimTime = 0.6f;        // laser visible this long before the shot
    [SerializeField] float shotSpeed = 5f;
    [SerializeField] float cooldown = 1.5f;
    [SerializeField] float laserLength = 12f;
    [SerializeField] int damage = 1;
    [SerializeField] Color aimColor = Color.red;

    void Start()
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player").transform;
        if (sprite == null) sprite = GetComponentInChildren<SpriteRenderer>();
        if (sprite != null) baseColor = sprite.color;
        facing = startFacing; homeX = transform.position.x;
        laser = Sight2D.MakeLine(gameObject, 0.06f);
        laser.startColor = laser.endColor = new Color(aimColor.r, aimColor.g, aimColor.b, 0.8f);
        ApplyFlip();
        SetState(State.Idle);
    }

    Vector2 Eye() => (Vector2)transform.position + eyeOffset;
    Vector2 Muzzle() => muzzle != null ? (Vector2)muzzle.position : (Vector2)transform.position + new Vector2(facing * 0.5f, 0.2f);
    bool Sees() => Sight2D.CanSee(Eye(), Vector2.right * facing, player, viewRange, fov, obstacleMask);
    float HDist() => Mathf.Abs(player.position.x - transform.position.x);

    void Update()
    {
        timer += Time.deltaTime;
        switch (state)
        {
            case State.Idle:
                if (Sees()) SetState(State.KeepDistance);
                break;

            case State.KeepDistance:
                FacePlayer();
                if (Sees()) timer = 0f;                              // timer = time since last seen
                Reposition();
                if (Sees() && HDist() <= shootRange && Sight2D.OnScreen(transform.position)) SetState(State.Aim);
                else if (timer > loseTime) SetState(State.Idle);
                break;

            case State.Aim:                                          // direction is locked: the player can step aside
                UpdateLaser();
                if (sprite != null) sprite.color = Mathf.PingPong(Time.time * 8f, 1f) > 0.5f ? aimColor : baseColor;
                if (timer >= aimTime) { Fire(); SetState(State.Cooldown); }
                break;

            case State.Cooldown:                                     // counter window
                if (timer >= cooldown) SetState(State.KeepDistance);
                break;
        }
    }

    void FacePlayer()
    {
        float dx = player.position.x - transform.position.x;
        if (Mathf.Abs(dx) > 0.05f) { facing = Mathf.Sign(dx); ApplyFlip(); }
    }

    void ApplyFlip() { if (sprite != null) sprite.flipX = facing < 0f; }

    void Reposition()
    {
        float dx = player.position.x - transform.position.x, dist = Mathf.Abs(dx), dir = 0f;
        if (dist > keepDistance + distanceTolerance) dir = Mathf.Sign(dx);
        else if (dist < keepDistance - distanceTolerance) dir = -Mathf.Sign(dx);
        float x = Mathf.Clamp(transform.position.x + dir * moveSpeed * Time.deltaTime, homeX - leash, homeX + leash);
        transform.position = new Vector3(x, transform.position.y, transform.position.z);
    }

    void UpdateLaser()
    {
        Vector2 from = Muzzle();
        float len = laserLength;
        var hit = Physics2D.Raycast(from, aimDir, laserLength, obstacleMask);
        if (hit.collider != null) len = hit.distance;
        laser.SetPosition(0, from);
        laser.SetPosition(1, from + aimDir * len);
    }

    void Fire()
    {
        var go = new GameObject("Shooter shot");
        go.transform.position = Muzzle();
        go.transform.localScale = Vector3.one * 0.35f;
        var sr = go.AddComponent<SpriteRenderer>();
        if (sprite != null) sr.sprite = sprite.sprite;
        sr.color = aimColor; sr.sortingOrder = 4;
        go.AddComponent<CircleCollider2D>().isTrigger = true;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic; rb.gravityScale = 0f;
        go.AddComponent<EnemyShot>().Init(aimDir, shotSpeed, damage, obstacleMask);
    }

    void SetState(State next)
    {
        state = next; timer = 0f;
        if (sprite != null) sprite.color = baseColor;
        laser.enabled = next == State.Aim;
        if (next == State.Aim)
        {
            aimDir = ((Vector2)player.position - Muzzle()).normalized;   // locked at the start of the aim
            UpdateLaser();
        }
        if (stateLabel != null) stateLabel.text = next.ToString();
    }

    void OnDrawGizmosSelected()
    {
        float f = Application.isPlaying ? facing : startFacing;
        Sight2D.DrawCone(transform.position, Vector2.right * f, viewRange, fov);
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, shootRange);
    }
}
