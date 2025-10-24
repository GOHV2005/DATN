using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    public Transform cameraTransform;
    [Range(0f, 1f)] public float parallaxMultiplier = 0.5f;
    public bool infiniteScrolling = true;

    private Vector3 lastCameraPosition;
    private float textureUnitSizeX;

    void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main?.transform;

        lastCameraPosition = cameraTransform.position;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            textureUnitSizeX = sr.bounds.size.x;
        else
            infiniteScrolling = false;
    }

    void LateUpdate()
    {
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;

        // Apply parallax only on X axis
        transform.position += new Vector3(deltaMovement.x * parallaxMultiplier, 0f, 0f);

        lastCameraPosition = cameraTransform.position;

        if (infiniteScrolling && textureUnitSizeX > 0f)
        {
            float cameraOffsetX = cameraTransform.position.x - transform.position.x;
            if (Mathf.Abs(cameraOffsetX) >= textureUnitSizeX)
            {
                float offsetPositionX = cameraOffsetX % textureUnitSizeX;
                transform.position = new Vector3(cameraTransform.position.x + offsetPositionX, transform.position.y, transform.position.z);
            }
        }
    }
}
