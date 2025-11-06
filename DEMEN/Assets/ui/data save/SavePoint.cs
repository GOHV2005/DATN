using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class SavePoint : MonoBehaviour
{
    [Header("Optional Visual")]
    public CheckpointVisual checkpointVisual; // Kéo CheckpointVisual vào đây
    private bool playerInRange = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))playerInRange = true; checkpointVisual?.SetPlayerInRange(true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) playerInRange = false; checkpointVisual?.SetPlayerInRange(false);
    }

    void Update()
    {
        if (playerInRange && Keyboard.current.eKey.wasPressedThisFrame)
        {
            int slotIndex = PlayerPrefs.GetInt("CurrentSlot", 0);
            SaveData data = SaveSystem.LoadGame(slotIndex) ?? new SaveData();

            // === LƯU CHECKPOINT ===
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                // Lưu scene & vị trí như checkpoint
                SceneSaveData checkpoint = new SceneSaveData
                {
                    sceneName = SceneManager.GetActiveScene().name,
                    position = transform.position, // ← DÙNG VỊ TRÍ CỦA CHECKPOINT, KHÔNG PHẢI PLAYER
                    playTime = PlayTimeTracker.Instance != null ? PlayTimeTracker.Instance.GetPlayTime() : 0f
                };
                data.AddScene(checkpoint);

                // Đánh dấu đây là checkpoint (tuỳ chọn)
                PlayerPrefs.SetString("LastCheckpointScene", SceneManager.GetActiveScene().name);
                PlayerPrefs.SetFloat("CheckpointX", transform.position.x);
                PlayerPrefs.SetFloat("CheckpointY", transform.position.y);
                PlayerPrefs.SetFloat("CheckpointZ", transform.position.z);
            }

            // Lưu inventory
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

}
