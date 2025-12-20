using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class BossMantisAI : MonoBehaviour
{
    public enum BossState { Idle, Moving, UsingSkill, Dead }
    BossState currentState = BossState.Idle;
    [Header("UI")]
    private UnityEngine.UI.Slider bossHealthSlider; // kéo slider từ Inspector
    private Health bossHealth;                     // tham chiếu Health component


    [Header("REFERENCES")]
    public Transform player;
    public Animator anim;
    public SpriteRenderer sr;
    public SpriteRenderer deadEyes;
    public Collider2D arenaCollider;
    public GameObject arenaBarrier;
    public ShockWavesManager shockWavesManager;

    [Header("BOSS HP")]
    public int maxHP = 1000;
    private int currentHP;
    private bool isDead = false;

    [Header("PREFABS")]
    public GameObject shockwavePrefab;
    public GameObject smokePrefab;

    [Header("MOVE")]
    public float walkSpeed = 2f;
    public float dashSpeed = 12f;
    public float dashDistance = 6f;
    public float approachSpeed = 3f;
    public float warningDistance = 3f;

    [Header("SKILL 3")]
    public float warningTime = 0.8f;

    [Header("DAMAGE")]
    public int skill1Damage = 100;
    public int skill2Damage = 100;
    public int skill3Damage = 150;

    [Header("AUDIO")]
    public AudioClip attackClip;
    public AudioClip strongAttackClip;
    public AudioClip teleportClip;
    public AudioClip arenaBgm;

    public AudioMixerGroup sfxMixer;
    public AudioMixerGroup musicMixer;

    AudioSource sfxSource;
    AudioSource musicSource;

    bool combatStarted;
    bool isAttackActive;
    int currentDamage;

    // ================= START =================
    void Start()
    {
        // Lấy Health
        bossHealth = GetComponent<Health>();
        FindBossSlider();
        if (bossHealthSlider != null && bossHealth != null)
        {
            bossHealthSlider.gameObject.SetActive(false); // ẩn lúc đầu
            bossHealthSlider.maxValue = bossHealth.maxHealth;
            bossHealthSlider.value = bossHealth.currentHealth;

            // gán sự kiện khi boss chết
            bossHealth.onDeath += Die;
        }

        if (!player)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        deadEyes.gameObject.SetActive(false);

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.outputAudioMixerGroup = sfxMixer;
        sfxSource.spatialBlend = 0f;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.outputAudioMixerGroup = musicMixer;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
    }

    void FindBossSlider()
    {
        // Ưu tiên tìm theo TAG
        GameObject sliderObj = GameObject.FindGameObjectWithTag("BossSlider");

        // Dự phòng: tìm theo tên
        if (sliderObj == null)
            sliderObj = GameObject.Find("BossHealthSlider");

        if (sliderObj != null)
        {
            bossHealthSlider = sliderObj.GetComponent<UnityEngine.UI.Slider>();
            bossHealthSlider.gameObject.SetActive(false); // ẩn lúc đầu
        }
        else
        {
            Debug.LogWarning("⚠️ BossMantis: Không tìm thấy BossSlider");
        }
    }

    public void StartCombat()
    {
        if (combatStarted || bossHealth == null || bossHealth.currentHealth <= 0) return;

        combatStarted = true;

        // hiện thanh máu boss
        if (bossHealthSlider != null)
            bossHealthSlider.gameObject.SetActive(true);

        StartCoroutine(CombatIntro());
    }



    void Update()
    {
        if (bossHealth == null || bossHealth.currentHealth <= 0) return;

        // cập nhật slider
        if (bossHealthSlider != null)
            bossHealthSlider.value = bossHealth.currentHealth;

        if (currentState == BossState.Moving)
            MoveToPlayer();
    }


    // ================= INTRO =================
    IEnumerator CombatIntro()
    {
        // 🔒 KHÓA ARENA
        if (arenaBarrier)
            arenaBarrier.SetActive(true);

        PlayMusic();

        currentState = BossState.Idle;

        for (int i = 0; i < 3; i++)
        {
            anim.Play("TanCongManh(Bongua)");
            sfxSource.PlayOneShot(strongAttackClip);
            yield return new WaitForSeconds(0.8f);
        }

        currentState = BossState.Moving;

        StartCoroutine(CombatLoop());
    }
    public void AnimEvent_RamShockwave()
    {
        if (shockWavesManager != null)
            shockWavesManager.CallShockWaves();
    }
    // ================= COMBAT LOOP =================
    IEnumerator CombatLoop()
    {
        while (!isDead)
        {
            currentState = BossState.UsingSkill;

            float r = Random.value;
            if (r < 0.33f)
                yield return SkillDash();
            else if (r < 0.66f)
                yield return SkillShockwave();
            else
                yield return SkillTeleport();

            currentState = BossState.Moving;
            yield return new WaitForSeconds(1.2f);
        }
    }

    // ================= MOVE =================
    void MoveToPlayer()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        transform.position += new Vector3(dir.x * walkSpeed * Time.deltaTime, 0, 0);
        sr.flipX = dir.x > 0;
        anim.Play("DiBo(BoNgua)");
    }

    // ================= SKILL 1 =================
    IEnumerator SkillDash()
    {
        anim.Play("TanCong(BoNgua)");
        sfxSource.PlayOneShot(attackClip);
        yield return DashForward(dashSpeed, dashDistance, skill1Damage);
    }

    // ================= SKILL 2 =================
    IEnumerator SkillShockwave()
    {
        anim.Play("TanCongManh(Bongua)");
        sfxSource.PlayOneShot(strongAttackClip);
        yield return new WaitForSeconds(0.6f);

        float dir = player.position.x > transform.position.x ? 1 : -1;
        Instantiate(shockwavePrefab, transform.position, Quaternion.identity)
            .GetComponent<ShockwaveProjectile>()
            ?.Initialize(dir, skill2Damage);
    }

    // ================= SKILL 3 (STEALTH) =================
    IEnumerator SkillTeleport()
    {
        anim.Play("TanCongManh(Bongua)");
        sfxSource.PlayOneShot(strongAttackClip);

        SpawnSmoke();
        sfxSource.PlayOneShot(teleportClip);

        yield return new WaitForSeconds(0.3f);

        // 👻 TÀN HÌNH
        sr.enabled = false;
        deadEyes.gameObject.SetActive(false);

        // 🚶‍♂️ CHUYỂN SANG ANIMATION KHÔNG EVENT
        anim.Play("DiBo(BoNgua)");

        // 📍 Random vị trí
        float x = Random.Range(
            arenaCollider.bounds.min.x + 1f,
            arenaCollider.bounds.max.x - 1f
        );
        transform.position = new Vector3(x, transform.position.y, 0);

        // ➡️ TIẾN GẦN PLAYER (KHÔNG EVENT)
        while (Vector2.Distance(transform.position, player.position) > warningDistance)
        {
            Vector3 dir = (player.position - transform.position).normalized;
            transform.position += new Vector3(dir.x * approachSpeed * Time.deltaTime, 0, 0);
            yield return null;
        }

        // ⚠️ CẢNH BÁO
        deadEyes.gameObject.SetActive(true);
        yield return new WaitForSeconds(warningTime);
        deadEyes.gameObject.SetActive(false);

        // 💥 HIỆN HÌNH + ĐÒN THẬT (CÓ EVENT)
        sr.enabled = true;
        anim.Play("TanCongManh(Bongua)");

        yield return DashForward(
            dashSpeed * 1.3f,
            dashDistance * 1.3f,
            skill3Damage
        );
    }


    // ================= DASH CORE =================
    IEnumerator DashForward(float speed, float distance, int dmg)
    {
        isAttackActive = true;
        currentDamage = dmg;

        Vector3 dir = (player.position.x > transform.position.x) ? Vector3.right : Vector3.left;
        sr.flipX = dir.x > 0;

        float moved = 0f;
        while (moved < distance)
        {
            float step = speed * Time.deltaTime;
            transform.position += dir * step;
            moved += step;
            yield return null;
        }

        isAttackActive = false;
    }

    // ================= DAMAGE & DEATH =================
    public void TakeDamage(int dmg)
    {
        if (isDead) return;

        currentHP -= dmg;
        currentHP = Mathf.Max(currentHP, 0);

        // cập nhật slider
        if (bossHealthSlider != null)
            bossHealthSlider.value = currentHP;

        if (currentHP <= 0)
            Die();
    }


    void Die()
    {
        if (bossHealthSlider != null)
            bossHealthSlider.gameObject.SetActive(false);

        isDead = true;
        currentState = BossState.Dead;

        StopAllCoroutines();

        if (arenaBarrier)
            arenaBarrier.SetActive(false);

        if (musicSource.isPlaying)
            musicSource.Stop();

        deadEyes.gameObject.SetActive(false);
        anim.Play("Chet(BoNgua)");

        Debug.Log("💀 Boss chết – Arena mở");
    }


    // ================= UTIL =================
    void SpawnSmoke()
    {
        if (smokePrefab)
            Instantiate(smokePrefab, transform.position, Quaternion.identity);
    }

    void PlayMusic()
    {
        if (!musicSource.isPlaying && arenaBgm)
        {
            musicSource.clip = arenaBgm;
            musicSource.Play();
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (isAttackActive && col.CompareTag("Player"))
        {
            col.GetComponent<PlayerController>()
               ?.TakeDamageFromEnemy(currentDamage, transform.position);
        }
    }

    // ================= GIZMOS =================
    void OnDrawGizmosSelected()
    {
        if (arenaCollider)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(arenaCollider.bounds.center, arenaCollider.bounds.size);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, warningDistance);
    }
}
