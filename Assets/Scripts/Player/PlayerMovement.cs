using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public sealed class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [Min(0f)] public float speed = 5.5f;
    [Min(0.05f)] public float jumpHeight = 2.5f;
    [Min(0f)] public float acceleration = 55f;
    [Range(0f, 0.2f)] public float coyoteTime = 0.08f;
    [Range(0f, 0.2f)] public float jumpBuffer = 0.1f;
    [Range(0.1f, 1f)] public float jumpCutMultiplier = 0.5f;
    [Min(1f)] public float fallGravityMultiplier = 1.7f;
    public Transform groundCheck;
    [Min(0.01f)] public float groundCheckRadius = 0.12f;
    public LayerMask groundLayer;

    Rigidbody2D rb;
    Collider2D body;
    SpriteRenderer facingSprite;
    float moveInput, coyoteCounter, jumpBufferCounter, baseGravity;
    Vector2 platformVelocity;
    float platformContactTime = -1f;
    bool grounded;

    public bool IsGrounded => grounded;
    public float HorizontalSpeed => rb ? rb.linearVelocity.x : 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        body = GetComponent<Collider2D>();
        facingSprite = GetComponentInChildren<SpriteRenderer>();
        baseGravity = rb.gravityScale;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void Update()
    {
        ReadInput();
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame && GameManager.Instance != null)
            GameManager.Instance.TogglePause();
        if (jumpBufferCounter > 0f) jumpBufferCounter -= Time.unscaledDeltaTime;
    }

    void ReadInput()
    {
        moveInput = 0f;
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) moveInput -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) moveInput += 1f;
            if (keyboard.spaceKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame) QueueJump();
            if (keyboard.spaceKey.wasReleasedThisFrame || keyboard.wKey.wasReleasedThisFrame || keyboard.upArrowKey.wasReleasedThisFrame) CutJump();
        }
        var pad = Gamepad.current;
        if (pad != null)
        {
            float x = pad.leftStick.ReadValue().x;
            if (Mathf.Abs(x) > 0.15f) moveInput = x;
            if (pad.buttonSouth.wasPressedThisFrame) QueueJump();
            if (pad.buttonSouth.wasReleasedThisFrame) CutJump();
            if (pad.startButton.wasPressedThisFrame && GameManager.Instance != null) GameManager.Instance.TogglePause();
        }
        var mobileStick = MobileJoystick.Current;
        if (mobileStick != null && Mathf.Abs(mobileStick.Horizontal) > 0.01f)
            moveInput = mobileStick.Horizontal;
        moveInput = Mathf.Clamp(moveInput, -1f, 1f);
    }

    public void QueueJump() => jumpBufferCounter = jumpBuffer;

    public void CutJump()
    {
        if (rb != null && rb.linearVelocity.y > 0f)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
    }

    void FixedUpdate()
    {
        if (Time.fixedTime - platformContactTime > Time.fixedDeltaTime * 1.5f) platformVelocity = Vector2.zero;
        grounded = CheckGrounded();
        if (grounded) coyoteCounter = coyoteTime;
        else coyoteCounter = Mathf.Max(0f, coyoteCounter - Time.fixedDeltaTime);

        var velocity = rb.linearVelocity;
        velocity.x = Mathf.MoveTowards(velocity.x, moveInput * speed + (grounded ? platformVelocity.x : 0f), acceleration * Time.fixedDeltaTime);
        if (grounded && Mathf.Abs(platformVelocity.y) > 0.01f) velocity.y = platformVelocity.y;
        rb.linearVelocity = velocity;

        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            float jumpSpeed = Mathf.Sqrt(2f * Mathf.Abs(Physics2D.gravity.y * baseGravity) * jumpHeight);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpSpeed);
            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
            grounded = false;
            platformVelocity = Vector2.zero;
            Assignment6.A6Audio.PlayJump();
            if (GameManager.Instance != null) GameManager.Instance.NotifyJump();
        }

        float gravity = baseGravity;
        if (rb.linearVelocity.y < -0.05f) gravity *= fallGravityMultiplier;
        rb.gravityScale = gravity;
        if (facingSprite && Mathf.Abs(rb.linearVelocity.x) > 0.05f) facingSprite.flipX = rb.linearVelocity.x < 0f;
    }

    bool CheckGrounded()
    {
        if (body == null) return false;
        Bounds b = body.bounds;
        float width = Mathf.Max(0.08f, b.size.x * 0.72f);
        float y = b.min.y + 0.025f;
        Vector2 left = new Vector2(b.center.x - width * 0.5f, y);
        Vector2 right = new Vector2(b.center.x + width * 0.5f, y);
        float distance = groundCheck != null ? groundCheckRadius + 0.035f : 0.12f;
        return HasGround(left, distance) || HasGround((left + right) * 0.5f, distance) || HasGround(right, distance);
    }

    bool HasGround(Vector2 origin, float distance)
    {
        var hits = Physics2D.RaycastAll(origin, Vector2.down, distance, groundLayer);
        foreach (var hit in hits)
        {
            if (hit.collider == null || hit.collider.isTrigger || hit.normal.y <= 0.45f) continue;
            if (hit.rigidbody == rb || hit.collider.transform == transform || hit.collider.transform.IsChildOf(transform)) continue;
            return true;
        }
        return false;
    }

    public void SetMovingPlatformVelocity(Vector2 velocity)
    {
        platformVelocity = velocity;
        platformContactTime = Time.fixedTime;
    }

    void OnDrawGizmosSelected()
    {
        if (!body) return;
        Bounds b = body.bounds;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new Vector3(b.min.x + b.size.x * 0.14f, b.min.y + 0.025f), new Vector3(b.max.x - b.size.x * 0.14f, b.min.y + 0.025f));
    }
}
