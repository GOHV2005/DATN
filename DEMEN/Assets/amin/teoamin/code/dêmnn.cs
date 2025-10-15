using UnityEngine;
using System.Collections;

public class PlayerController2D : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    private bool isGrounded;
    private bool isDashing;
    private bool facingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Ground check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // === DI CHUYỂN TRÁI/PHẢI (A/D) ===
        float horizontal = 0f;
        if (Input.GetKey(KeyCode.A)) horizontal = -1f;
        if (Input.GetKey(KeyCode.D)) horizontal = 1f;

        // Flip character
        if ((horizontal > 0 && !facingRight) || (horizontal < 0 && facingRight))
        {
            Flip();
        }

        // Animation: Running
        anim.SetBool("isRunning", horizontal != 0 && isGrounded && !isDashing);

        // === NHẢY (K) ===
        if (Input.GetKeyDown(KeyCode.K) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
        anim.SetBool("isJumping", !isGrounded); // Tự động bật khi bay

        // === ĐÁNH (J) ===
        if (Input.GetKeyDown(KeyCode.J))
        {
            anim.SetBool("isAttacking", true);
            Invoke("ResetAttack", 0.3f); // Giả sử animation attack dài 0.3s
        }

        // === DASH (L) ===
        if (Input.GetKeyDown(KeyCode.L) && !isDashing)
        {
            float dashDirection = horizontal != 0 ? horizontal : (facingRight ? 1f : -1f);
            StartCoroutine(Dash(dashDirection));
        }
    }

    void FixedUpdate()
    {
        if (!isDashing)
        {
            float horizontal = 0f;
            if (Input.GetKey(KeyCode.A)) horizontal = -1f;
            if (Input.GetKey(KeyCode.D)) horizontal = 1f;

            rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);
        }
    }

    IEnumerator Dash(float direction)
    {
        isDashing = true;
        anim.SetBool("isDashing", true);

        // Tạm tắt gravity để dash mượt
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        float time = 0f;
        while (time < dashDuration)
        {
            rb.linearVelocity = new Vector2(direction * dashSpeed, 0f);
            time += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        rb.gravityScale = originalGravity;
        isDashing = false;
        anim.SetBool("isDashing", false);
    }

    void Flip()
    {
        facingRight = !facingRight;
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !facingRight;
        }
    }

    void ResetAttack()
    {
        anim.SetBool("isAttacking", false);
    }
}