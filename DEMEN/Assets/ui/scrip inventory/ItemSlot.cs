using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IPointerClickHandler
{
    public string itemName;
    public int quantity;
    public Sprite itemSprite;
    public bool isFull;
    public string itemDescription;
    public Sprite emptySprite;

    [SerializeField] private int maxNumberOfItems = 99;

    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Image itemImage;

    public Image itemDescriptionImage;
    public TMP_Text itemDescriptionNameText;
    public TMP_Text itemDescriptionText;

    public GameObject selectedShader;
    public bool thisItemSelected;
    public GameObject actionPanel;

    [Header("Drop Settings")]
    [SerializeField] private Vector3 dropScale = new Vector3(0.3f, 0.3f, 0.3f);
    [SerializeField] private int dropOrderInLayer = 5;

    private InventoryManager inventoryManager;

    private void Start()
    {
        inventoryManager = GetComponentInParent<InventoryManager>();
        UpdateSlotUI();
    }

    private void Update()
    {
        if (actionPanel != null && actionPanel.activeSelf && Input.GetMouseButtonDown(0))
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(
                actionPanel.GetComponent<RectTransform>(),
                Input.mousePosition,
                null))
            {
                actionPanel.SetActive(false);
            }
        }
    }

    public int Additem(string name, int qty, Sprite sprite, string desc)
    {
        if (isFull) return qty;

        itemName = name;
        itemSprite = sprite;
        itemImage.sprite = sprite;
        itemDescription = desc;

        quantity += qty;
        if (quantity >= maxNumberOfItems)
        {
            int extra = quantity - maxNumberOfItems;
            quantity = maxNumberOfItems;
            isFull = true;
            UpdateSlotUI();
            return extra;
        }

        inventoryManager?.OnSlotChanged();
        UpdateSlotUI();
        return 0;
    }

    public void UpdateSlotUI()
    {
        if (itemImage != null)
            itemImage.sprite = itemSprite != null ? itemSprite : emptySprite;

        if (quantityText != null)
        {
            quantityText.enabled = quantity > 1;
            quantityText.text = quantity > 1 ? quantity.ToString() : "";
        }
    }

    public void UpdateDescription()
    {
        if (itemDescriptionNameText != null) itemDescriptionNameText.text = itemName;
        if (itemDescriptionText != null) itemDescriptionText.text = itemDescription;
        if (itemDescriptionImage != null) itemDescriptionImage.sprite = itemSprite != null ? itemSprite : emptySprite;
    }

    private void ClearDescription()
    {
        if (itemDescriptionNameText != null) itemDescriptionNameText.text = "";
        if (itemDescriptionText != null) itemDescriptionText.text = "";
        if (itemDescriptionImage != null) itemDescriptionImage.sprite = emptySprite;
    }

    public void UpdateActionPanel()
    {
        if (actionPanel != null)
        {
            actionPanel.SetActive(true);
            Button useBtn = actionPanel.transform.Find("UseButton")?.GetComponent<Button>();
            Button dropBtn = actionPanel.transform.Find("DropButton")?.GetComponent<Button>();

            if (useBtn != null) useBtn.interactable = quantity > 0;
            if (dropBtn != null) dropBtn.interactable = quantity > 0;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!thisItemSelected)
        {
            inventoryManager?.DeselectAllSlots();
            selectedShader?.SetActive(true);
            thisItemSelected = true;

            if (!string.IsNullOrEmpty(itemName)) UpdateDescription();
            else ClearDescription();

            if (actionPanel != null) actionPanel.SetActive(false);
        }
        else
        {
            if (!string.IsNullOrEmpty(itemName) && actionPanel != null)
            {
                actionPanel.SetActive(!actionPanel.activeSelf);
                UpdateActionPanel();
            }
        }
    }

    public void UseItem()
    {
        if (quantity <= 0 || string.IsNullOrEmpty(itemName) || inventoryManager == null) return;

        bool usable = inventoryManager.UseItem(itemName);
        if (usable)
        {
            quantity -= 1;
            if (quantity <= 0) EmptySlot();
            else UpdateSlotUI();
        }

        if (actionPanel != null) actionPanel.SetActive(false);
        inventoryManager?.OnSlotChanged();
    }

    public void DropItem()
    {
        if (quantity <= 0 || string.IsNullOrEmpty(itemName) || inventoryManager == null) return;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideAll();
            Time.timeScale = 1f;
        }

        PlayerController player = PlayerController.Instance;
        if (player != null)
        {
            // Callback chỉ chạy NẾU không bị hủy
            player.DropItem(itemName, quantity, itemSprite, itemDescription, () =>
            {
                // Chỉ trừ item nếu drop thành công
                quantity -= 1;
                if (quantity <= 0) EmptySlot();
                else UpdateSlotUI();
                inventoryManager?.OnSlotChanged();
            });
        }
        else
        {
            Debug.LogWarning("Player not found!");
        }

        if (actionPanel != null) actionPanel.SetActive(false);
    }

    public void EmptySlot()
    {
        inventoryManager?.OnSlotChanged();
        quantity = 0;
        isFull = false;
        itemName = "";
        itemSprite = emptySprite;
        itemImage.sprite = emptySprite;
        itemDescription = "";
        thisItemSelected = false;
        selectedShader?.SetActive(false);
        actionPanel?.SetActive(false);
        UpdateSlotUI();
        ClearDescription();
    }

    public void SetItem(string name, int qty, Sprite sprite, string desc)
    {
        itemName = name;
        quantity = qty;
        itemSprite = sprite;
        itemDescription = desc;
        UpdateSlotUI();
        UpdateDescription();
    }
}