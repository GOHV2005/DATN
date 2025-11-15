using UnityEngine;

public class BossAI : MonoBehaviour
{
    public Transform player;
    public Transform safeZone;

    [Header("Movement")]
    public float chaseSpeed = 5f;
    public float catchUpSpeed = 8f;
    public float chaseRange = 15f;

    [Header("Skills")]
    public GameObject laserPrefab;
    public Transform laserSpawnPoint;
    public float laserCooldown = 5f;
    private float laserTimer;

    public GameObject guardPrefab;
    public Transform guardSpawnPoint;
    public float guardCooldown = 10f;
    private float guardTimer;

    [Header("Hand Slam")]
    public GameObject slamEffectPrefab;
    public float slamChance = 0.2f; // 20% chance per chase cycle

    private bool playerInSafeZone = false;

    void Update()
    {
        if (playerInSafeZone)
        {
            // Boss leaves when player reaches safe zone
            LeaveScene();
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= chaseRange)
        {
            ChasePlayer();
            UseSkills();
        }
        else
        {
            BlockPlayerPath();
            CatchUp();
        }
    }

    void ChasePlayer()
    {
        transform.position = Vector2.MoveTowards(
            transform.position,
            player.position,
            chaseSpeed * Time.deltaTime
        );

        // Random chance to slam hands
        if (Random.value < slamChance * Time.deltaTime)
        {
            SlamGround();
        }
    }

    void BlockPlayerPath()
    {
        // Example: spawn a slam effect in front of player to block
        Vector2 blockPos = player.position + Vector3.right * 2f;
        Instantiate(slamEffectPrefab, blockPos, Quaternion.identity);
    }

    void CatchUp()
    {
        transform.position = Vector2.MoveTowards(
            transform.position,
            player.position,
            catchUpSpeed * Time.deltaTime
        );
    }

    void UseSkills()
    {
        // Laser attack
        if (Time.time > laserTimer)
        {
            ShootLaser();
            laserTimer = Time.time + laserCooldown;
        }

        // Summon guards
        if (Time.time > guardTimer)
        {
            SummonGuard();
            guardTimer = Time.time + guardCooldown;
        }
    }

    void ShootLaser()
    {
        Instantiate(laserPrefab, laserSpawnPoint.position, Quaternion.identity);
        Debug.Log("Boss fires laser!");
    }

    void SummonGuard()
    {
        Instantiate(guardPrefab, guardSpawnPoint.position, Quaternion.identity);
        Debug.Log("Boss summons guard!");
    }

    void SlamGround()
    {
        Instantiate(slamEffectPrefab, player.position, Quaternion.identity);
        Debug.Log("Boss slams ground!");
    }

    void LeaveScene()
    {
        // Boss flies away or disappears
        transform.position = Vector2.MoveTowards(
            transform.position,
            safeZone.position,
            catchUpSpeed * Time.deltaTime
        );

        if (Vector2.Distance(transform.position, safeZone.position) < 1f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("SafeZone"))
        {
            playerInSafeZone = true;
        }
    }
}
