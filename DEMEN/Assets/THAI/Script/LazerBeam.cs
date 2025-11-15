using UnityEngine;

public class LaserBeam : MonoBehaviour
{
    public float duration = 1f;

    void Start()
    {
        Destroy(gameObject, duration); // Laser disappears after 1s
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player hit by laser!");
            // Apply damage or knockback here
        }
    }
}
