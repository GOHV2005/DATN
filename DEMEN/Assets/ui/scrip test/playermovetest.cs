using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController2D : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool facingRight = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (UIManager.IsUIOpen)
        {
            moveInput = Vector2.zero;
            return;  // 🚫 dừng input khi UI đang mở
        }
        if (Keyboard.current != null)
        {
            moveInput.x = (Keyboard.current.dKey.isPressed ? 1 : 0)
                        + (Keyboard.current.aKey.isPressed ? -1 : 0);
        }

        // xoay mặt theo hướng di chuyển
        if (moveInput.x > 0 && !facingRight) Flip();
        if (moveInput.x < 0 && facingRight) Flip();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput.x * speed, rb.linearVelocity.y);
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1; // lật player và toàn bộ con của nó
        transform.localScale = scale;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("SceneTrigger"))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("NextScene");
        }
    }
}
