using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class SavePoint : MonoBehaviour
{
    private bool playerInRange = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) playerInRange = false;
    }

    void Update()
    {
        if (playerInRange && Keyboard.current.eKey.wasPressedThisFrame)
        {
            int slotIndex = PlayerPrefs.GetInt("CurrentSlot", 0);
            SaveData data = SaveSystem.LoadGame(slotIndex) ?? new SaveData();

            // Save scene
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                SceneSaveData sceneData = new SceneSaveData
                {
                    sceneName = SceneManager.GetActiveScene().name,
                    position = player.transform.position,
                    playTime = PlayTimeTracker.Instance != null ? PlayTimeTracker.Instance.GetPlayTime() : 0f
                };
                data.AddScene(sceneData);
            }

            // Save inventory
            InventoryManager invMgr = InventoryManager.Instance;
            if (invMgr != null)
            {
                data.inventory = invMgr.GetInventoryData();
            }

            SaveSystem.SaveGame(slotIndex, data);
            PlayerPrefs.SetInt("CurrentSlot", slotIndex);

            Debug.Log($"[SAVE] Slot {slotIndex} saved scene {SceneManager.GetActiveScene().name} + inventory {data.inventory.items.Count} items");
        }
    }
    
}
