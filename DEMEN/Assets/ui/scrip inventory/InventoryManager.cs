using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [Header("Item SLot")]
    public ItemSlot[] itemSlot;

    public itemSO[] itemSOs;


    public bool UseItem(string ItemName)
    {
        for(int i = 0; i < itemSOs.Length; i++)
        {
            if (itemSOs[i].itemName == ItemName)
            {
                bool usable = itemSOs[i].UseItem();
                return usable;
            } 
        }
        return false;
    }
    public int AddItem(string itemName, int quantity, Sprite itemSprite, string itemDescription)
    {
        // Duyệt qua inventory từ ô đầu tiên -> cuối cùng
        for (int i = 0; i < itemSlot.Length; i++)
        {
            // Nếu slot này đang trống hoặc có cùng item và chưa full
            if (itemSlot[i].quantity == 0 ||
                (itemSlot[i].itemName == itemName && itemSlot[i].isFull == false))
            {
                int leftOver = itemSlot[i].Additem(itemName, quantity, itemSprite, itemDescription);

                // Nếu còn dư thì tiếp tục tìm ô tiếp theo
                if (leftOver > 0)
                    return AddItem(itemName, leftOver, itemSprite, itemDescription);

                return 0; // hết item → thoát
            }
        }

        // Nếu đi hết inventory mà vẫn còn dư thì trả về số dư
        return quantity;
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
