using UnityEngine;

public class SpikeTrapZone : MonoBehaviour
{
    public SpikePool spikePool;
    public Transform[] spikePositions; // assign roof positions in Inspector

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            DropSpikes();
        }
    }

    void DropSpikes()
    {
        foreach (Transform pos in spikePositions)
        {
            spikePool.GetSpike(pos.position);
        }
    }
}
