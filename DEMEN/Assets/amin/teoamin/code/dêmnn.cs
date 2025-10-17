using UnityEngine;
using System.Collections;

public class PlayerController2D : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;

    [Header("Ground Check (Optional - Visual Only)")]
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
        // === DI CHUYỂN TRÁI/PHẢI (A/D) ===
        float horizontal = 0f;
        if (Input.GetKey(KeyCode.A)) horizontal = -1f;
        if (Input.GetKey(KeyCode.D)) horizontal = 1f;

        // Flip hướng nhân vật
        if ((horizontal > 0 && !facingRight) || (horizontal < 0 && facingRight))
        {
            Flip();
        }

        // Animation: chạy
        anim.SetBool("isRunning", horizontal != 0 && isGrounded && !isDashing);

        // === NHẢY (K) ===
        if (Input.GetKeyDown(KeyCode.K) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false; // Chờ đến khi chạm đất lại
            anim.SetBool("isJumping", true);
        }

        // === ĐÁNH (J) ===
        if (Input.GetKeyDown(KeyCode.J))
        {
            anim.SetBool("isAttacking", true);
            Invoke("ResetAttack", 0.3f);
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

    // === VA CHẠM MẶT ĐẤT (Thay cho OverlapCircle) ===
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            isGrounded = true;
            anim.SetBool("isJumping", false);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            isGrounded = true;
            anim.SetBool("isJumping", false);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            isGrounded = false;
            anim.SetBool("isJumping", true);
        }
    }
}
