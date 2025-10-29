using UnityEngine;
using System.Collections;

public class CicadaAI : MonoBehaviour
{
    [Header("📍 Spawn Position")]
    private Vector3 spawnPosition;
    private Quaternion idleRotation;

    [Header("🌍 Territory")]
    public Transform territoryCenter; // ✅ LÃNH THỔ LÀ 1 GAMEOBJECT
    public float territoryRadius = 4f; // Bán kính lãnh thổ

    [Header("👁️ Detection")]
    public float detectionDistance = 5f;
    [Range(10f, 90f)] public float detectionAngle = 45f;

    [Header("⚔️ Attack")]
    public float dashSpeed = 15f;
    public float returnSpeed = 6f;
    public float cooldownAfterAttack = 1.5f;

    [Header("🎯 Target")]
    public string playerTag = "Player";

    private Animator animator;
    private Transform player;
    private SpriteRenderer spriteRenderer;
    private State currentState = State.Idle;

    private enum State { Idle, Chase, Return, Cooldown }

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spawnPosition = transform.position;
        idleRotation = transform.rotation;

        if (GetComponent<Rigidbody2D>() == null)
        {
            Debug.LogError("❌ Ve sầu cần Rigidbody2D (Kinematic)!");
            enabled = false;
            return;
        }

        // Nếu chưa gán territoryCenter, dùng chính ve sầu làm tâm
        if (territoryCenter == null)
        {
            territoryCenter = transform;
            Debug.LogWarning("⚠️ territoryCenter chưa được gán. Sử dụng vị trí ve sầu làm tâm lãnh thổ.");
        }
    }

    void Update()
    {
        if (animator != null && currentState == State.Idle)
        {
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("dung(vesau)"))
                animator.Play("dung(vesau)");

            transform.rotation = idleRotation;
            spriteRenderer.flipX = false;
            spriteRenderer.flipY = false;
        }

        if (player == null)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag(playerTag);
            if (players.Length > 0) player = players[0].transform;
        }

        switch (currentState)
        {
            case State.Idle:
                if (player != null && IsPlayerInSight())
                {
                    currentState = State.Chase;
                }
                break;

            case State.Chase:
                if (player != null)
                {
                    // ✅ GIỚI HẠN TARGET TRONG LÃNH THỔ
                    Vector3 target = player.position;
                    Vector2 toCenter = (Vector2)target - (Vector2)territoryCenter.position;
                    bool playerOutsideTerritory = toCenter.magnitude > territoryRadius;
                    if (playerOutsideTerritory)
                    {
                        target = territoryCenter.position + (Vector3)toCenter.normalized * territoryRadius;
                    }

                    Vector3 direction = (target - transform.position).normalized;
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                    bool shouldFlipX = false;
                    if (angle > 90f || angle < -90f)
                    {
                        shouldFlipX = true;
                        angle -= 180f;
                    }

                    transform.rotation = Quaternion.Euler(0, 0, angle);
                    spriteRenderer.flipX = !shouldFlipX;
                    spriteRenderer.flipY = false;

                    transform.position = Vector3.MoveTowards(transform.position, target, dashSpeed * Time.deltaTime);

                    // ✅ KIỂM TRA: NẾU ĐÃ ĐẾN BIÊN VÀ PLAYER NGOÀI LÃNH THỔ → BAY VỀ
                    if (playerOutsideTerritory && Vector3.Distance(transform.position, target) < 0.1f)
                    {
                        currentState = State.Return;
                    }
                }
                else
                {
                    currentState = State.Return;
                }
                break;

            case State.Return:
                Vector3 returnDir = (spawnPosition - transform.position).normalized;
                float returnAngle = Mathf.Atan2(returnDir.y, returnDir.x) * Mathf.Rad2Deg;

                bool shouldFlipXReturn = false;
                if (returnAngle > 90f || returnAngle < -90f)
                {
                    shouldFlipXReturn = true;
                    returnAngle -= 180f;
                }

                transform.rotation = Quaternion.Euler(0, 0, returnAngle);
                spriteRenderer.flipX = !shouldFlipXReturn;
                spriteRenderer.flipY = false;

                transform.position = Vector3.MoveTowards(transform.position, spawnPosition, returnSpeed * Time.deltaTime);
                if (Vector3.Distance(transform.position, spawnPosition) < 0.1f)
                {
                    transform.rotation = idleRotation;
                    spriteRenderer.flipX = false;
                    spriteRenderer.flipY = false;
                    currentState = State.Cooldown;
                    StartCoroutine(ResetToIdle());
                }
                break;

            case State.Cooldown:
                break;
        }
    }

    // ✅ ÉP VỊ TRÍ VỀ TRONG LÃNH THỔ (tâm = territoryCenter)
    void LateUpdate()
    {
        if (territoryCenter == null) return;

        Vector2 toCenter = (Vector2)transform.position - (Vector2)territoryCenter.position;
        if (toCenter.magnitude > territoryRadius)
        {
            transform.position = territoryCenter.position + (Vector3)toCenter.normalized * territoryRadius;
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (currentState == State.Chase && col.gameObject.CompareTag(playerTag))
        {
            OnAttack();
            currentState = State.Return;
        }
    }

    bool IsPlayerInSight()
    {
        if (player == null) return false;
        Vector2 toPlayer = (Vector2)player.position - (Vector2)transform.position;
        if (toPlayer.magnitude > detectionDistance) return false;

        Vector2 lookDirection = (Vector2)(idleRotation * Vector3.up);
        float angle = Vector2.Angle(lookDirection, toPlayer);
        return angle < detectionAngle;
    }

    void OnAttack()
    {
        Debug.Log("💥 Ve sầu tấn công Player!");
    }

    IEnumerator ResetToIdle()
    {
        yield return new WaitForSeconds(cooldownAfterAttack);
        player = null;
        currentState = State.Idle;
    }

    // =============== 🎨 GIZMOS ===============
    void OnDrawGizmos()
    {
        // Vẽ lãnh thổ (tâm = territoryCenter)
        if (territoryCenter != null)
        {
            Gizmos.color = new Color(0, 1, 0, 0.2f);
            Gizmos.DrawWireSphere(territoryCenter.position, territoryRadius);
            Gizmos.DrawSphere(territoryCenter.position, 0.1f);
        }

        // Vẽ tầm nhìn
        Vector2 lookDir = Vector2.up;
        if (Application.isPlaying)
        {
            lookDir = (Vector2)(idleRotation * Vector3.up);
        }

        Gizmos.color = new Color(1, 0.5f, 0, 0.4f);
        Vector3 origin = transform.position;
        Vector3 left = Quaternion.Euler(0, 0, detectionAngle) * new Vector3(lookDir.x, lookDir.y, 0) * detectionDistance;
        Vector3 right = Quaternion.Euler(0, 0, -detectionAngle) * new Vector3(lookDir.x, lookDir.y, 0) * detectionDistance;

        Gizmos.DrawRay(origin, left);
        Gizmos.DrawRay(origin, right);
        Gizmos.DrawLine(origin + left, origin + right);
        Gizmos.DrawWireSphere(origin, detectionDistance);
    }
}