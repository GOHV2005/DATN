using UnityEngine;

public class TargetPoint : MonoBehaviour
{
    [Header("Door Reference")]
    public DoorController door;   // assign in Inspector

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            // Destroy bullet
            Destroy(other.gameObject);

            // Open the door
            if (door != null)
            {
                door.OpenDoor();
            }
        }
    }
}
