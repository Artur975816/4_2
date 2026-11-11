using UnityEngine;
using TMPro;

/// PATROLLER: Patrol -> Chase -> Attack (wind-up) -> Recover
public class PatrollerEnemy : MonoBehaviour
{
    enum State { Patrol, Chase, Attack, Recover }
    State state; float timer, nextAttackTime; int wp; float facing = 1f;
    float minX, maxX; Color baseColor;

    [Header("Refs")]
    [SerializeField] Transform[] points;          // waypoints: NOT children of this enemy
    [SerializeField] Transform player;            // empty = found by tag "Player"
    [SerializeField] LayerMask obstacleMask;      // tick only Walls
    [SerializeField] SpriteRenderer sprite;
    [SerializeField] TMP_Text stateLabel;         // optional, 3D TextMeshPro child
    [SerializeField] Vector2 eyeOffset = new Vector2(0f, 0.3f);

    [Header("Tuning")]
    [SerializeField] float patrolSpeed = 1.5f;
    [SerializeField] float chaseSpeed = 3f;       // player runs at 5.5, so it can be outrun
    [SerializeField] float viewRange = 6f;
    [SerializeField] float fov = 90f;
    [SerializeField] float loseTime = 2f;
    [SerializeField] float chaseExtra = 2f;       // how far past the route it may chase (stops it walking off ledges)
    [SerializeField] float attackRange = 1.2f;
    [SerializeField] float windUp = 0.5f;
    [SerializeField] float recovery = 0.6f;
    [SerializeField] float cooldown = 1.5f;
    [SerializeField] int damage = 1;

    void Start()
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player").transform; // cached once
        if (sprite == null) sprite = GetComponentInChildren<SpriteRenderer>();
        if (sprite != null) baseColor = sprite.color;
        minX = maxX = transform.position.x;
        foreach (var p in points) { minX = Mathf.Min(minX, p.position.x); maxX = Mathf.Max(maxX, p.position.x); }
        SetState(State.Patrol);
    }

    Vector2 Eye() => (Vector2)transform.position + eyeOffset;
    bool Sees() => Sight2D.CanSee(Eye(), Vector2.right * facing, player, viewRange, fov, obstacleMask);
    float Dist() => Vector2.Distance(transform.position, player.position);

    void Update()
    {
        timer += Time.deltaTime;
        switch (state)
        {
            case State.Patrol:
                MoveTo(points[wp].position.x, patrolSpeed, false);
                if (Mathf.Abs(transform.position.x - points[wp].position.x) < 0.05f) wp = (wp + 1) % points.Length;
                if (Sees()) SetState(State.Chase);
                break;

            case State.Chase:
                if (Sees()) timer = 0f;                              // timer = time since last seen
                MoveTo(player.position.x, chaseSpeed, true);
                if (Dist() < attackRange && Time.time >= nextAttackTime && Sight2D.OnScreen(transform.position))
                    SetState(State.Attack);
                else if (timer > loseTime) SetState(State.Patrol);
                break;

            case State.Attack:                                       // stands still and turns red = warning
                if (timer >= windUp)
                {
                    if (Dist() < attackRange * 1.2f)                 // player stepped away -> miss
                        player.GetComponentInParent<Assignment6.Health>()?.TakeDamage(damage);
                    nextAttackTime = Time.time + cooldown;
                    SetState(State.Recover);
                }
                break;

            case State.Recover:                                      // counter window: player hits back
                if (timer >= recovery) SetState(State.Chase);
                break;
        }
    }

    void MoveTo(float targetX, float speed, bool clamp)
    {
        Vector3 pos = transform.position;
        float dx = targetX - pos.x;
        if (Mathf.Abs(dx) > 0.01f) facing = Mathf.Sign(dx);
        float x = Mathf.MoveTowards(pos.x, targetX, speed * Time.deltaTime);
        if (clamp) x = Mathf.Clamp(x, minX - chaseExtra, maxX + chaseExtra);
        transform.position = new Vector3(x, pos.y, pos.z);
        if (sprite != null) sprite.flipX = facing < 0f;
    }

    void SetState(State next)
    {
        state = next; timer = 0f;
        if (sprite != null) sprite.color = next == State.Attack ? Color.red : baseColor;
        if (stateLabel != null) stateLabel.text = next.ToString();
    }

    void OnDrawGizmosSelected()
    {
        float f = Application.isPlaying ? facing : 1f;
        Sight2D.DrawCone(transform.position, Vector2.right * f, viewRange, fov);
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
