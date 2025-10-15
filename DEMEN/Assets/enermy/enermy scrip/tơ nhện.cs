using UnityEngine;

public class WebProjectile : MonoBehaviour
{
    public float speed = 5f;
    public float lifetime = 2.5f;
    private bool hasHit = false;

    public void Initialize(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (hasHit) return;
        hasHit = true;

        // 👇 DEBUG: XEM VA CHẠM VỚI CÁI GÌ
        Debug.Log($"[Web] Va chạm với: {col.name} | Tag: '{col.tag}' | IsTrigger: {col.isTrigger}");

        if (col.CompareTag("Player"))
        {
            var spider = Object.FindAnyObjectByType<EnemySpider>();
            if (spider == null)
            {
                Debug.LogError("[Web] ❌ KHÔNG TÌM THẤY NHỆN! (EnemySpider không tồn tại)");
            }
            else
            {
                spider.OnWebHitPlayer();
                Debug.Log("[Web] ✅ TRÚNG PLAYER! Gọi OnWebHitPlayer()");
            }
        }
        else
        {
            Debug.Log("[Web] 💥 Dính vật cản, tự hủy");
        }

        
    }
}