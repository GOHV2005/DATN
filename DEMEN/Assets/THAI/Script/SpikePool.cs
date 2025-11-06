using UnityEngine;
using System.Collections.Generic;

public class SpikePool : MonoBehaviour
{
    public GameObject spikePrefab;
    public int poolSize = 20;
    private List<GameObject> pool;

    void Awake()
    {
        pool = new List<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject spike = Instantiate(spikePrefab);
            spike.SetActive(false);
            pool.Add(spike);
        }
    }

    public GameObject GetSpike()
    {
        foreach (GameObject spike in pool)
        {
            if (!spike.activeInHierarchy)
            {
                return spike;
            }
        }

        // Optional: expand pool if needed
        GameObject newSpike = Instantiate(spikePrefab);
        newSpike.SetActive(false);
        pool.Add(newSpike);
        return newSpike;
    }
}
