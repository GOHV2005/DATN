using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class SavePoint : MonoBehaviour
{
    private GameObject player;
    private bool playerInRange = false;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            Debug.LogError("[SavePoint] Không tìm thấy Player trong scene!");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("[SavePoint] Player đã vào vùng save point");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("[SavePoint] Player đã rời vùng save point");
        }
    }

    private void Update()
    {
        if (playerInRange && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            SaveGame();
        }
    }

    private void SaveGame()
    {
        if (player == null) return;

        Vector3 pos = player.transform.position;
        string scene = SceneManager.GetActiveScene().name;
        float time = PlayTimeTracker.Instance.GetPlayTime();

        SaveData data = new SaveData(pos, scene, time);
        int slot = PlayerPrefs.GetInt("CurrentSlot", 0); // lưu theo slot hiện tại
        SaveSystem.SaveGame(data, slot);

        Debug.Log($"[SAVE] Slot {slot} lưu tại {scene} | Pos={pos} | PlayTime={time}");
    }
}
