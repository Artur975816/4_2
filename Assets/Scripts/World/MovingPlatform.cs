using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public sealed class MovingPlatform : MonoBehaviour
{
    [Tooltip("Endpoint in world space. The platform starts at its scene position.")]
    public Vector2 endpointOffset = new Vector2(4f, 0f);
    [Min(0.05f)] public float speed = 1.5f;
    [Min(0f)] public float endpointPause = 0.45f;

    Rigidbody2D body;
    Vector2 start, end, previous;
    Vector2 platformVelocity;
    float wait;
    bool returning;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        start = body.position;
        end = start + endpointOffset;
        previous = body.position;
    }

    void FixedUpdate()
    {
        Vector2 target = returning ? start : end;
        if (wait > 0f) wait -= Time.fixedDeltaTime;
        else
        {
            Vector2 next = Vector2.MoveTowards(body.position, target, speed * Time.fixedDeltaTime);
            body.MovePosition(next);
            if ((next - target).sqrMagnitude < 0.0001f)
            {
                returning = !returning;
                wait = endpointPause;
            }
        }
        platformVelocity = (body.position - previous) / Time.fixedDeltaTime;
        previous = body.position;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        var movement = collision.collider.GetComponentInParent<PlayerMovement>();
        if (movement == null) return;
        var playerCollider = collision.collider;
        var platformCollider = GetComponent<Collider2D>();
        if (playerCollider.bounds.min.y < platformCollider.bounds.max.y - 0.12f) return;
        var playerBody = movement.GetComponent<Rigidbody2D>();
        if (playerBody != null && playerBody.linearVelocity.y > platformVelocity.y + 0.25f) return;
        movement.SetMovingPlatformVelocity(platformVelocity);
    }
}
