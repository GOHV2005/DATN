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
        GameObject player = GameObject.FindWithTag("Player");
        int slotIndex = PlayerPrefs.GetInt("CurrentSlot", -1);

        if (slotIndex != -1)
        {
            SaveData data = SaveSystem.LoadGame(slotIndex);
            if (data != null)
            {
                // Load vị trí player
                SceneSaveData sceneData = data.GetScene(SceneManager.GetActiveScene().name);
                if (sceneData != null && player != null)
                {
                    player.transform.position = sceneData.position;
                }

                // Load inventory
                InventoryManager invMgr = GameObject.Find("InventoryPanel")?.GetComponent<InventoryManager>();
                if (invMgr != null)
                {
                    invMgr.LoadInventoryData(data.inventory);
                }
            }
        }

        if (player == null)
            player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);

    }
}
