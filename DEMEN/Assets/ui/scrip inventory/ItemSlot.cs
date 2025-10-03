using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IPointerClickHandler
{
    //=====ITEM DATA=====//
    public string itemName;
    public int quantity;
    public Sprite itemSprite;
    public bool isFull;
    public string itemDescription;
    public Sprite emptySprite;

    [SerializeField]
    private int maxNumberOfItems;

    //=====ITEM SLOT=====//
    [SerializeField]
    private TMP_Text quantityText;

    [SerializeField]
    private Image itemImage;

    //=====ITEM DESCRIPTION SLOT======//
    public Image itemDescriptionImage;
    public TMP_Text itemDescriptionNameText;
    public TMP_Text itemDescriptionText;


    public GameObject selectedShader;
    public bool thisItemSelected;

    private InventoryManager inventoryManager;


    private void Start()
    {
        inventoryManager = GameObject.Find("InventoryPanel").GetComponent<InventoryManager>();
    }


    public int Additem(string itemName, int quantity, Sprite itemSprite, string itemDescription)
    {
        //Chefck to see if the slot is already full
        if (isFull)
            return quantity;
        
        //UPDATE NAME
        this.itemName = itemName;

        //UPDATE IMAGE
        this.itemSprite = itemSprite;
        itemImage.sprite = itemSprite;
        
        //UPDATE DESCRIPTION
        this.itemDescription = itemDescription;

        //UPDATE QUATITY
        this.quantity += quantity;
        if(this.quantity >= maxNumberOfItems)
        {
            quantityText.text = maxNumberOfItems.ToString();
            quantityText.enabled = true;
            isFull = true;
            
            //Return the LEFTOVERS
            int extraItems = this.quantity - maxNumberOfItems;
            this.quantity = maxNumberOfItems;
            return extraItems;
        };

        //UPDATE QUANTITY TEXT
        quantityText.text =this.quantity.ToString();
        quantityText.enabled = true;

        return 0;


    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Left)
        {
            OnLeftClick();
        }
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClick();
        }
    }

    public void OnLeftClick()
    {
        inventoryManager.DeselectAllSlots();
        selectedShader.SetActive(true);
        thisItemSelected = true;
        itemDescriptionNameText.text = itemName;
        itemDescriptionText.text = itemDescription;
        itemDescriptionImage.sprite = itemSprite;
        if (itemDescriptionImage.sprite == null)       
            itemDescriptionImage.sprite = emptySprite;
        
    }
    public void OnRightClick()
    {

    }
}
