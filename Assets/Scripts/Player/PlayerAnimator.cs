using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    public Rigidbody2D rb;
    public SpriteRenderer spriteRenderer;
    public float runThreshold = 0.1f;

    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        if (rb == null) rb = GetComponentInParent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (rb == null) return;
        float horizontal = rb.linearVelocity.x;
        anim.SetFloat("Speed", Mathf.Abs(horizontal));
        anim.SetBool("Grounded", Mathf.Abs(rb.linearVelocity.y) < 0.08f);
        if (spriteRenderer != null && Mathf.Abs(horizontal) > runThreshold)
            spriteRenderer.flipX = horizontal < 0;
    }
}
