using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InventoryManager : MonoBehaviour
{
    [Header("Item Slots")]
    public ItemSlot[] itemSlots;
    public itemSO[] itemSOs;

    public static InventoryManager Instance;

    [Header("Giữ item khi qua level?")]
    public bool persistAcrossLevels = true;

    private InventoryData currentInventoryData;

    private void Awake()
    {
        // 🔒 Singleton pattern — chỉ có 1 InventoryManager tồn tại
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (itemSlots == null || itemSlots.Length == 0)
                itemSlots = GetComponentsInChildren<ItemSlot>(true);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Ngăn bị gắn event nhiều lần
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 🔥 Khi scene mới được load
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name;

        // 🔸 Nếu là menu → clear inventory
        if (sceneName == "UI Start Test 3==D")
        {
            Debug.Log($"[InventoryManager] Scene {sceneName} là menu → giữ inventory (không clear).");
            gameObject.SetActive(false);
            return;
        }


        // 🔸 Nếu không có UIManager → tạm ẩn Inventory
        var uiManagers = Object.FindObjectsByType<UIManager>(FindObjectsSortMode.None);
        if (uiManagers.Length == 0)
        {
            Debug.LogWarning($"[InventoryManager] Scene {sceneName} KHÔNG có UIManager → Inventory tạm ẩn.");
            gameObject.SetActive(false);
            return;
        }

        // 🔸 Kích hoạt lại inventory
        gameObject.SetActive(true);

        // 🔸 Gán lại itemSlots nếu canvas bị rebuild
        if (itemSlots == null || itemSlots.Length == 0)
            itemSlots = GetComponentsInChildren<ItemSlot>(true);

        // ✅ KHÔNG clear ở đây nữa, chỉ load lại nếu có data
        if (persistAcrossLevels && currentInventoryData != null && currentInventoryData.items.Count > 0)
        {
            Debug.Log($"[InventoryManager] Restore inventory ({currentInventoryData.items.Count} items) sau khi scene {sceneName} load xong.");
            LoadInventoryData(currentInventoryData);
        }
        else
        {
            Debug.Log($"[InventoryManager] Scene {sceneName} load xong, chưa có inventory data.");
        }
    }

    // ==================== ITEM LOGIC ====================

    public bool UseItem(string itemName)
    {
        foreach (var so in itemSOs)
        {
            if (so.itemName == itemName)
                return so.UseItem();
        }
        return false;
    }

    public int AddItem(string name, int qty, Sprite sprite, string desc)
    {
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i].quantity == 0 ||
                (itemSlots[i].itemName == name && !itemSlots[i].isFull))
            {
                int leftover = itemSlots[i].Additem(name, qty, sprite, desc);
                if (leftover > 0)
                    return AddItem(name, leftover, sprite, desc);
                return 0;
            }
        }
        return qty;
    }

    // ✅ CHỈNH HÀM NÀY để lưu đúng vị trí từng item
    public InventoryData GetInventoryData()
    {
        InventoryData data = new InventoryData();

        for (int i = 0; i < itemSlots.Length; i++)
        {
            ItemSlot slot = itemSlots[i];
            if (!string.IsNullOrEmpty(slot.itemName) && slot.quantity > 0)
            {
                data.items.Add(new ItemData
                {
                    itemName = slot.itemName,
                    quantity = slot.quantity,
                    spriteName = slot.itemSprite != null ? slot.itemSprite.name : "",
                    itemDescription = slot.itemDescription,
                    slotIndex = i // ✅ lưu lại đúng vị trí slot
                });
            }
        }

        return data;
    }

    // ✅ Load lại inventory, giữ đúng slot
    public void LoadInventoryData(InventoryData data)
    {
        if (itemSlots == null || itemSlots.Length == 0)
            itemSlots = GetComponentsInChildren<ItemSlot>(true);

        // 🔹 Bước 1: clear sạch toàn bộ slot trước khi load
        foreach (var slot in itemSlots)
            slot.EmptySlot();

        // 🔹 Bước 2: load dữ liệu từ save
        for (int i = 0; i < data.items.Count && i < itemSlots.Length; i++)
        {
            var itemData = data.items[i];
            Sprite sprite = GetSpriteByName(itemData.spriteName);

            itemSlots[i].SetItem(
                itemData.itemName,
                itemData.quantity,
                sprite,
                itemData.itemDescription
            );
        }

        Debug.Log($"[InventoryManager] Inventory loaded: {data.items.Count} items");
    }

    public void DeselectAllSlots()
    {
        foreach (var slot in itemSlots)
        {
            slot.selectedShader?.SetActive(false);
            slot.thisItemSelected = false;
        }
    }

    public void OnSlotChanged()
    {
        currentInventoryData = GetInventoryData();
    }

    public void ClearInventory()
    {
        if (itemSlots == null || itemSlots.Length == 0)
            itemSlots = GetComponentsInChildren<ItemSlot>(true);

        foreach (var slot in itemSlots)
        {
            slot.itemName = "";
            slot.quantity = 0;
            slot.itemSprite = slot.emptySprite;
            slot.itemDescription = "";
            slot.isFull = false;
            slot.UpdateSlotUI();
        }

        currentInventoryData = new InventoryData();
        Debug.Log("[InventoryManager] Inventory cleared");
    }
    private Sprite GetSpriteByName(string spriteName)
    {
        if (string.IsNullOrEmpty(spriteName))
            return null;

        // 🔍 Tìm tất cả sprite có thể load từ Resources
        Sprite sprite = Resources.Load<Sprite>(spriteName);
        if (sprite != null)
            return sprite;

        // 🔍 Nếu không tìm được, thử tìm trong tất cả sprite đã load
        Sprite[] allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
        foreach (var s in allSprites)
        {
            if (s.name == spriteName)
                return s;
        }

        Debug.LogWarning($"[InventoryManager] Không tìm thấy sprite '{spriteName}'!");
        return null;
    }

}
