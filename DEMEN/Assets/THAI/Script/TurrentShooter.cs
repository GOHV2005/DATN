using UnityEngine;
using System.Collections;

public class TurretShooter : MonoBehaviour
{
    public SpikePool spikePool;
    public Transform spawnPoint;
    public float fireInterval = 2f;
    public float spikeSpeed = 10f;
    public int spikesPerShot = 3;
    public float spreadAngle = 30f;

    private bool playerInRange = false;
    private Coroutine firingRoutine;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (firingRoutine == null)
                firingRoutine = StartCoroutine(FireSpikesRepeatedly());
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (firingRoutine != null)
            {
                StopCoroutine(firingRoutine);
                firingRoutine = null;
            }
        }
    }

    IEnumerator FireSpikesRepeatedly()
    {
        while (playerInRange)
        {
            FireSpikes();
            yield return new WaitForSeconds(fireInterval);
        }
    }

    void FireSpikes()
    {
        for (int i = 0; i < spikesPerShot; i++)
        {
            float angleStep = spreadAngle / (spikesPerShot - 1);
            float angle = -spreadAngle / 2 + angleStep * i;
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            GameObject spike = spikePool.GetSpike();
            spike.transform.position = spawnPoint.position;
            spike.transform.rotation = rotation;
            spike.SetActive(true);

            Rigidbody2D rb = spike.GetComponent<Rigidbody2D>();
            rb.linearVelocity = rotation * Vector2.right * spikeSpeed;
        }
    }
}
