using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
[System.Serializable]
public class ItemPrefabPair
{
    public string itemName;
    public GameObject prefab;
}
public class InventoryManager : MonoBehaviour
{
    [Header("Item Slots")]
    public ItemSlot[] itemSlots;
    public itemSO[] itemSOs;

    public static InventoryManager Instance;

    [Header("Giữ item khi qua level?")]
    public bool persistAcrossLevels = true;

    private InventoryData currentInventoryData;

    public List<ItemPrefabPair> itemPrefabs = new List<ItemPrefabPair>();
    // ========================== INIT ==========================
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            itemSlots = GetComponentsInChildren<ItemSlot>(true);

            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            SceneManager.sceneLoaded += OnSceneLoaded;

            // ✅ CHỈ LOAD TỪ SaveSlotX.json
            int currentSlot = PlayerPrefs.GetInt("CurrentSlot", 0);
            SaveData saveData = SaveSystem.LoadGame(currentSlot); // 👈 LOAD SaveData
            if (saveData != null)
            {
                currentInventoryData = saveData.inventory; // 👈 LẤY inventory từ SaveData
                LoadInventoryData(currentInventoryData);
                Debug.Log($"[InventoryManager] Loaded inventory from SaveSlot{currentSlot}.json");
            }
            else
            {
                Debug.Log("[InventoryManager] No saved inventory found.");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void RemoveItem(string itemName, int amount)
    {
        if (itemSlots == null) return;

        for (int i = itemSlots.Length - 1; i >= 0; i--)
        {
            if (itemSlots[i].itemName == itemName && itemSlots[i].quantity > 0)
            {
                int remove = Mathf.Min(amount, itemSlots[i].quantity);
                itemSlots[i].quantity -= remove;
                amount -= remove;

                if (itemSlots[i].quantity <= 0)
                {
                    itemSlots[i].EmptySlot();
                }
                else
                {
                    itemSlots[i].UpdateSlotUI();
                }

                if (amount <= 0) break;
            }
        }
        OnSlotChanged(); // Lưu save nếu cần
    }
    public GameObject GetItemPrefab(string itemName)
    {
        var pair = itemPrefabs.Find(p => p.itemName == itemName);
        return pair != null ? pair.prefab : null;
    }
    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ========================== SCENE HANDLING ==========================
    private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        string newName = newScene.name;
        if (newName.Contains("UI Start"))
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        // 👇 LƯU TRẠNG THÁI HIỆN TẠI TRƯỚC KHI LOAD LẠI
        if (itemSlots != null && itemSlots.Length > 0)
        {
            currentInventoryData = GetInventoryData(); // 👈 LƯU LẠI
        }

        if (itemSlots == null || itemSlots.Length == 0)
            itemSlots = GetComponentsInChildren<ItemSlot>(true);

        if (persistAcrossLevels && currentInventoryData != null && currentInventoryData.items.Count > 0)
        {
            LoadInventoryData(currentInventoryData);
            Debug.Log($"[InventoryManager] Inventory restored ({currentInventoryData.items.Count} items)");
        }
    }

    // ✅ Bổ sung hàm này để tránh lỗi "The name 'OnSceneLoaded' does not exist"
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name;
        if (sceneName.Contains("UI Start"))
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        // 👇 LƯU TRẠNG THÁI HIỆN TẠI
        if (itemSlots != null && itemSlots.Length > 0)
        {
            currentInventoryData = GetInventoryData();
        }

        if (itemSlots == null || itemSlots.Length == 0)
            itemSlots = GetComponentsInChildren<ItemSlot>(true);

        if (persistAcrossLevels && currentInventoryData != null && currentInventoryData.items.Count > 0)
        {
            LoadInventoryData(currentInventoryData);
            Debug.Log($"[InventoryManager] Restored inventory after scene load.");
        }
    }

    // ========================== ITEM LOGIC ==========================
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

    // ========================== SAVE / LOAD ==========================
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
                    slotIndex = i
                });
            }
        }

        currentInventoryData = data;
        return data;
    }

    public void LoadInventoryData(InventoryData data)
    {
        if (data == null)
        {
            Debug.LogWarning("[InventoryManager] LoadInventoryData() → data is null!");
            return;
        }

        if (itemSlots == null || itemSlots.Length == 0)
            itemSlots = GetComponentsInChildren<ItemSlot>(true);

        foreach (var slot in itemSlots)
            slot.EmptySlot();

        foreach (var itemData in data.items)
        {
            if (itemData.slotIndex < 0 || itemData.slotIndex >= itemSlots.Length)
                continue;

            Sprite sprite = GetSpriteByName(itemData.spriteName);

            itemSlots[itemData.slotIndex].SetItem(
                itemData.itemName,
                itemData.quantity,
                sprite,
                itemData.itemDescription
            );
        }

        currentInventoryData = data;
        Debug.Log($"[InventoryManager] Inventory loaded: {data.items.Count} items");
    }

    // ========================== SLOT MANAGEMENT ==========================
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

    // ========================== SPRITE HELPER ==========================
    private Sprite GetSpriteByName(string spriteName)
    {
        if (string.IsNullOrEmpty(spriteName))
            return null;

        Sprite sprite = Resources.Load<Sprite>(spriteName);
        if (sprite != null)
            return sprite;

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
