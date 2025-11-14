using UnityEngine;

public class IncomeObj : MonoBehaviour
{
    public Transform cameraTransform;
    public float parallaxFactor = 0.5f; // Lower = slower movement (farther away)
    private Vector3 previousCameraPosition;

    void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        previousCameraPosition = cameraTransform.position;
    }

    void LateUpdate()
    {
        Vector3 deltaMovement = cameraTransform.position - previousCameraPosition;
        transform.position += new Vector3(deltaMovement.x * parallaxFactor, deltaMovement.y * parallaxFactor, 0);
        previousCameraPosition = cameraTransform.position;
    }
}
