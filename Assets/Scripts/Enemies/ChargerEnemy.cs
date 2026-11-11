using UnityEngine;
using TMPro;

/// CHARGER: Idle -> WindUp -> Dash -> Stunned
public class ChargerEnemy : MonoBehaviour
{
    enum State { Idle, WindUp, Dash, Stunned }
    State state; float timer; float facing = -1f; float dashDir, traveled; bool hitDone;
    Vector2 half, playerHalf; LineRenderer lane; Color baseColor;

    [Header("Refs")]
    [SerializeField] Transform player;
    [SerializeField] LayerMask obstacleMask;      // tick only Walls (ground must be on it too, for the ledge check)
    [SerializeField] SpriteRenderer sprite;
    [SerializeField] TMP_Text stateLabel;
    [SerializeField] Vector2 eyeOffset = new Vector2(0f, 0.3f);
    [SerializeField] float startFacing = -1f;

    [Header("Tuning")]
    [SerializeField] float triggerRange = 6f;
    [SerializeField] float fov = 180f;
    [SerializeField] float windUp = 0.7f;         // red flashing + lane on the floor
    [SerializeField] float dashSpeed = 9f;
    [SerializeField] float dashDistance = 7f;
    [SerializeField] float stunTime = 1f;         // player's turn: shoot it
    [SerializeField] int damage = 1;
    [SerializeField] bool stopAtLedge = true;
    [SerializeField] Color windUpColor = Color.red;
    [SerializeField] Color stunColor = new Color(0.55f, 0.65f, 1f);

    void Start()
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player").transform;
        if (sprite == null) sprite = GetComponentInChildren<SpriteRenderer>();
        if (sprite != null) baseColor = sprite.color;
        var col = GetComponent<Collider2D>();
        half = col != null ? (Vector2)col.bounds.extents : new Vector2(0.5f, 0.5f);
        var pc = player.GetComponentInChildren<Collider2D>();
        playerHalf = pc != null ? (Vector2)pc.bounds.extents : new Vector2(0.36f, 0.57f);
        facing = startFacing; ApplyFlip();
        lane = Sight2D.MakeLine(gameObject, 0.12f, 1);
        lane.startColor = lane.endColor = new Color(1f, 0.15f, 0.15f, 0.55f);
        SetState(State.Idle);
    }

    Vector2 Eye() => (Vector2)transform.position + eyeOffset;
    bool Sees() => Sight2D.CanSee(Eye(), Vector2.right * facing, player, triggerRange, fov, obstacleMask);
    void ApplyFlip() { if (sprite != null) sprite.flipX = facing < 0f; }

    void Update()
    {
        timer += Time.deltaTime;
        switch (state)
        {
            case State.Idle:
                if (Sees() && Sight2D.OnScreen(transform.position)) SetState(State.WindUp);
                break;

            case State.WindUp:                                       // direction already locked: leave the lane or jump
                DrawLane();
                if (sprite != null) sprite.color = Mathf.PingPong(Time.time * 10f, 1f) > 0.5f ? windUpColor : baseColor;
                if (timer >= windUp) SetState(State.Dash);
                break;

            case State.Dash:
                DashStep();
                break;

            case State.Stunned:
                if (timer >= stunTime) SetState(State.Idle);
                break;
        }
    }

    void DashStep()
    {
        float step = dashSpeed * Time.deltaTime;
        Vector2 pos = transform.position, dir = Vector2.right * dashDir;

        if (Physics2D.Raycast(pos, dir, half.x + step, obstacleMask)) { SetState(State.Stunned); return; }   // wall
        if (stopAtLedge)
        {
            Vector2 front = pos + dir * (half.x + step);
            if (!Physics2D.Raycast(front, Vector2.down, half.y + 0.4f, obstacleMask)) { SetState(State.Stunned); return; } // edge
        }

        transform.position += (Vector3)(dir * step);
        traveled += step;

        if (!hitDone && Overlaps())
        {
            hitDone = true;                                          // one hit per dash
            player.GetComponentInParent<Assignment6.Health>()?.TakeDamage(damage);
        }
        if (traveled >= dashDistance) SetState(State.Stunned);
    }

    bool Overlaps()
    {
        Vector2 d = player.position - transform.position;
        return Mathf.Abs(d.x) < half.x + playerHalf.x && Mathf.Abs(d.y) < half.y + playerHalf.y;
    }

    void DrawLane()
    {
        Vector2 a = transform.position, dir = Vector2.right * dashDir;
        float len = dashDistance;
        var hit = Physics2D.Raycast(a, dir, dashDistance, obstacleMask);
        if (hit.collider != null) len = hit.distance;
        lane.SetPosition(0, a + Vector2.down * (half.y - 0.1f));
        lane.SetPosition(1, a + dir * len + Vector2.down * (half.y - 0.1f));
    }

    void SetState(State next)
    {
        state = next; timer = 0f;
        lane.enabled = next == State.WindUp;
        if (sprite != null)
            sprite.color = next == State.Dash ? windUpColor : next == State.Stunned ? stunColor : baseColor;

        if (next == State.WindUp)
        {
            float dx = player.position.x - transform.position.x;
            dashDir = Mathf.Abs(dx) > 0.01f ? Mathf.Sign(dx) : facing;   // locked here
            facing = dashDir; ApplyFlip();
            DrawLane();
        }
        if (next == State.Dash) { traveled = 0f; hitDone = false; }
        if (stateLabel != null) stateLabel.text = next.ToString();
    }

    void OnDrawGizmosSelected()
    {
        float f = Application.isPlaying ? facing : startFacing;
        Sight2D.DrawCone(transform.position, Vector2.right * f, triggerRange, fov);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * f * dashDistance);
    }
}
