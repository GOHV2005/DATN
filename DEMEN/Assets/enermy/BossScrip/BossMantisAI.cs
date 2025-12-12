using System.Collections;
using UnityEngine;

public class BossMantisAI : MonoBehaviour
{
    public enum BossState { Idle, Moving, UsingSkill }

    [Header("References")]
    public Transform player;
    public Animator anim;
    public SpriteRenderer sr;
    public SpriteRenderer deadEyes; // 👈 PHẢI LÀ CON CỦA BOSS TRONG HIERARCHY
    public Collider2D arenaCollider;

    [Header("Prefabs & Effects")]
    public GameObject shockwavePrefab;

    [Header("Movement & Attack")]
    public float walkSpeed = 2f;
    public float attackSpeed = 10f;
    public float skill1MinDistanceFromPlayer = 8f;
    public float deadEyesTime = 1.5f;
    public float chargeDistance = 5f;
    public float playerNearDistance = 6f;

    [Header("DAMAGE")]
    public int skill1Damage = 100; // Skill1 = Skill2 → sát thương khi lao
    public int skill2Damage = 100;
    public int skill3Damage = 80;

    private BossState currentState = BossState.Idle;
    private bool combatStarted = false;
    private bool isAttackActive = false;
    private int currentAttackDamage = 0;

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f);
        if (deadEyes != null) deadEyes.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!combatStarted && player != null && arenaCollider != null)
        {
            if (arenaCollider.bounds.Contains(player.position))
            {
                combatStarted = true;
                StartCoroutine(StartCombat());
            }
        }
        // Cập nhật vị trí DeadEyes theo hướng nhìn
        if (deadEyes != null)
        {
            Vector3 localPos = deadEyes.transform.localPosition;
            localPos.x = sr.flipX ? 0.79f : -0.79f; // 👈 đảo X khi flip
            deadEyes.transform.localPosition = localPos;
        }
        if (currentState == BossState.Moving)
        {
            WalkTowardsPlayer(walkSpeed);
        }

        // Cập nhật hướng DeadEyes theo boss (nếu đang hiện)
        if (deadEyes != null && deadEyes.gameObject.activeSelf)
        {
            deadEyes.flipX = sr.flipX;
        }
    }

    IEnumerator StartCombat()
    {
        currentState = BossState.UsingSkill;
        anim.Play("TanCongManh(Bongua)");
        yield return new WaitForSeconds(2f);
        anim.Play("Dung(BoNgua)");
        currentState = BossState.Moving;
        StartCoroutine(SkillLoop());
    }

    void WalkTowardsPlayer(float speed)
    {
        if (player == null) return;
        Vector3 dir = (player.position - transform.position).normalized;
        transform.position += new Vector3(dir.x * speed * Time.deltaTime, 0, 0);
        sr.flipX = dir.x > 0;
        anim.Play("DiBo(BoNgua)");
    }

    IEnumerator SkillLoop()
    {
        while (true)
        {
            currentState = BossState.UsingSkill;
            float r = Random.Range(0f, 1f);

            if (r <= 0.2f) // 20% Skill1
                yield return StartCoroutine(Skill1());
            else if (r <= 0.6f) // 40% Skill3
                yield return StartCoroutine(Skill3());
            else // 40% Skill2
                yield return StartCoroutine(Skill2CheckNear());

            currentState = BossState.Moving;
            yield return new WaitForSeconds(1f);
        }
    }

    // ================= SKILL 1: chỉ cảnh báo → skill2 =================
    IEnumerator Skill1()
    {
        // Chỉ hiện DeadEyes → rồi dùng chung logic tấn công như Skill 2
        if (deadEyes != null)
        {
            deadEyes.gameObject.SetActive(true);
            yield return new WaitForSeconds(deadEyesTime);
            deadEyes.gameObject.SetActive(false);
        }

        yield return StartCoroutine(PerformAttack(skill1Damage));
    }

    // ================= SKILL 2: chỉ khi player GẦN =================
    IEnumerator Skill2CheckNear()
    {
        if (player == null) yield break;
        float distance = Vector2.Distance(player.position, transform.position);
        if (distance > playerNearDistance) yield break;

        yield return StartCoroutine(PerformAttack(skill2Damage));
    }
    IEnumerator PerformAttack(int damage)
    {
        currentAttackDamage = damage;
        isAttackActive = true;
        anim.Play("TanCong(BoNgua)");
        yield return StartCoroutine(AttackForward(attackSpeed, chargeDistance));
        anim.Play("Dung(BoNgua)");
        isAttackActive = false;
    }
    // ================= SKILL 3: chỉ khi player XA =================
    IEnumerator Skill3()
    {
        if (player == null) yield break;
        float distance = Vector2.Distance(player.position, transform.position);
        if (distance <= playerNearDistance) yield break;

        anim.Play("TanCongManh(Bongua)");
        yield return new WaitForSeconds(0.8f); // đợi animation xong

        // Spawn shockwave
        if (shockwavePrefab != null)
        {
            GameObject wave = Instantiate(shockwavePrefab, transform.position, Quaternion.identity);
            ShockwaveProjectile proj = wave.GetComponent<ShockwaveProjectile>();
            if (proj != null)
            {
                float dirX = (player.position.x > transform.position.x) ? 1f : -1f;
                proj.Initialize(dirX, skill3Damage);
            }
        }

        // Lao (có thể bỏ nếu không muốn)
        currentAttackDamage = skill3Damage;
        isAttackActive = true;
        anim.Play("TanCong(BoNgua)");
        yield return StartCoroutine(AttackForward(attackSpeed, chargeDistance));
        anim.Play("Dung(BoNgua)");
        isAttackActive = false;
    }

    IEnumerator AttackForward(float speed, float distance)
    {
        Vector3 start = transform.position;
        Vector3 dir = (player.position.x > transform.position.x) ? Vector3.right : Vector3.left;
        sr.flipX = dir.x > 0;

        float traveled = 0f;
        while (traveled < distance)
        {
            // (Tùy chọn) Kiểm tra vật cản — nếu cần
            // RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, speed * Time.deltaTime, ...);
            // if (hit) break;

            float step = speed * Time.deltaTime;
            transform.position += dir * step;
            traveled += step;
            yield return null;
        }
    }

    // ================= DAMAGE TRIGGER =================
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isAttackActive)
        {
            PlayerController pc = other.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.TakeDamageFromEnemy(currentAttackDamage, transform.position);
            }
        }
    }


    // ================= GIZMOS =================
    private void OnDrawGizmosSelected()
    {
        if (arenaCollider != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(arenaCollider.bounds.center, arenaCollider.bounds.size);
        }
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, playerNearDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chargeDistance);
    }
}