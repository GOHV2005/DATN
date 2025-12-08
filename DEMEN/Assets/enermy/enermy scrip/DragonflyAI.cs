using UnityEngine;
using System.Collections;

public class DragonflyAI : MonoBehaviour
{
    [Header("🌍 Territory Mode")]
    public bool useCustomTerritory = true; // Bật để dùng vùng tùy chỉnh (khuyên dùng)

    // --- Mode 1: DÙNG 2 ĐIỂM A/B ---
    public Transform pointA;
    public Transform pointB;

    // --- Mode 2: DÙNG VÙNG TÙY CHỈNH ---
    public Transform territoryCenter;
    [Range(1f, 20f)] public float territoryWidth = 6f;
    [Range(1f, 10f)] public float territoryHeight = 4f;

    [Header("⚙️ Patrol & Movement")]
    [Range(0.5f, 10f)] public float patrolSpeed = 3f;
    [Range(2f, 20f)] public float diveSpeed = 10f;
    public float orbitRadius = 2.5f;
    public float divePadding = 1f;

    [Header("👁️ Detection")]
    [Range(1f, 15f)] public float detectionDistance = 8f;
    [Range(10f, 120f)] public float detectionAngle = 60f;

    [Header("🎯 Target")]
    public string playerTag = "Player";

    // --- Internal ---
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Transform player;
    private float patrolY;

    private bool canAttack = true;
    private enum State { Patrol, DiveDown, Orbit, ReturnToPatrol }
    private State currentState = State.Patrol;

    private Vector3 patrolPointA, patrolPointB;
    private bool movingToB = true;

    private float orbitLeftX, orbitRightX;
    private bool orbitMovingRight = true;

    private float minX, maxX, minY, maxY;
    private bool boundsInitialized = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        if (GetComponent<Rigidbody2D>() == null)
        {
            Debug.LogError("❌ Dragonfly cần Rigidbody2D (Kinematic)!");
            enabled = false;
            return;
        }

