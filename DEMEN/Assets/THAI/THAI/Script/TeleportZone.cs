using UnityEngine;

public class TeleportZone2D : MonoBehaviour
{
    public Transform teleportTarget;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.position = teleportTarget.position;
        }
    }
}
