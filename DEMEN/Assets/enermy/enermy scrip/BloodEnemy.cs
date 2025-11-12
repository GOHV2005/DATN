using UnityEngine;

public class Blood : MonoBehaviour
{
    [Header("Settings")]
    public float lifetime = 0.5f;         // Thời gian tồn tại
    public float speed = 2f;              // Tốc độ bay
    public float randomScaleMin = 0.5f;   // Scale min
    public float randomScaleMax = 1.2f;   // Scale max

    private Vector2 direction;
    private float timer = 0f;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        // Scale ngẫu nhiên
        float scale = Random.Range(randomScaleMin, randomScaleMax);
        transform.localScale = Vector3.one * scale;

        // Hướng bay ngẫu nhiên, có thể tinh chỉnh theo knockback
        direction = Random.insideUnitCircle.normalized;

        // Nếu muốn bay theo hướng đẩy của kẻ tấn công, set direction từ TakeDamage
    }

    void Update()
    {
        // Bay theo hướng
        transform.position += (Vector3)direction * speed * Time.deltaTime;

        // Giảm alpha dần
        timer += Time.deltaTime;
        if (sr != null)
        {
            Color c = sr.color;
            c.a = Mathf.Lerp(1f, 0f, timer / lifetime);
            sr.color = c;
        }

        // Hủy sau lifetime
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    // Nếu muốn set hướng từ ngoài (ví dụ knockback của kẻ tấn công)
    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }
}
