using UnityEngine;

public class BossArenaTrigger : MonoBehaviour
{
    public BossMantisAI mantisBoss;
    public BossBeetleAI beetleBoss;
    private bool triggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(
            "===== TRIGGER ENTER =====\n" +
            $"Name: {other.name}\n" +
            $"Tag: {other.tag}\n" +
            $"Layer: {LayerMask.LayerToName(other.gameObject.layer)}\n" +
            $"IsTrigger: {other.isTrigger}\n" +
            $"Position: {other.transform.position}\n" +
            $"Time: {Time.time}\n" +
            $"Parent: {(other.transform.parent ? other.transform.parent.name : "None")}\n" +
            $"Has PlayerController: {(other.GetComponentInParent<PlayerController>() != null)}"
        );

        // ⚠️ GIỮ NGUYÊN LOGIC CỦA BẠN
        if (other.GetComponentInParent<PlayerController>() == null)
            return;

        // ❌ ĐÃ BẬT RỒI → BỎ
        if (triggered) return;
        triggered = true;

        Debug.Log("✅ PLAYER ENTER ARENA – START COMBAT");

        if (mantisBoss != null)
        {
            mantisBoss.StartIntroSequence();
        }

        if (beetleBoss != null)
        {
            beetleBoss.StartIntroSequence();
        }
    }
}
