using UnityEngine;
using System.Collections;

public class ShockwaveProjectile : MonoBehaviour
{
    public float speed = 8f;
    public float lifetime = 3f;           // Tổng thời gian tồn tại
    public float fadeDuration = 0.3f;     // Thời gian mờ dần trước khi hủy
    public float damage = 80f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private float startTime;
    private bool isFading = false;
    private int direction = 1; // 👈 THÊM DÒNG NÀY: Khai báo biến direction

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        if (rb == null)
            Debug.LogError("ShockwaveProjectile: Thiếu Rigidbody2D!");
        if (sr == null)
            Debug.LogError("ShockwaveProjectile: Thiếu SpriteRenderer!");

        startTime = Time.time;
        Destroy(gameObject, lifetime); // Tự hủy sau lifetime giây
    }

    public void Initialize(float dir, float dmg)
    {
        direction = dir > 0 ? 1 : -1; // 👈 GÁN DIRECTION TẠI ĐÂY
        damage = dmg;

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("ShockwaveProjectile: Thiếu Rigidbody2D trong Initialize!");
            return;
        }

        rb.linearVelocity = new Vector2(direction * speed, 0f);

        // 👇 XOAY THEO HƯỚNG BAY
        transform.rotation = Quaternion.Euler(0, 0, direction == 1 ? 0 : 180);
    }

    void Update()
    {
        // 👇 TÍNH TOÁN ĐỘ MỜ DẦN
        float elapsed = Time.time - startTime;
        float fadeStart = lifetime - fadeDuration;

        if (elapsed >= fadeStart && !isFading)
        {
            isFading = true;
            StartCoroutine(FadeOut());
        }
    }

    IEnumerator FadeOut()
    {
        float alpha = 1f;
        while (alpha > 0)
        {
            alpha -= Time.deltaTime / fadeDuration;
            if (sr != null)
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);
            yield return null;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController playerScript = other.GetComponent<PlayerController>();
            if (playerScript != null)
            {
                playerScript.TakeDamageFromEnemy(damage, transform.position);
            }
            Destroy(gameObject); // Hủy ngay nếu trúng player
        }
    }
}