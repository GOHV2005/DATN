using UnityEngine;

public class AutoSaveRAM : MonoBehaviour
{
    public static AutoSaveRAM Instance;

    [Header("=== AUTO SAVE RAM ===")]
    public float playerHealth;
    public InventoryData inventory;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // 🔹 GỌI TRƯỚC KHI CHUYỂN SCENE
    public void Capture()
    {
        var player = PlayerController.Instance;
        var inv = InventoryManager.Instance;

        if (player != null)
            playerHealth = player.CurrentHealth;

        if (inv != null)
            inventory = inv.GetInventoryData();

        Debug.Log("[AutoSaveRAM] ✔ Đã lưu RAM");
    }

    // 🔹 GỌI SAU KHI LOAD SCENE XONG
    public void Restore()
    {
        var player = PlayerController.Instance;
        var inv = InventoryManager.Instance;

        if (player != null)
            player.SetHealth(playerHealth);

        if (inv != null && inventory != null)
            inv.LoadInventoryData(inventory);

        Debug.Log("[AutoSaveRAM] ✔ Đã khôi phục RAM");
    }

    // 🔹 XOÁ RAM KHI PLAYER CHẾT
    public void Clear()
    {
        playerHealth = 0;
        inventory = null;
        Debug.Log("[AutoSaveRAM] ❌ Clear RAM");
    }
}
