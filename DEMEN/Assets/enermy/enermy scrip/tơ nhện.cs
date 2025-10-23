// WebProjectile.cs
using UnityEngine;

public class WebProjectile : MonoBehaviour
{
    public float speed = 5f;
    public float lifetime = 2.5f;
    private Vector2 direction;
    private bool hasHit = false; // 👈 tránh hủy nhiều lần

    public void Initialize(Vector2 dir)
    {
        direction = dir.normalized;
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (hasHit) return; // 👈 chỉ xử lý 1 lần
        hasHit = true;

        if (col.CompareTag("Player"))
        {
            var spider = Object.FindAnyObjectByType<EnemySpider>();
            spider?.OnWebHitPlayer();
            Debug.Log("[Web] Trúng player!");
        }
        else
        {
            Debug.Log("[Web] Dính vật cản: " + col.name);
        }

        //Destroy(gameObject);
    }
}