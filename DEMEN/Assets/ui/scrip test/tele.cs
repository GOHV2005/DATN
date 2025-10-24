using UnityEngine;

public class Teleport : MonoBehaviour
{
    [Header("Teleport target (nơi đến)")]
    public Transform target; // Cổng đích

    [Header("Chống lặp liên tục")]
    public float cooldownTime = 0.5f; // thời gian tạm ngưng teleport

    private bool canTeleport = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canTeleport) return;
        if (other.CompareTag("Player") && target != null)
        {
            // Dịch chuyển player
            other.transform.position = target.position;

            // Ngăn teleport liên tục (chống vòng lặp A ↔ B)
            StartCoroutine(TeleportCooldown(other));
        }
    }

    private System.Collections.IEnumerator TeleportCooldown(Collider2D player)
    {
        canTeleport = false;
        yield return new WaitForSeconds(cooldownTime);
        canTeleport = true;
    }
}