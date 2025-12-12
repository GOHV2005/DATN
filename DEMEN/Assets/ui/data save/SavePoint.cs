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

    void Update()
    {
        if (Keyboard.current == null) return;

        bool isPlayerInside = IsPlayerInsideTrigger();
        checkpointVisual?.SetPlayerInRange(isPlayerInside);

        if (isPlayerInside && Keyboard.current.eKey.wasPressedThisFrame)
        {
            SaveCheckpoint();
        }
    }

    private bool IsPlayerInsideTrigger()
    {
        var boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null) return false;

        Collider2D[] colliders = Physics2D.OverlapBoxAll(
            (Vector2)transform.position,
            boxCollider.size,
            0f,
            1 << gameObject.layer // hoặc dùng LayerMask nếu cần
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

        string currentSceneName = SceneManager.GetActiveScene().name;

        // Lấy hoặc tạo SceneSaveData cho scene hiện tại
        SceneSaveData checkpoint = data.GetScene(currentSceneName);
        if (checkpoint == null)
        {
            checkpoint = new SceneSaveData();
            checkpoint.sceneName = currentSceneName;
        }

        checkpoint.position = transform.position;
        checkpoint.playTime = PlayTimeTracker.Instance != null ? PlayTimeTracker.Instance.GetPlayTime() : 0f;

        // 👇 LƯU ONLY SAVEABLE OBJECTS TRONG SCENE HIỆN TẠI
        checkpoint.existingObjects.Clear();
        var allSaveables = FindObjectsOfType<SaveableObject>();
        foreach (var obj in allSaveables)
        {
            checkpoint.existingObjects.Add(obj.guid);
        }

        data.AddScene(checkpoint);

        // Lưu checkpoint vào PlayerPrefs để respawn nhanh (nếu cần)
        PlayerPrefs.SetString("LastCheckpointScene", currentSceneName);
        PlayerPrefs.SetFloat("CheckpointX", transform.position.x);
        PlayerPrefs.SetFloat("CheckpointY", transform.position.y);
        PlayerPrefs.SetFloat("CheckpointZ", transform.position.z);

        // 👇 LƯU INVENTORY + TRẠNG THÁI TRANG BỊ
        data.inventory = new InventoryData();
        InventoryManager invMgr = InventoryManager.Instance;
        if (invMgr != null)
        {
            for (int i = 0; i < invMgr.itemSlots.Length; i++)
            {
                data.inventory.AddItem(invMgr.itemSlots[i], i);
            }

            var player = PlayerController.Instance;
            if (player != null)
            {
                data.inventory.isHoldingLongden = player.IsHoldingLongden;
                data.inventory.isHoldingCuocChim = player.IsHoldingCuocChim;
                data.inventory.isHoldingKiem = player.IsHoldingKiem;
            }
        }

        SaveSystem.SaveGame(slotIndex, data);
        PlayerPrefs.SetInt("CurrentSlot", slotIndex);

        Debug.Log($"[CHECKPOINT] Saved in {currentSceneName} with {checkpoint.existingObjects.Count} objects. " +
                  $"Longden={data.inventory.isHoldingLongden}, Cuoc={data.inventory.isHoldingCuocChim}, Kiem={data.inventory.isHoldingKiem}");
    }
}