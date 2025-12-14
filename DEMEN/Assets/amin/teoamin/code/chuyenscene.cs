using UnityEngine;
using UnityEngine.SceneManagement;

public class chuyenscene : MonoBehaviour
{
    public string SceneName;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            AutoSaveRAM.Instance?.Capture();
            SceneLoader.LoadScene(SceneName);
            SaveCheckpoint();
        }
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

        // Lưu inventory
        InventoryManager invMgr = InventoryManager.Instance;
        if (invMgr != null)
        {
            data.inventory = invMgr.GetInventoryData();
            var player = PlayerController.Instance;

            if (player != null)
            {


                // 🔒 LƯU MÁU FULL VÀO SAVE
                data.playerHealth = player.CurrentHealth;

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
