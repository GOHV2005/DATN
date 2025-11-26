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
        int lastSlot = PlayerPrefs.GetInt("LastUsedSlot", 0);
        Debug.Log($"[InventoryManager] Using last saved slot: {lastSlot}");

        InventoryData data = SaveSystem.LoadInventory(lastSlot);
        if (data != null)
        {
            LoadInventoryData(data);
            Debug.Log($"[InventoryManager] Inventory loaded: {data.items.Count} items");
        }
        else
        {
            Debug.Log("[InventoryManager] No saved inventory found on startup.");
        }
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            itemSlots = GetComponentsInChildren<ItemSlot>(true);

            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            SceneManager.sceneLoaded += OnSceneLoaded;

            // ✅ Tự động load lại inventory khi khởi động game
            int currentSlot = PlayerPrefs.GetInt("LastSaveSlot", 0);
            InventoryData invData = SaveSystem.LoadInventory(currentSlot);
            if (invData != null && invData.items.Count > 0)
            {
                currentInventoryData = invData;
                LoadInventoryData(invData);
                Debug.Log($"[InventoryManager] Auto-loaded inventory on startup ({invData.items.Count} items).");
            }
            else
            {
                Debug.Log("[InventoryManager] No saved inventory found on startup.");
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
        Debug.Log($"[InventoryManager] Active scene changed → {newName}");

        if (newName.Contains("UI Start") || newName.StartsWith("UI Start Test 3==D"))
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (itemSlots == null || itemSlots.Length == 0)
            itemSlots = GetComponentsInChildren<ItemSlot>(true);

        if (persistAcrossLevels && currentInventoryData != null && currentInventoryData.items.Count > 0)
        {
            LoadInventoryData(currentInventoryData);
            Debug.Log($"[InventoryManager] Inventory restored ({currentInventoryData.items.Count} items)");
        }
        else
        {
            Debug.Log("[InventoryManager] No inventory data found to restore.");
        }
    }

    // ✅ Bổ sung hàm này để tránh lỗi "The name 'OnSceneLoaded' does not exist"
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name;
        Debug.Log($"[InventoryManager] Scene loaded: {sceneName}");

        if (sceneName.Contains("UI Start"))
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (itemSlots == null || itemSlots.Length == 0)
            itemSlots = GetComponentsInChildren<ItemSlot>(true);

        if (persistAcrossLevels && currentInventoryData != null && currentInventoryData.items.Count > 0)
        {
            LoadInventoryData(currentInventoryData);
            Debug.Log($"[InventoryManager] Restore inventory ({currentInventoryData.items.Count} items) after scene load.");
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
