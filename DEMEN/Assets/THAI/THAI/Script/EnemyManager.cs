using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [Header("Detection Zone")]
    public string playerTag = "Player";

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 10f;
    public float fireRate = 1f; // seconds between shots

    [Header("Animation")]
    public Animator animator;

    private bool playerInZone = false;
    private float nextFireTime = 0f;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInZone = true;
            nextFireTime = Time.time; // reset timer
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInZone = false;
        }
    }

    void Update()
    {
        if (playerInZone && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Shoot()
    {
        // Play shoot animation
        if (animator != null)
        {
            animator.SetTrigger("Shoot");
        }

        // Spawn bullet
        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = firePoint.right * bulletSpeed;
            }
        }
    }
}
