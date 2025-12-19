using UnityEngine;

public class WebProjectile : MonoBehaviour
{
    public float speed = 5f;
    public float lifetime = 2.5f;
    private bool hasHit = false;
    private Collider2D webCollider;
    private Vector2 moveDirection;
    private EnemySpider ownerSpider;

    public void Initialize(Vector2 direction, float customSpeed = -1f)
    {
        moveDirection = direction.normalized;

        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        if (customSpeed > 0)
            speed = customSpeed;
    }

    public void SetOwner(EnemySpider spider)
    {
        ownerSpider = spider;
    }

    void Start()
    {
        webCollider = GetComponent<Collider2D>();
        if (webCollider != null)
        {
            webCollider.enabled = false;
            Invoke(nameof(EnableCollider), 0.05f);
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
        // ✅ Fix lỗi CS0034: Ép moveDirection thành Vector3 hoặc cộng đúng kiểu
        transform.position += (Vector3)moveDirection * speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (hasHit) return;
        hasHit = true;

        Debug.Log($"[Web] Va chạm với: {col.name} | Tag: '{col.tag}'");

        if (col.CompareTag("Player"))
        {
            if (ownerSpider != null)
            {
                ownerSpider.OnWebHitPlayer();
                Debug.Log("[Web] ✅ TRÚNG PLAYER!");
            }
            Destroy(gameObject);
        }
    }
}