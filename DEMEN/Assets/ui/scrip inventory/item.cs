using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] public string itemName;
    [SerializeField] public int quantity = 1;
    [SerializeField] public Sprite sprite;
    [TextArea][SerializeField] public string itemDescription;
    [SerializeField] private string groundTag = "Ground";
    [SerializeField] private float pickupRange = 1.2f;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (PlayerController.Instance == null || PlayerController.Instance.feetPoint == null)
            return;

        float distance = Vector2.Distance(transform.position, PlayerController.Instance.feetPoint.position);
        if (distance <= pickupRange && Input.GetKeyDown(KeyCode.E))
        {
            Pickup();
        }
    }

    private void Pickup()
    {
        InventoryManager invMgr = InventoryManager.Instance;
        if (invMgr == null)
        {
            Debug.LogWarning($"Cannot pick up {itemName}: InventoryManager.Instance is null!");
            return;
        }

        int leftover = invMgr.AddItem(itemName, quantity, sprite, itemDescription);

        if (leftover <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            quantity = leftover;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(groundTag))
        {
            FreezeItem();
        }
    }

    private void FreezeItem()
    {
        if (rb == null) return;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Static;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        Gizmos.DrawSphere(transform.position, pickupRange);
    }
}