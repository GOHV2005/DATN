using UnityEngine;

public class SpikeReset : MonoBehaviour
{
    public float lifetime = 3f;

    void OnEnable()
    {
        Invoke(nameof(DisableSpike), lifetime);
    }

    void DisableSpike()
    {
        gameObject.SetActive(false);
    }
}
