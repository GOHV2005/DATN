using UnityEngine;
using System.Collections.Generic;

public class PlatformPool : MonoBehaviour
{
    public static PlatformPool Instance; // Singleton for easy access

    [SerializeField] private GameObject platformPrefab;
    [SerializeField] private int poolSize = 20;

    private Queue<GameObject> pool = new Queue<GameObject>();

    void Awake()
    {
        Instance = this;
        FillPool();
    }

    void FillPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(platformPrefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public GameObject GetPlatform(Vector3 position)
    {
        if (pool.Count == 0)
        {
            // Expand pool if needed
            FillPool();
        }

        GameObject obj = pool.Dequeue();
        obj.SetActive(true);
        obj.transform.position = position;

        return obj;
    }

    public void ReturnPlatform(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
