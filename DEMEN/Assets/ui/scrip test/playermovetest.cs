using UnityEngine;
using UnityEngine.InputSystem; // dùng Input System package

public class PlayerController2D : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // đọc input theo Input System
        if (Keyboard.current != null)
        {
            moveInput.x = (Keyboard.current.dKey.isPressed ? 1 : 0) + (Keyboard.current.aKey.isPressed ? -1 : 0);
        }
    }

    void FixedUpdate()
    {
        // sử dụng linearVelocity thay vì velocity (obsolete)
        rb.linearVelocity = new Vector2(moveInput.x * speed, rb.linearVelocity.y);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "SceneTrigger")
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("NextScene");
        }
    }
}
