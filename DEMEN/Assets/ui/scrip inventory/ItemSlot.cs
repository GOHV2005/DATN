using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    //=====ITEM DATA=====//
    public string itemName;
    public int quantity;
    public Sprite itemSprite;
    public bool isFull;
    public string itemDescription;
    public Sprite emptySprite;

    [SerializeField] private int maxNumberOfItems = 99;

    //=====UI SLOT=====//
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Image itemImage;

    //=====ITEM DESCRIPTION=====//
    public Image itemDescriptionImage;
    public TMP_Text itemDescriptionNameText;
    public TMP_Text itemDescriptionText;

    public GameObject selectedShader;
    public bool thisItemSelected;

    // Panel chứa 2 nút Drop / Use
    public GameObject actionPanel;

    private InventoryManager inventoryManager;

    [Header("Drop Settings")]
    [SerializeField] private Vector3 dropScale = new Vector3(0.3f, 0.3f, 0.3f);
    [SerializeField] private int dropOrderInLayer = 5;

    // Drag ghost
    private GameObject dragIcon;
    private DragItem dragItemUI;
    private Canvas parentCanvas;

    private void Start()
    {
        parentCanvas = GetComponentInParent<Canvas>();
        inventoryManager = GetComponentInParent<InventoryManager>();
        UpdateSlotUI();
    }


    private void Update()
    {
        // Nếu action panel đang bật, click ngoài sẽ tắt
        if (actionPanel != null && actionPanel.activeSelf)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (!RectTransformUtility.RectangleContainsScreenPoint(
                    actionPanel.GetComponent<RectTransform>(),
                    Input.mousePosition,
                    parentCanvas.worldCamera))
                {
                    actionPanel.SetActive(false);
                }
            }
        }
    }

    //================== ADD ITEM ==================//
    public int Additem(string name, int qty, Sprite sprite, string desc)
    {
        Debug.Log($"[DEBUG] AddItem called: {name}, qty: {qty}");
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

    //================== UI ==================//
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
        if (itemDescriptionNameText != null)
            itemDescriptionNameText.text = itemName;
        if (itemDescriptionText != null)
            itemDescriptionText.text = itemDescription;
        if (itemDescriptionImage != null)
            itemDescriptionImage.sprite = itemSprite != null ? itemSprite : emptySprite;
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

    //================== SLOT SELECTION ==================//
    public void OnPointerClick(PointerEventData eventData)
    {
        // Nếu slot chưa được chọn → lần click đầu tiên
        if (!thisItemSelected)
        {
            inventoryManager?.DeselectAllSlots();
            selectedShader?.SetActive(true);
            thisItemSelected = true;

            if (!string.IsNullOrEmpty(itemName))
                UpdateDescription();
            else
                ClearDescription();

            // Không bật actionPanel lần đầu, chỉ chọn slot
            if (actionPanel != null)
                actionPanel.SetActive(false);
        }
        else
        {
            // Slot đã được chọn → lần click thứ hai
            if (!string.IsNullOrEmpty(itemName) && actionPanel != null)
            {
                actionPanel.SetActive(!actionPanel.activeSelf);
                UpdateActionPanel();
            }
        }
    }

    //================== DRAG & DROP ==================//
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!thisItemSelected || quantity <= 0 || string.IsNullOrEmpty(itemName))
        {
            eventData.pointerDrag = null;
            return;
        }

        dragIcon = Instantiate(Resources.Load<GameObject>("DragItemUI"), parentCanvas.transform);
        dragItemUI = dragIcon.GetComponent<DragItem>();
        dragItemUI.Setup(itemSprite, quantity);

        CanvasGroup cg = dragIcon.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;

        RectTransform rt = dragIcon.GetComponent<RectTransform>();
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            eventData.position,
            parentCanvas.worldCamera,
            out localPoint);
        rt.localPosition = localPoint;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIcon == null) return;

        RectTransform rt = dragIcon.GetComponent<RectTransform>();
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            eventData.position,
            parentCanvas.worldCamera,
            out localPoint);
        rt.localPosition = localPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragIcon != null) Destroy(dragIcon);
    }

    public void OnDrop(PointerEventData eventData)
    {
        ItemSlot draggedSlot = eventData.pointerDrag?.GetComponent<ItemSlot>();
        if (draggedSlot == null || draggedSlot == this) return;

        inventoryManager?.DeselectAllSlots();
        selectedShader?.SetActive(true);
        thisItemSelected = true;

        if (!string.IsNullOrEmpty(itemName)) UpdateDescription();
        else ClearDescription();

        // Nếu slot trống
        if (quantity <= 0)
        {
            Additem(draggedSlot.itemName, draggedSlot.quantity, draggedSlot.itemSprite, draggedSlot.itemDescription);
            draggedSlot.EmptySlot();
        }
        // Cùng loại
        else if (itemName == draggedSlot.itemName)
        {
            int total = quantity + draggedSlot.quantity;
            if (total <= maxNumberOfItems)
            {
                quantity = total;
                draggedSlot.EmptySlot();
            }
            else
            {
                quantity = maxNumberOfItems;
                draggedSlot.quantity = total - maxNumberOfItems;
            }
        }
        // Đổi chỗ
        else
        {
            string tempName = itemName;
            Sprite tempSprite = itemSprite;
            string tempDesc = itemDescription;
            int tempQty = quantity;

            itemName = draggedSlot.itemName;
            itemSprite = draggedSlot.itemSprite;
            itemDescription = draggedSlot.itemDescription;
            quantity = draggedSlot.quantity;
            itemImage.sprite = itemSprite;
            UpdateSlotUI();

            draggedSlot.itemName = tempName;
            draggedSlot.itemSprite = tempSprite;
            draggedSlot.itemDescription = tempDesc;
            draggedSlot.quantity = tempQty;
            draggedSlot.itemImage.sprite = tempSprite;
            draggedSlot.UpdateSlotUI();
        }

        UpdateSlotUI();
        draggedSlot.UpdateSlotUI();
    }

    //================== USE & DROP ==================//
    public void UseItem()
    {
        inventoryManager?.OnSlotChanged();

        if (quantity <= 0 || string.IsNullOrEmpty(itemName) || inventoryManager == null) return;

        bool usable = inventoryManager.UseItem(itemName);
        if (usable)
        {
            quantity -= 1;
            if (quantity <= 0) EmptySlot();
            else UpdateSlotUI();
        }

        if (actionPanel != null) actionPanel.SetActive(false);
    }

    public void DropItem()
    {
        inventoryManager?.OnSlotChanged();
        if (quantity <= 0 || string.IsNullOrEmpty(itemName)) return;

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        GameObject itemToDrop = new GameObject(itemName);
        Item newItem = itemToDrop.AddComponent<Item>();
        newItem.quantity = 1;
        newItem.itemName = itemName;
        newItem.sprite = itemSprite;
        newItem.itemDescription = itemDescription;

        SpriteRenderer sr = itemToDrop.AddComponent<SpriteRenderer>();
        sr.sprite = itemSprite;
        sr.sortingOrder = dropOrderInLayer;
        sr.sortingLayerName = "Ground";

        BoxCollider2D col = itemToDrop.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        Rigidbody2D rb = itemToDrop.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        float facingDir = player.transform.localScale.x > 0 ? 1f : -1f;
        itemToDrop.transform.position = player.transform.position + new Vector3(facingDir * 1f, 0f, 0f);
        itemToDrop.transform.localScale = dropScale;

        quantity -= 1;
        if (quantity <= 0) EmptySlot();
        else UpdateSlotUI();

        if (actionPanel != null) actionPanel.SetActive(false);
    }

    //================== EMPTY SLOT ==================//
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
    public void SafeUpdateUI()
    {
        // kiểm tra null trước khi set UI
        if (itemImage != null)
            itemImage.sprite = itemSprite != null ? itemSprite : emptySprite;

        if (quantityText != null)
        {
            quantityText.enabled = quantity > 1;
            quantityText.text = quantity > 1 ? quantity.ToString() : "";
        }

        if (itemDescriptionImage != null)
            itemDescriptionImage.sprite = itemSprite != null ? itemSprite : emptySprite;

        if (itemDescriptionNameText != null)
            itemDescriptionNameText.text = itemName;

        if (itemDescriptionText != null)
            itemDescriptionText.text = itemDescription;
    }
}
