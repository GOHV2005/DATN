using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawn : MonoBehaviour
{
    private static GameObject playerInstance;

    void Awake()
    {
        if (playerInstance != null)
        {
            Destroy(gameObject);
            return;
        }

        playerInstance = GameObject.FindGameObjectWithTag("Player");

        if (playerInstance == null)
        {
            GameObject prefab = Resources.Load<GameObject>("Player");
            if (prefab != null)
                playerInstance = Instantiate(prefab, GetSpawnPosition(), Quaternion.identity);
            else
                Debug.LogError("[SPAWN] Không tìm thấy Player trong scene hoặc Resources!");
        }

        DontDestroyOnLoad(playerInstance);
        DontDestroyOnLoad(gameObject);
    }

    Vector3 GetSpawnPosition()
    {
        int slot = PlayerPrefs.GetInt("CurrentSlot", 0);
        SaveData data = SaveSystem.LoadGame(slot);
        if (data != null)
            return data.GetPosition();
        return new Vector3(-7f, -0.03f, 0f);
    }
}
