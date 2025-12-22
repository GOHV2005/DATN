using UnityEngine;

public class damerock : MonoBehaviour
{
    [Header("=== SÁT THƯƠNG ===")]
    public float damage = 1f;
    public float damageCooldown = 0f;
    private float lastDamageTime = 0f;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player") && Time.time - lastDamageTime > damageCooldown)
        {
            PlayerController player = collision.collider.GetComponent<PlayerController>();
            if (player != null)
            {
                AttackDirection dir = player.GetAttackDirection(transform.position);
                player.TakeDamage(damage, dir);
                lastDamageTime = Time.time;
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && Time.time - lastDamageTime > damageCooldown)
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                AttackDirection dir = player.GetAttackDirection(transform.position);
                player.TakeDamage(damage, dir);
                lastDamageTime = Time.time;
            }
        }
    }

}
