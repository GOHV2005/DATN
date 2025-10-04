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

    [SerializeField] private int maxNumberOfItems;

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
        inventoryManager = GameObject.Find("InventoryPanel").GetComponent<InventoryManager>();
        actionPanel.SetActive(false);

        parentCanvas = GetComponentInParent<Canvas>();
    }

    public void Update()
    {
        // Nếu action panel đang bật
        if (actionPanel != null && actionPanel.activeSelf)
        {
            // Nếu nhấn chuột trái
            if (Input.GetMouseButtonDown(0))
            {
                // Kiểm tra vị trí chuột có nằm ngoài panel không
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


    public int Additem(string itemName, int quantity, Sprite itemSprite, string itemDescription)
    {
        if (isFull) return quantity;

        this.itemName = itemName;
        this.itemSprite = itemSprite;
        itemImage.sprite = itemSprite;
        this.itemDescription = itemDescription;

        this.quantity += quantity;
        if (this.quantity >= maxNumberOfItems)
        {
            quantityText.text = maxNumberOfItems.ToString();
            quantityText.enabled = true;
            isFull = true;

            int extraItems = this.quantity - maxNumberOfItems;
            this.quantity = maxNumberOfItems;
            return extraItems;
        }

        quantityText.text = this.quantity.ToString();
        quantityText.enabled = true;

        return 0;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnLeftClick();
        }
    }

    public void OnLeftClick()
    {
        // Nếu chưa chọn slot
        if (!thisItemSelected)
        {
            if (inventoryManager != null)
                inventoryManager.DeselectAllSlots();

            if (selectedShader != null)
                selectedShader.SetActive(true);

            thisItemSelected = true;

            if (!string.IsNullOrEmpty(itemName))
                UpdateDescription();
            else
                ClearDescription();

            if (actionPanel != null)
                actionPanel.SetActive(false); // không bật panel khi slot trống
        }
        else
        {
            // Chỉ toggle panel nếu slot có item
            if (!string.IsNullOrEmpty(itemName) && actionPanel != null)
                actionPanel.SetActive(!actionPanel.activeSelf);
        }
    }


    //================= DRAG & DROP =================//
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (quantity <= 0) return;

        // Nếu slot chưa được select, vẫn select nó
        if (!thisItemSelected)
        {
            if (inventoryManager != null)
                inventoryManager.DeselectAllSlots();

            if (selectedShader != null)
                selectedShader.SetActive(true);

            thisItemSelected = true;

            // Cập nhật description
            if (!string.IsNullOrEmpty(itemName))
                UpdateDescription();
            else
                ClearDescription();

            // Lần drag đầu tiên không bật panel
            if (actionPanel != null)
                actionPanel.SetActive(false);
        }

        // Tạo ghost icon
        dragIcon = Instantiate(Resources.Load<GameObject>("DragItemUI"), parentCanvas.transform);
        dragItemUI = dragIcon.GetComponent<DragItem>();
        dragItemUI.Setup(itemSprite, quantity);
    }



    public void OnDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
        {
            dragIcon.transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
        {
            Destroy(dragIcon);
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        ItemSlot draggedSlot = eventData.pointerDrag.GetComponent<ItemSlot>();
        if (draggedSlot == null || draggedSlot == this) return;

        // Nếu slot này trống, chuyển toàn bộ
        if (quantity <= 0)
        {
            Additem(draggedSlot.itemName, draggedSlot.quantity, draggedSlot.itemSprite, draggedSlot.itemDescription);
            draggedSlot.EmptySlot();
        }
        // Nếu cùng loại item
        else if (itemName == draggedSlot.itemName)
        {
            int total = quantity + draggedSlot.quantity;
            if (total <= maxNumberOfItems)
            {
                // đủ chứa -> gộp hết
                quantity = total;
                quantityText.text = quantity.ToString();
                draggedSlot.EmptySlot();
            }
            else
            {
                // slot này full -> đặt max, còn dư lại cho draggedSlot
                quantity = maxNumberOfItems;
                quantityText.text = quantity.ToString();
                draggedSlot.quantity = total - maxNumberOfItems;
                draggedSlot.quantityText.text = draggedSlot.quantity.ToString();
            }
        }
        // Nếu khác loại item -> đổi chỗ
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
            quantityText.text = quantity > 0 ? quantity.ToString() : "";

            draggedSlot.itemName = tempName;
            draggedSlot.itemSprite = tempSprite;
            draggedSlot.itemDescription = tempDesc;
            draggedSlot.quantity = tempQty;
            draggedSlot.itemImage.sprite = tempSprite;
            draggedSlot.quantityText.text = tempQty > 0 ? tempQty.ToString() : "";
        }
    }


    public void UseItem()
    {
        if (quantity <= 0 || string.IsNullOrEmpty(itemName) || inventoryManager == null) return;
        bool usable = inventoryManager.UseItem(itemName);
        if (usable)
        {
            quantity -= 1;
            quantityText.text = quantity > 0 ? quantity.ToString() : "";
            if (quantity <= 0) EmptySlot();
        }
        if (actionPanel != null) actionPanel.SetActive(false);
    }

    public void DropItem()
    {
        if (quantity <= 0 || string.IsNullOrEmpty(itemName)) return;

        // 1. Tạo GameObject item trên mặt đất
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

        // 2. Đặt vị trí item trước player
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Vector3 dropPosition = player.transform.position;
            float facingDir = player.transform.localScale.x > 0 ? 1f : -1f;
            dropPosition += new Vector3(facingDir * 1.0f, 0f, 0f);
            itemToDrop.transform.position = dropPosition;
            itemToDrop.transform.localScale = dropScale;
        }

        // 3. Trừ số lượng trong slot
        quantity -= 1;
        quantityText.text = quantity > 0 ? quantity.ToString() : "";
        if (quantity <= 0) EmptySlot();

        if (actionPanel != null) actionPanel.SetActive(false);
    }

    private void EmptySlot()
    {
        quantity = 0;
        isFull = false;
        itemName = "";
        itemSprite = emptySprite;
        itemDescription = "";

        if (quantityText != null) quantityText.enabled = false;
        if (itemImage != null) itemImage.sprite = emptySprite;

        if (selectedShader != null) selectedShader.SetActive(false);
        thisItemSelected = false;
        if (actionPanel != null) actionPanel.SetActive(false);
    }

    // Tạo 1 hàm riêng để update Description khi click slot
    private void UpdateDescription()
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
        if (itemDescriptionNameText != null)
            itemDescriptionNameText.text = "";
        if (itemDescriptionText != null)
            itemDescriptionText.text = "";
        if (itemDescriptionImage != null)
            itemDescriptionImage.sprite = emptySprite;
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


}