        InitializeTerritory();
        SetupPatrolPoints();
        currentState = State.Patrol;
    }

    void InitializeTerritory()
    {
        if (useCustomTerritory)
        {
            if (territoryCenter == null)
                territoryCenter = transform;

            Vector3 center = territoryCenter.position;
            float halfW = territoryWidth * 0.5f;
            float halfH = territoryHeight * 0.5f;

            minX = center.x - halfW;
            maxX = center.x + halfW;
            minY = center.y - halfH;
            maxY = center.y + halfH;
        }
        else
        {
            if (pointA == null || pointB == null)
            {
                Debug.LogWarning("⚠️ Missing patrol points. Creating temporary ones.");
                CreateTemporaryPatrolPoints();
            }

            float aX = pointA.position.x, bX = pointB.position.x;
            float aY = pointA.position.y, bY = pointB.position.y;

            minX = Mathf.Min(aX, bX) - divePadding;
            maxX = Mathf.Max(aX, bX) + divePadding;
            minY = Mathf.Min(aY, bY) - divePadding * 0.5f;
            maxY = Mathf.Max(aY, bY) + divePadding * 0.3f;
        }

        boundsInitialized = true;
        patrolY = (minY + maxY) * 0.6f;
    }

    void SetupPatrolPoints()
    {
        if (useCustomTerritory)
        {
            float midY = (minY + maxY) * 0.6f;
            patrolPointA = new Vector3(minX + 1f, midY, transform.position.z);
            patrolPointB = new Vector3(maxX - 1f, midY, transform.position.z);
        }
        else
        {
            patrolPointA = pointA.position;
            patrolPointB = pointB.position;
        }
    }

    void Update()
    {
        if (animator != null && !animator.GetCurrentAnimatorStateInfo(0).IsName("bay(chuonchuon)"))
            animator.Play("bay(chuonchuon)");

        if (player == null)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag(playerTag);
            if (players.Length > 0) player = players[0].transform;
        }

        if (player != null && !IsPlayerInSight())
            player = null;

        if (player == null && currentState != State.Patrol && currentState != State.ReturnToPatrol)
        {
            currentState = State.ReturnToPatrol;
            canAttack = true;
        }

        switch (currentState)
        {
            case State.Patrol:
                Patrol();
                if (player != null && IsPlayerInSight()) StartAttack();
                break;
            case State.DiveDown:
                DiveToPlayer();
                break;
            case State.Orbit:
                OrbitAroundPlayer();
                break;
            case State.ReturnToPatrol:
                ReturnToPatrolPath();
                break;
        }
    }

    void LateUpdate()
    {
        if (!boundsInitialized) return;
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        transform.position = pos;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag(playerTag) &&
            currentState == State.DiveDown &&
            canAttack &&
            player != null)
        {
            OnAttack();
            canAttack = false;
            SetupOrbit();
            currentState = State.Orbit;
        }
    }

    bool IsPlayerInSight()
    {
        if (player == null) return false;
        Vector2 toPlayer = (Vector2)player.position - (Vector2)transform.position;
        if (toPlayer.magnitude > detectionDistance) return false;

        Vector2 lookDirection;
        if (currentState == State.Patrol)
        {
            Vector3 targetPos = movingToB ? patrolPointB : patrolPointA;
            lookDirection = (Vector2)targetPos - (Vector2)transform.position; // ✅ FIX
        }
        else if (currentState == State.Orbit)
            lookDirection = Vector2.right * (orbitMovingRight ? 1 : -1);
        else
            lookDirection = toPlayer.normalized;

        return Vector2.Angle(lookDirection, toPlayer) < detectionAngle;
    }

    void Patrol()
    {
        Vector3 target = movingToB ? patrolPointB : patrolPointA;
        transform.position = Vector3.MoveTowards(transform.position, target, patrolSpeed * Time.deltaTime);
        FlipSprite(((Vector2)target - (Vector2)transform.position).x); // ✅ FIX
        if (Vector3.Distance(transform.position, target) < 0.1f)
            movingToB = !movingToB;
    }

    void StartAttack()
    {
        canAttack = true;
        currentState = State.DiveDown;
    }

    void DiveToPlayer()
    {
        if (player == null) return;
        Vector3 safePos = player.position;
        safePos.x = Mathf.Clamp(safePos.x, minX, maxX);
        safePos.y = Mathf.Clamp(safePos.y, minY, maxY);
        transform.position = Vector3.MoveTowards(transform.position, safePos, diveSpeed * Time.deltaTime);
        FlipSprite(((Vector2)safePos - (Vector2)transform.position).x); // ✅ FIX
    }

    void SetupOrbit()
    {
        if (player == null) return;
        float clampedX = Mathf.Clamp(player.position.x, minX + orbitRadius, maxX - orbitRadius);
        orbitLeftX = clampedX - orbitRadius;
        orbitRightX = clampedX + orbitRadius;
        orbitMovingRight = true;
    }

    void OrbitAroundPlayer()
    {
        if (player == null) return;
        float clampedX = Mathf.Clamp(player.position.x, minX + orbitRadius, maxX - orbitRadius);
        orbitLeftX = clampedX - orbitRadius;
        orbitRightX = clampedX + orbitRadius;

        float targetX = orbitMovingRight ? orbitRightX : orbitLeftX;
        Vector3 orbitPos = new Vector3(targetX, patrolY, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, orbitPos, patrolSpeed * Time.deltaTime);
        FlipSprite(orbitMovingRight ? 1 : -1);

        if ((orbitMovingRight && transform.position.x >= orbitRightX - 0.1f) ||
            (!orbitMovingRight && transform.position.x <= orbitLeftX + 0.1f))
        {
            orbitMovingRight = !orbitMovingRight;
        }

        StartCoroutine(DelayedDive());
    }

    IEnumerator DelayedDive()
    {
        yield return new WaitForSeconds(0.2f);
        if (player != null && currentState == State.Orbit && IsPlayerInSight())
        {
            canAttack = true;
            currentState = State.DiveDown;
        }
    }

    void ReturnToPatrolPath()
    {
        Vector3 target = movingToB ? patrolPointB : patrolPointA;
        transform.position = Vector3.MoveTowards(transform.position, target, patrolSpeed * Time.deltaTime);
        FlipSprite(((Vector2)target - (Vector2)transform.position).x); // ✅ FIX
        if (Vector3.Distance(transform.position, target) < 0.3f)
            currentState = State.Patrol;
    }

    void OnAttack()
    {
        Debug.Log("💥 Dragonfly hit the Player!");
        // player.GetComponent<Health>().TakeDamage(1);
    }

    void FlipSprite(float dirX)
    {
        if (spriteRenderer != null)
            spriteRenderer.flipX = dirX > 0;
    }

    // =============== 🎨 GIZMOS ===============
    void OnDrawGizmos()
    {
        InitializeGizmoBounds();

        Gizmos.color = new Color(0, 1, 0, 0.2f);
        Gizmos.DrawCube(
            new Vector3((minX + maxX) * 0.5f, (minY + maxY) * 0.5f, transform.position.z),
            new Vector3(maxX - minX, maxY - minY, 0)
        );
        Gizmos.DrawWireCube(
            new Vector3((minX + maxX) * 0.5f, (minY + maxY) * 0.5f, transform.position.z),
            new Vector3(maxX - minX, maxY - minY, 0)
        );

        if (Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(patrolPointA, 0.15f);
            Gizmos.DrawSphere(patrolPointB, 0.15f);
            Gizmos.DrawLine(patrolPointA, patrolPointB);
        }

        if (Application.isPlaying && currentState == State.Orbit && player != null)
        {
            Gizmos.color = new Color(1, 0, 1, 0.3f);
            Vector3 center = new Vector3(player.position.x, patrolY, transform.position.z);
            Gizmos.DrawWireCube(center, new Vector3(orbitRadius * 2, 0.2f, 0));
        }

        // Tầm nhìn
        Vector2 lookDir = Vector2.right;
        if (Application.isPlaying && player != null)
        {
            if (currentState == State.Patrol)
            {
                Vector3 targetPos = movingToB ? patrolPointB : patrolPointA;
                lookDir = (Vector2)targetPos - (Vector2)transform.position; // ✅ FIX
            }
            else
                lookDir = ((Vector2)player.position - (Vector2)transform.position).normalized;
        }

        Gizmos.color = new Color(1, 1, 0, 0.4f);
        Vector3 origin = transform.position;
        Vector3 left = Quaternion.Euler(0, 0, detectionAngle) * new Vector3(lookDir.x, lookDir.y, 0) * detectionDistance;
        Vector3 right = Quaternion.Euler(0, 0, -detectionAngle) * new Vector3(lookDir.x, lookDir.y, 0) * detectionDistance;
        Gizmos.DrawRay(origin, left);
        Gizmos.DrawRay(origin, right);
        Gizmos.DrawLine(origin + left, origin + right);
    }

    void InitializeGizmoBounds()
    {
        if (useCustomTerritory)
        {
            Transform center = territoryCenter != null ? territoryCenter : transform;
            float halfW = territoryWidth * 0.5f;
            float halfH = territoryHeight * 0.5f;
            minX = center.position.x - halfW;
            maxX = center.position.x + halfW;
            minY = center.position.y - halfH;
            maxY = center.position.y + halfH;
        }
        else
        {
            if (pointA != null && pointB != null)
            {
                float aX = pointA.position.x, bX = pointB.position.x;
                float aY = pointA.position.y, bY = pointB.position.y;
                float pad = divePadding;
                minX = Mathf.Min(aX, bX) - pad;
                maxX = Mathf.Max(aX, bX) + pad;
                minY = Mathf.Min(aY, bY) - pad * 0.5f;
                maxY = Mathf.Max(aY, bY) + pad * 0.3f;
            }
            else
            {
                minX = transform.position.x - 3f;
                maxX = transform.position.x + 3f;
                minY = transform.position.y - 1f;
                maxY = transform.position.y + 2f;
            }
        }
    }

    void CreateTemporaryPatrolPoints()
    {
        GameObject a = new GameObject("Dragonfly_PatrolA");
        GameObject b = new GameObject("Dragonfly_PatrolB");
        a.transform.position = transform.position + Vector3.left * 3f + Vector3.up * 1f;
        b.transform.position = transform.position + Vector3.right * 3f + Vector3.up * 1f;
        pointA = a.transform;
        pointB = b.transform;
    }
}