using UnityEngine;

public class SmoothCameraTarget : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Rigidbody2D playerRb;       // To read vertical velocity

    [Header("Settings")]
    public float horizontalBias = 2f;  // Distance ahead of player (X)
    public float verticalBias = 2f;    // Max vertical offset (Y)
    public float smoothSpeed = 5f;     // How fast the target moves

    private Vector3 desiredPos;

    void LateUpdate()
    {
        if (player == null || playerRb == null) return;

        // Facing direction (assuming you flip with localScale.x)
        float facingDir = Mathf.Sign(player.localScale.x);

        // Horizontal offset
        float xOffset = facingDir * horizontalBias;

        // Vertical offset based on velocity
        // Normalize velocity to [-1,1] range for interpolation
        float normalizedVelY = Mathf.Clamp(playerRb.linearVelocity.y / 10f, -1f, 1f);
        float yOffset = normalizedVelY * verticalBias;

        // Desired position ahead of player
        desiredPos = new Vector3(
            player.position.x + xOffset,
            player.position.y + yOffset,
            transform.position.z // keep depth unchanged
        );

        // Smoothly move empty follow target
        transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);
    }
}
