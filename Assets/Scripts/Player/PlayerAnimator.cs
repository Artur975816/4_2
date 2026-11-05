using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    public Rigidbody2D rb;
    public SpriteRenderer spriteRenderer;
    public float runThreshold = 0.1f;

    private Animator anim;
    private PlayerMovement movement;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        anim.updateMode = AnimatorUpdateMode.Normal;
        if (rb == null) rb = GetComponentInParent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        movement = GetComponentInParent<PlayerMovement>();
    }

    private void Update()
    {
        // Pause freezes the hero (Normal time). Victory / Game Over set timeScale to 0 and still need to animate.
        bool finished = GameManager.Instance != null && GameManager.Instance.IsFinished;
        anim.updateMode = finished ? AnimatorUpdateMode.UnscaledTime : AnimatorUpdateMode.Normal;
        if (rb == null) return;
        float horizontal = rb.linearVelocity.x;
        anim.SetFloat("Speed", Mathf.Abs(horizontal));
        anim.SetFloat("VerticalSpeed", rb.linearVelocity.y);
        anim.SetBool("Grounded", movement != null ? movement.IsGrounded : Mathf.Abs(rb.linearVelocity.y) < 0.08f);
        if (spriteRenderer != null && Mathf.Abs(horizontal) > runThreshold)
            spriteRenderer.flipX = horizontal < 0;
    }
}
