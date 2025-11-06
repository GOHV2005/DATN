using UnityEngine;

public class PistonTrap : MonoBehaviour
{
    public float pushForce = 10f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, pushForce);
                Debug.Log("Piston activated! Player pushed up.");
            }
        }
    }
}
