using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [Header("Item SLot")]
    public ItemSlot[] itemSlot;

    public void AddItem(string itemName, int quantity, Sprite itemSprite, string itemDescription)
    {
        for (int i = 0; i < itemSlot.Length; i++) 
        {
            if (itemSlot[i].isFull == false)
            {
                itemSlot[i].Additem(itemName, quantity, itemSprite, itemDescription);
                return;
            }
        }
    }

    public void DeselectAllSlots()
    {
        for(int i = 0;i < itemSlot.Length; i++)
        {
            itemSlot[i].selectedShader.SetActive(false);
            itemSlot[i].thisItemSelected = false;
        }
    }
}
