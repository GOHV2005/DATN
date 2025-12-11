using UnityEngine;

public class AxeSwingTrap : MonoBehaviour
{
    public float swingSpeed = 2f;   // speed of swing
    public float swingAngle = 60f;  // max angle in degrees

    private Quaternion startRotation;

    void Start()
    {
        startRotation = transform.localRotation;
    }

    void Update()
    {
        float angle = Mathf.Sin(Time.time * swingSpeed) * swingAngle;
        transform.localRotation = startRotation * Quaternion.Euler(0, 0, angle);
    }
}
