using UnityEngine;
using System.Collections;
public class SmoothCameraTarget : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Rigidbody2D playerRb;       // To read vertical velocity

    [Header("Settings")]
    public float horizontalBias = 2f;  // Distance ahead of player (X)
    public float verticalBias = 2f;    // Max vertical offset (Y)
    public float smoothSpeed = 5f;     // How fast the target moves
    private float facingDirSmooth = 1f;

    private Vector3 desiredPos;

    public IEnumerator ShakeCamera(float duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition;

        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = originalPos + new Vector3(offsetX, offsetY, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }

    void LateUpdate()
    {
        if (player == null || playerRb == null) return;

        // Facing direction (assuming you flip with localScale.x)
        float targetDir = Mathf.Sign(player.localScale.x);
        facingDirSmooth = Mathf.Lerp(facingDirSmooth, targetDir, Time.deltaTime * 8f); // smooth flip

        float xOffset = facingDirSmooth * horizontalBias;

        // Vertical offset based on velocity
        // Normalize velocity to [-1,1] range for interpolation
        float normalizedVelY = Mathf.Clamp(playerRb.linearVelocity.y / 15f, -0.7f, 0.7f);
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
