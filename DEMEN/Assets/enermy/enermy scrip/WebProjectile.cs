using UnityEngine;

public class WebProjectile : MonoBehaviour
{
    public float speed = 5f;
    public float lifetime = 2.5f;
    private bool hasHit = false;
    private Collider2D webCollider;

    public void Initialize(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Start()
    {
        webCollider = GetComponent<Collider2D>();
        if (webCollider != null)
        {
            // 👇 TẮT COLLIDER 1 FRAME ĐỂ TRÁNH VA CHẠM TỨC THÌ
            webCollider.enabled = false;
            Invoke(nameof(EnableCollider), 0.5f);
        }
        Destroy(gameObject, lifetime);
    }

    void EnableCollider()
    {
        if (webCollider != null)
            webCollider.enabled = true;
    }

    void Update()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (hasHit) return;
        hasHit = true;

        Debug.Log($"[Web] Va chạm với: {col.name} | Tag: '{col.tag}'");

        if (col.CompareTag("Player"))
        {
            var spider = Object.FindAnyObjectByType<EnemySpider>();
            if (spider != null)
            {
                
                Debug.Log("[Web] ✅ TRÚNG PLAYER!");
            }
            Destroy(gameObject);
        }

       
    }
}