using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class SavePoint : MonoBehaviour
{
    [Header("Optional Visual")]
    public CheckpointVisual checkpointVisual;

    // KHÔNG DÙNG BIẾN CỜ NỮA — KIỂM TRA TRỰC TIẾP MỖI FRAME
    void Update()
    {
        if (Keyboard.current == null) return;

        // ✅ KIỂM TRA TRỰC TIẾP: "CÓ PLAYER NÀO TRONG VÙNG KHÔNG?"
        bool isPlayerInside = IsPlayerInsideTrigger();

        checkpointVisual?.SetPlayerInRange(isPlayerInside);

        if (isPlayerInside && Keyboard.current.eKey.wasPressedThisFrame)
        {
            SaveCheckpoint();
        }
    }

    private bool IsPlayerInsideTrigger()
    {
        // Cách đơn giản: tìm tất cả collider trong vùng trigger
        Collider2D[] colliders = Physics2D.OverlapBoxAll(
            (Vector2)transform.position,
            GetComponent<BoxCollider2D>()?.size ?? Vector2.one,
            0f
        );

        foreach (var col in colliders)
        {
            if (col.CompareTag("Player"))
                return true;
        }
        return false;
    }

    private void SaveCheckpoint()
    {
        int slotIndex = PlayerPrefs.GetInt("CurrentSlot", 0);
        SaveData data = SaveSystem.LoadGame(slotIndex) ?? new SaveData();

        SceneSaveData checkpoint = new SceneSaveData
        {
            sceneName = SceneManager.GetActiveScene().name,
            position = transform.position,
            playTime = PlayTimeTracker.Instance != null ? PlayTimeTracker.Instance.GetPlayTime() : 0f
        };
        data.AddScene(checkpoint);

        PlayerPrefs.SetString("LastCheckpointScene", SceneManager.GetActiveScene().name);
        PlayerPrefs.SetFloat("CheckpointX", transform.position.x);
        PlayerPrefs.SetFloat("CheckpointY", transform.position.y);
        PlayerPrefs.SetFloat("CheckpointZ", transform.position.z);

        InventoryManager invMgr = InventoryManager.Instance;
        if (invMgr != null)
        {
            data.inventory = invMgr.GetInventoryData();
        }

        SaveSystem.SaveGame(slotIndex, data);
        PlayerPrefs.SetInt("CurrentSlot", slotIndex);

        Debug.Log($"[CHECKPOINT] Saved at {transform.position}");
    }
}