using UnityEngine;
namespace EnemyPatrolling
{
    public class PlayerMovement : MonoBehaviour
    {
        public float moveSpeed = 5f; // Player movement speed
        private Rigidbody2D rb; // Reference to the Rigidbody2D for physics-based movement

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>(); // Get the Rigidbody2D component attached to the object
        }

        private void Update()
        {
            // Get user input for horizontal and vertical movement
            float horizontal = Input.GetAxis("Horizontal"); // Left/Right arrows or A/D keys
            float vertical = Input.GetAxis("Vertical"); // Up/Down arrows or W/S keys

            // Calculate movement vector
            Vector2 movement = new Vector2(horizontal, vertical) * moveSpeed;

            // Apply movement to Rigidbody2D to ensure physics-based movement
            rb.linearVelocity = movement; // The Rigidbody2D controls position based on velocity
        }
    }
}