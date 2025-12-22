using UnityEngine;

public class FallingSpike : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool isFalling = false;

    [SerializeField] private float fallDelay = 0.5f;
    [SerializeField] private AudioSource fallSound;
    [SerializeField] private AudioSource hitSound;

    void OnEnable()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true; // platform stays frozen until triggered
        isFalling = false;
    }

    // Called by the trigger zone
    public void TriggerFall()
    {
        if (!isFalling)
        {
            isFalling = true;
            Invoke(nameof(DropPlatform), fallDelay);
        }
    }

    void DropPlatform()
    {
        rb.isKinematic = false;

        if (fallSound != null)
            fallSound.Play();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isFalling)
        {
            // Play hit sound
            if (hitSound != null)
                hitSound.Play();

            // Shake camera
            SmoothCameraTarget cam = Camera.main.GetComponent<SmoothCameraTarget>();
            if (cam != null)
                cam.StartCoroutine(cam.ShakeCamera(0.3f, 0.2f));

            // Disappear immediately when touching anything
            // If using pooling:
            PlatformPool.Instance.ReturnPlatform(gameObject);
            // If not pooling:
            // Destroy(gameObject);
        }
    }
}
