using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    public float fallDelay = 3f;      // thời gian chờ trước khi rơi
    public float destroyDelay = 3f;   // (tùy chọn) tự hủy sau khi rơi
    private Rigidbody2D rb;
    private bool isFalling = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic; // ban đầu cố định
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Kiểm tra va chạm với nhân vật
        if (collision.gameObject.CompareTag("Player") && !isFalling)
        {
            isFalling = true;
            Invoke(nameof(Fall), fallDelay);
        }
    }

    void Fall()
    {
        rb.bodyType = RigidbodyType2D.Dynamic; // chuyển sang rơi
        Destroy(gameObject, destroyDelay);     // (tùy chọn) xóa sau khi rơi
    }
}
