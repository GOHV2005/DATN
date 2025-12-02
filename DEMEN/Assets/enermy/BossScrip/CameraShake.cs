using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public IEnumerator DoShake(float intensity, float duration)
    {
        Vector3 originalPos = transform.localPosition;
        float time = 0;

        while (time < duration)
        {
            transform.localPosition = originalPos + Random.insideUnitSphere * intensity;
            time += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }

    public void Shake(float intensity, float duration)
    {
        StartCoroutine(DoShake(intensity, duration));
    }
}
