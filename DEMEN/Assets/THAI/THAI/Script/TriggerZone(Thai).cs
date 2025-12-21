using UnityEngine;

public class TriggerZone : MonoBehaviour
{
    [SerializeField] private FallingSpike platform;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            platform.TriggerFall();
        }
    }
}
