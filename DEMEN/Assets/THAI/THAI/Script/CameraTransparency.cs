using UnityEngine;
using System.Collections.Generic;

public class CameraTransparency : MonoBehaviour
{
    public Transform player;
    public LayerMask obstacleMask;
    public float transparentAlpha = 0.5f; // 50% visible
    public float fadeSpeed = 5f;

    private List<SpriteRenderer> fadedObstacles = new List<SpriteRenderer>();

    void LateUpdate()
    {
        // Reset previously faded obstacles
        foreach (var sr in fadedObstacles)
        {
            if (sr != null)
            {
                Color c = sr.color;
                c.a = Mathf.Lerp(c.a, 1f, Time.deltaTime * fadeSpeed); // back to opaque
                sr.color = c;
            }
        }
        fadedObstacles.Clear();

        // Cast ray from camera to player
        Vector3 origin = transform.position;
        Vector3 dir = player.position - origin;
        float dist = dir.magnitude;

        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, dir.normalized, dist, obstacleMask);

        foreach (RaycastHit2D hit in hits)
        {
            SpriteRenderer sr = hit.collider.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color c = sr.color;
                c.a = Mathf.Lerp(c.a, transparentAlpha, Time.deltaTime * fadeSpeed);
                sr.color = c;
                fadedObstacles.Add(sr);
            }
        }
    }
}
