using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawn : MonoBehaviour
{
    public static PlayerSpawn Instance;
    public GameObject playerPrefab;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        SpawnPlayer();
    }

    public void SpawnPlayer()
    {
        // Tìm Player trong scene
        GameObject player = GameObject.FindWithTag("Player");

        int slotIndex = PlayerPrefs.GetInt("CurrentSlot", -1);

        if (slotIndex != -1)
        {
            SaveData data = SaveSystem.LoadGame(slotIndex);
            if (data != null)
            {
                SceneSaveData sceneData = data.GetScene(SceneManager.GetActiveScene().name);
                if (sceneData != null)
                {
                    // Dịch chuyển Player tới vị trí đã lưu
                    if (player != null)
                    {
                        player.transform.position = sceneData.position;
                        Debug.Log($"[SPAWN] Dịch chuyển Player tới {sceneData.position}");
                    }
                    else
                    {
                        Instantiate(playerPrefab, sceneData.position, Quaternion.identity);
                        Debug.Log($"[SPAWN] Spawn Player tại {sceneData.position}");
                    }
                    return;
                }
            }
        }

        // Nếu không có save → giữ nguyên vị trí đặt trong scene
        if (player == null)
        {
            Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            Debug.Log("[SPAWN] Spawn Player mới tại (0,0,0) (không có SaveData)");
        }
        else
        {
            Debug.Log("[SPAWN] Không có SaveData, giữ nguyên vị trí gốc của Player trong scene");
        }
    }
}
