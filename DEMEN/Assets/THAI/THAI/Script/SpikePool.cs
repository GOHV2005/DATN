using UnityEngine;
using System.Collections.Generic;

public class SpikePool : MonoBehaviour
{
    public GameObject spikePrefab;
    public int poolSize = 20;
    private Queue<GameObject> spikePool = new Queue<GameObject>();

    void Start()
    {
        for (int i = poolSize - 1; i >= 0; i--)
        {
            GameObject spike = Instantiate(spikePrefab);
            spike.SetActive(false);
            spikePool.Enqueue(spike);
        }
    }

    public GameObject GetSpike(Vector3 position)
    {
        if (spikePool.Count > 0)
        {
            GameObject spike = spikePool.Dequeue();
            spike.transform.position = position;
            spike.SetActive(true);

            Rigidbody rb = spike.GetComponent<Rigidbody>();
            rb.linearVelocity = Vector3.zero;        // reset movement
            rb.angularVelocity = Vector3.zero; // reset rotation
            rb.useGravity = true;              // enable falling

            return spike;
        }
        return null;
    }

    public void ReturnSpike(GameObject spike)
    {
        Rigidbody rb = spike.GetComponent<Rigidbody>();
        rb.useGravity = false;                // turn off gravity
        rb.linearVelocity = Vector3.zero;           // clear movement
        rb.angularVelocity = Vector3.zero;    // clear rotation

        spike.SetActive(false);
        spikePool.Enqueue(spike);
    }
}
