using UnityEngine;

public class Spike : MonoBehaviour
{
    private SpikePool pool;

    public void SetPool(SpikePool spikePool)
    {
        pool = spikePool;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            pool.ReturnSpike(gameObject);
        }
    }
}
