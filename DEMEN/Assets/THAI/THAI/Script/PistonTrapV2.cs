using UnityEngine;

public class PistolTrapV2 : MonoBehaviour
{
    public Transform leftPistol;
    public Transform rightPistol;
    public float slamSpeed = 200f;
    public float slamAngle = 45f;
    private bool triggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            triggered = true;
        }
    }

    void Update()
    {
        if (triggered)
        {
            // Rotate left pistol clockwise
            leftPistol.localRotation = Quaternion.RotateTowards(
                leftPistol.localRotation,
                Quaternion.Euler(0, 0, -slamAngle),
                slamSpeed * Time.deltaTime
            );

            // Rotate right pistol counter-clockwise
            rightPistol.localRotation = Quaternion.RotateTowards(
                rightPistol.localRotation,
                Quaternion.Euler(0, 0, slamAngle),
                slamSpeed * Time.deltaTime
            );
        }
    }
}
