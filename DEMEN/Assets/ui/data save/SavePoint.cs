using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class SavePoint : MonoBehaviour
{
    private bool playerInRange = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            playerInRange = false;
    }

    void Update()
    {
        if (playerInRange && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            int slotIndex = PlayerPrefs.GetInt("CurrentSlot", 0);
            SaveData data = SaveSystem.LoadGame(slotIndex) ?? new SaveData();

            GameObject player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                Debug.LogError("[SavePoint] Không tìm thấy Player!");
                return;
            }

            SceneSaveData sceneData = new SceneSaveData
            {
                sceneName = SceneManager.GetActiveScene().name,
                position = player.transform.position,
                playTime = PlayTimeTracker.Instance != null ? PlayTimeTracker.Instance.GetPlayTime() : 0f
            };

            // Thêm hoặc cập nhật scene trong SaveData
            data.AddScene(sceneData);

            // Lưu dữ liệu
            SaveSystem.SaveGame(slotIndex, data);
            PlayerPrefs.SetInt("CurrentSlot", slotIndex);

            Debug.Log($"[SAVE] Slot {slotIndex} lưu scene {sceneData.sceneName} | Pos={sceneData.position} | Time={sceneData.playTime:F2}s");
        }
    }
}
