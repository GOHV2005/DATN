using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InventoryManager : MonoBehaviour
{
    [Header("Item Slots")]
    public ItemSlot[] itemSlots;

    public itemSO[] itemSOs;

    public static InventoryManager Instance;

    private InventoryData currentInventoryData;

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Gán ItemSlot nếu chưa có
            if (itemSlots == null || itemSlots.Length == 0)
                itemSlots = GetComponentsInChildren<ItemSlot>();
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Đăng ký scene loaded
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Nếu scene có player → bật canvas
        bool hasPlayer = GameObject.FindWithTag("Player") != null;
        gameObject.SetActive(hasPlayer);  // chỉ bật nếu scene có player

        // Gán lại ItemSlot (trường hợp UI canvas được rebuild)
        if (itemSlots == null || itemSlots.Length == 0)
            itemSlots = GetComponentsInChildren<ItemSlot>();

        // Load lại inventory nếu có dữ liệu
        if (currentInventoryData != null && hasPlayer)
            LoadInventoryData(currentInventoryData);
    }

    // ==================== ITEM MANAGEMENT ====================

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
            if (itemSlots[i].quantity == 0 || (itemSlots[i].itemName == name && !itemSlots[i].isFull))
            {
                int leftover = itemSlots[i].Additem(name, qty, sprite, desc);
                if (leftover > 0) return AddItem(name, leftover, sprite, desc);
                return 0;
            }
        }
        return qty;
    }

    public InventoryData GetInventoryData()
    {
        InventoryData data = new InventoryData();
        for (int i = 0; i < itemSlots.Length; i++)
        {
            var slot = itemSlots[i];
            if (!string.IsNullOrEmpty(slot.itemName))
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
        return data;
    }

    public void LoadInventoryData(InventoryData data)
    {
        foreach (var item in data.items)
        {
            int i = item.slotIndex;
            if (i >= 0 && i < itemSlots.Length)
            {
                Sprite sprite = null;
                if (!string.IsNullOrEmpty(item.spriteName))
                    sprite = Resources.Load<Sprite>("Sprites/Items/" + item.spriteName);

                itemSlots[i].Additem(item.itemName, item.quantity, sprite ?? itemSlots[i].emptySprite, item.itemDescription);
                itemSlots[i].UpdateSlotUI();
                itemSlots[i].UpdateDescription();
            }
        }
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
}
