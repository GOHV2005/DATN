using UnityEngine;

public class NPCNearDetection : MonoBehaviour
{
    public float detectionRadius = 2f; // Khoảng cách "gần"
    private Animator animator;
    private bool isPlayerNear = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
            Debug.LogError("NPC thiếu Animator!");
    }

    void Update()
    {
        // Tìm Player (chỉ tìm 1 lần hoặc dùng cache nếu có nhiều NPC)
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.transform.position);
            bool wasNear = isPlayerNear;
            isPlayerNear = distance <= detectionRadius;

            // Chỉ cập nhật khi trạng thái thay đổi (tối ưu)
            if (wasNear != isPlayerNear)
            {
                animator.SetBool("isNearPlayer", isPlayerNear);
            }
        }
        else
        {
            // Không thấy Player → tắt
            if (isPlayerNear)
            {
                isPlayerNear = false;
                animator.SetBool("isNearPlayer", false);
            }
        }
    }

    // (Tùy chọn) Vẽ bán kính phát hiện trong Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}