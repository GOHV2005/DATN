using UnityEngine;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    [Header("Combat Settings")]
    public Transform attackPoint;         // Vị trí đánh (đặt empty object ở tay hoặc trước mặt)
    public float attackRange = 0.5f;      // Phạm vi đánh
    public float attackRate = 2f;         // Số lần tấn công / giây
    public int attackDamage = 20;         // Sát thương mỗi đòn

    public LayerMask enemyLayers;         // Layer của Enemy

    [Header("Visual Feedback")]
    public GameObject attackVisual;       // GameObject hiển thị vùng đánh (có renderer)
    public float visualDuration = 0.1f;   // Thời gian hiển thị vùng đánh

    private float nextAttackTime = 0f;

    void Update()
    {
        if (Time.time >= nextAttackTime)
        {
            if (Input.GetMouseButtonDown(0)) // Chuột trái
            {
                Attack();
                nextAttackTime = Time.time + 1f / attackRate;
            }
        }
    }

    void Attack()
    {
        // Hiển thị vùng đánh nếu có
        if (attackVisual != null)
        {
            attackVisual.SetActive(true);
            StartCoroutine(HideAttackVisual());
        }

        // Kiểm tra kẻ địch trong vùng đánh
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        // Gây damage cho tất cả enemy trúng
        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<EnemyHealth>()?.TakeDamage(attackDamage);
        }

        Debug.Log("Player attacked!");
    }

    IEnumerator HideAttackVisual()
    {
        yield return new WaitForSeconds(visualDuration);
        if (attackVisual != null)
            attackVisual.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}