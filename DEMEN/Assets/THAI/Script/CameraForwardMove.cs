using UnityEngine;

public class CameraForwardMove : MonoBehaviour
{
    // Speed of the camera movement
    public float speed = 5f;

    void Update()
    {
        // Move the camera continuously to the right (X axis)
        transform.Translate(Vector3.right * speed * Time.deltaTime);

        // If you want vertical movement instead, use:
        // transform.Translate(Vector3.up * speed * Time.deltaTime);
    }
}
