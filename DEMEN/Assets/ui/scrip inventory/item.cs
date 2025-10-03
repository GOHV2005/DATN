using Unity.VisualScripting;
using UnityEngine;

public class item : MonoBehaviour
{
    [SerializeField]
    private string itemName;

    [SerializeField]
    private int quantity;

    [SerializeField]
    private Sprite sprite;

    [TextArea]
    [SerializeField]
    private string itemDescription;

    private  InventoryManager inventoryManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inventoryManager = GameObject.Find("InventoryPanel").GetComponent<InventoryManager>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            int LeftOverItem = inventoryManager.AddItem(itemName,quantity, sprite, itemDescription);
            if (LeftOverItem <= 0)
                Destroy(gameObject);
            else
                quantity = LeftOverItem;
            Destroy(gameObject);

        }
    }
    
}
