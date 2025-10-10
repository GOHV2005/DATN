using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController2D : MonoBehaviour
{
    [Header("Di chuyển")]
    public float speed = 5f;

    [Header("Nhảy")]
    public float jumpForce = 8f;
    public Transform groundCheck;          // điểm kiểm tra chạm đất
    public float groundCheckRadius = 0.2f; // bán kính kiểm tra
    public LayerMask groundLayer;          // lớp Ground (mặt đất)

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool facingRight = true;
    private bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (UIManager.IsUIOpen)
        {
            moveInput = Vector2.zero;
            return; // 🚫 dừng input khi UI đang mở
        }

        if (Keyboard.current != null)
        {
            moveInput.x = (Keyboard.current.dKey.isPressed ? 1 : 0)
                        + (Keyboard.current.aKey.isPressed ? -1 : 0);

            // 🟩 Kiểm tra phím nhảy
            if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
            {
                Jump();
            }
        }

        // xoay mặt theo hướng di chuyển
        if (moveInput.x > 0 && !facingRight) Flip();
        if (moveInput.x < 0 && facingRight) Flip();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput.x * speed, rb.linearVelocity.y);
        CheckGround();
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    void CheckGround()
    {
        // 🟨 Kiểm tra có đang chạm đất không
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    /*private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("SceneTrigger"))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("NextScene");
        }
    }*/

    private void OnDrawGizmosSelected()
    {
        // Vẽ vùng kiểm tra chạm đất trong Scene để dễ debug
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
