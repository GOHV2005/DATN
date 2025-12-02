// DestroyAfterTime.cs
using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    public float lifetime = 5f; // thời gian tồn tại (giây)

    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}