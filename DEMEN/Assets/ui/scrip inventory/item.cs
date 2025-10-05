using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] public string itemName;
    [SerializeField] public int quantity = 1;
    [SerializeField] public Sprite sprite;
    [TextArea][SerializeField] public string itemDescription;

    private bool isPlayerInRange = false;

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            // Lấy InventoryManager Instance an toàn
            InventoryManager invMgr = InventoryManager.Instance;

            if (invMgr == null)
            {
                Debug.LogWarning($"Cannot pick up {itemName}: InventoryManager.Instance is null!");
                return;
            }

            // Thêm item vào inventory
            int leftover = invMgr.AddItem(
                itemName,
                quantity,
                sprite,
                itemDescription
            );

            if (leftover <= 0)
            {
                Destroy(gameObject); // hết số lượng → destroy item
            }
            else
            {
                quantity = leftover; // còn dư → giữ lại số lượng
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
            Debug.Log($"Player vào vùng item {itemName}, nhấn E để nhặt");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
            Debug.Log($"Player rời vùng item {itemName}");
        }
    }
}
