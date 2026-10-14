using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerMovement : MonoBehaviour
{
    public float speed = 7f;
    public float jumpForce = 12f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.16f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Collider2D bodyCollider;
    private float moveInput;
    private bool jumpRequested;
    private bool grounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            jumpRequested = true;

        if (Input.GetKeyDown(KeyCode.Escape) && GameManager.Instance != null)
            GameManager.Instance.TogglePause();
    }

    private void FixedUpdate()
    {
        grounded = CheckGrounded();
        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);

        if (jumpRequested && grounded)
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        jumpRequested = false;
    }

    private bool CheckGrounded()
    {
        if (groundCheck != null)
            return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        Bounds b = bodyCollider.bounds;
        Vector2 origin = new Vector2(b.center.x, b.min.y + 0.03f);
        return Physics2D.Raycast(origin, Vector2.down, 0.12f, groundLayer);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
