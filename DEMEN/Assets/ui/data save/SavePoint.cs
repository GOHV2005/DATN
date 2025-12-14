using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class SavePoint : MonoBehaviour
{
    [Header("=== CÁC OBJECT CẦN LƯU TRẠNG THÁI ===")]
    public SaveableObject[] objectsToSave;
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

        string currentScene = SceneManager.GetActiveScene().name;

        SceneSaveData checkpoint = new SceneSaveData
        {
            sceneName = currentScene,
            position = transform.position,
            playTime = PlayTimeTracker.Instance != null ? PlayTimeTracker.Instance.GetPlayTime() : 0f
        };
        data.AddScene(checkpoint);

        PlayerPrefs.SetString("LastCheckpointScene", currentScene);
        PlayerPrefs.SetFloat("CheckpointX", transform.position.x);
        PlayerPrefs.SetFloat("CheckpointY", transform.position.y);
        PlayerPrefs.SetFloat("CheckpointZ", transform.position.z);

        // Lưu inventory
        InventoryManager invMgr = InventoryManager.Instance;
        if (invMgr != null)
        {
            data.inventory = invMgr.GetInventoryData();
            var player = PlayerController.Instance;

            if (player != null)
            {
                // ❤️ HỒI FULL MÁU KHI SAVE POINT
                player.CurrentHealth = player.maxHealth;

                // 🔒 LƯU MÁU FULL VÀO SAVE
                data.playerHealth = player.maxHealth;

                data.inventory.isHoldingLongden = player.IsHoldingLongden;
                data.inventory.isHoldingCuocChim = player.IsHoldingCuocChim;
                data.inventory.isHoldingKiem = player.IsHoldingKiem;
            }
        }


        // ✅ Lưu SaveableObject chỉ của scene hiện tại
        data.saveableObjects.RemoveAll(o => o.sceneName == currentScene); // xóa bản lưu cũ của scene này
        foreach (var obj in FindObjectsOfType<SaveableObject>())
        {
            if (obj.sceneName != currentScene) continue;

            data.saveableObjects.Add(new SaveData.SaveableObjectRef(
                obj.guid,
                obj.sceneName,
                obj.GetComponent<SaveData.ISaveable>()?.CaptureState() // nếu có state
            ));
        }
        SaveSystem.SaveGame(slotIndex, data);
        PlayerPrefs.SetInt("CurrentSlot", slotIndex);

        Debug.Log($"[CHECKPOINT] Saved scene '{currentScene}' with {data.saveableObjects.Count} saveable objects.");
    }

}