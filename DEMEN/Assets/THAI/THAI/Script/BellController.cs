using UnityEngine;
using System.Collections;

public class BellController : MonoBehaviour
{
    public float liftHeight = 0.5f;
    public float liftDuration = 0.3f;
    public Transform ballAnchor;
    public bool isInteractable = false;
    public System.Action<BellController> onBellClicked;
    private bool _isLifting;

    private void OnMouseDown()
    {
        if (!isInteractable || _isLifting) return;
        onBellClicked?.Invoke(this);
    }

    public Transform GetBallAnchor() => ballAnchor != null ? ballAnchor : transform;

    public void SetInteractable(bool value) => isInteractable = value;

    // 👇 Add these here
    public void HideVisuals()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;
    }

    public void ShowVisuals()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = true;
    }

    public void LiftAndReveal(System.Action onComplete = null)
    {
        if (_isLifting) return;
        StartCoroutine(LiftRoutine(onComplete));
    }

    private IEnumerator LiftRoutine(System.Action onComplete)
    {
        _isLifting = true;
        Vector3 start = transform.position;
        Vector3 end = start + Vector3.up * liftHeight;

        // Lift up
        float t = 0f;
        while (t < liftDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / liftDuration);
            transform.position = Vector3.Lerp(start, end, k);
            yield return null;
        }

        onComplete?.Invoke();

        // Lower down
        t = 0f;
        while (t < liftDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / liftDuration);
            transform.position = Vector3.Lerp(end, start, k);
            yield return null;
        }

        _isLifting = false;
    }
}
