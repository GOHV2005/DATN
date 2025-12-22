using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using Unity.Cinemachine;

public class BossMantisAI : MonoBehaviour
{
    public enum BossState { Idle, Moving, UsingSkill, Dead }
    BossState currentState = BossState.Idle;
    public bool skipIntro = false; // nếu true → bỏ qua dialogue + cinematic

    [Header("REFERENCES")]
    public Transform player;
    public Animator anim;
    public SpriteRenderer sr;
    public SpriteRenderer deadEyes;
    public Collider2D arenaCollider;
    public GameObject arenaBarrier;
    public ShockWavesManager shockWavesManager;

    [Header("CINEMATIC")]
    public CinemachineCamera introCam;      // Camera quay intro
    public CinemachineCamera gameplayCam;   // Camera gameplay

    [Header("DIALOGUE")]
    public Dialogue bossIntroDialogue;

    [Header("BOSS HP")]
    public int maxHP = 1000;
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

    // Runtime
    bool combatStarted;
    bool isAttackActive;
    int currentDamage;

    private string introPrefKey;
    private bool introPlayed = false;
    private bool introEnded = false;
    private Health bossHealth;
    void Awake()
    {
        introPrefKey = $"BossMantisIntroPlayed_{gameObject.name}_{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}";
    }

    void Start()
    {
        bossHealth = GetComponent<Health>();


        if (!player)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        deadEyes.gameObject.SetActive(false);

        sfxSource = gameObject.GetOrAddComponent<AudioSource>();
        sfxSource.outputAudioMixerGroup = sfxMixer;
        sfxSource.spatialBlend = 0f;

        musicSource = gameObject.GetOrAddComponent<AudioSource>();
        musicSource.outputAudioMixerGroup = musicMixer;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
    }

    void OnDestroy()
    {
        if (DialogueSystem.Instance != null)
            DialogueSystem.Instance.OnDialogueComplete -= OnIntroDialogueComplete;
    }

#if UNITY_EDITOR
    void OnApplicationQuit()
    {
        if (UnityEditor.EditorApplication.isPlaying)
        {
            PlayerPrefs.DeleteKey(introPrefKey);
        }
    }
#endif

    [ContextMenu("Reset Intro for Testing")]
    public void ResetIntroForTesting()
    {
        PlayerPrefs.DeleteKey(introPrefKey);
        Debug.Log($"✅ Đã reset intro cho Boss Mantis '{gameObject.name}'");
    }

    // =============== PUBLIC ENTRY POINT ===============
    public void StartIntroSequence()
    {
        if (combatStarted || isDead || bossHealth?.currentHealth <= 0 || introPlayed) return;
        introPlayed = true;
        BossUIManager.Instance?.Show(bossHealth);
        // ✅ NẾU LÀ SPAWNED → BỎ QUA CINEMATIC, VÀO COMBAT NGAY
        if (skipIntro)
        {
            StartCoroutine(StartCombatImmediately());
            return;
        }

        // BẬT CAMERA INTRO NGAY LÚC VÀO
        ActivateIntroCam(true);

        // HIỂN THỊ DIALOGUE NẾU CHƯA XEM
        if (bossIntroDialogue != null && !PlayerPrefs.HasKey(introPrefKey))
        {
            UIManager.IsTalkingToNPC = true;
            player?.GetComponent<PlayerController>()?.SetDialogueState(true);

            DialogueSystem.Instance.OnDialogueComplete += OnIntroDialogueComplete;
            DialogueSystem.Instance.StartDialogue(bossIntroDialogue);
        }
        else
        {
            StartCoroutine(RunBossIntro());
        }
    }

    IEnumerator StartCombatImmediately()
    {
        // Bật barrier, nhạc, UI — nhưng KHÔNG có cinematic
        if (arenaBarrier) arenaBarrier.SetActive(true);
        PlayMusic();

        combatStarted = true;

        currentState = BossState.Moving;
        StartCoroutine(CombatLoop());
        yield break;
    }
    void OnIntroDialogueComplete()
    {
        DialogueSystem.Instance.OnDialogueComplete -= OnIntroDialogueComplete;
        PlayerPrefs.SetInt(introPrefKey, 1);
        PlayerPrefs.Save();

        UIManager.IsTalkingToNPC = false;
        player?.GetComponent<PlayerController>()?.SetDialogueState(false);

        StartCoroutine(RunBossIntro());
    }

    // =============== CHẠY INTRO BOSS (3 LẦN ĐÁNH) ===============
    IEnumerator RunBossIntro()
    {
        // CHẠY NHẠC ĐẤU TRƯỜNG
        PlayMusic();

        // MỞ BARRIER
        if (arenaBarrier) arenaBarrier.SetActive(true);

        // BOSS IDLE
        currentState = BossState.Idle;
        anim.Play("Dung(BoNgua)"); // hoặc idleAnim nếu có

        // 💥 PLAY 3 LẦN ĐÁNH
        for (int i = 0; i < 3; i++)
        {
            anim.Play("TanCongManh(Bongua)");
            sfxSource.PlayOneShot(strongAttackClip);
            yield return new WaitForSeconds(0.8f);
        }

        // ✅ HẾT INTRO → CHUYỂN CAMERA VỀ PLAYER, BẮT ĐẦU COMBAT
        yield return new WaitForSeconds(0.2f);
        ActivateIntroCam(false); // chuyển về gameplayCam

        combatStarted = true;

        currentState = BossState.Moving;
        StartCoroutine(CombatLoop());
    }

    void ActivateIntroCam(bool active)
    {
        if (introCam != null) introCam.Priority = active ? 50 : 0;
        if (gameplayCam != null) gameplayCam.Priority = active ? 0 : 50;
    }

    // =============== COMBAT ===============
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

    void Update()
    {
        if (bossHealth == null || bossHealth.currentHealth <= 0) return;

        if (currentState == BossState.Moving)
            MoveToPlayer();
    }

    void MoveToPlayer()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        transform.position += new Vector3(dir.x * walkSpeed * Time.deltaTime, 0, 0);
        sr.flipX = dir.x > 0;
        anim.Play("DiBo(BoNgua)");
    }

    public void AnimEvent_RamShockwave()
    {
        if (shockWavesManager != null)
            shockWavesManager.CallShockWaves();
    }

    IEnumerator SkillDash()
    {
        anim.Play("TanCong(BoNgua)");
        sfxSource.PlayOneShot(attackClip);
        yield return DashForward(dashSpeed, dashDistance, skill1Damage);
    }

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

    IEnumerator SkillTeleport()
    {
        anim.Play("TanCongManh(Bongua)");
        sfxSource.PlayOneShot(strongAttackClip);
        SpawnSmoke();
        sfxSource.PlayOneShot(teleportClip);
        yield return new WaitForSeconds(0.3f);

        sr.enabled = false;
        deadEyes.gameObject.SetActive(false);
        anim.Play("DiBo(BoNgua)");

        float x = Random.Range(arenaCollider.bounds.min.x + 1f, arenaCollider.bounds.max.x - 1f);
        transform.position = new Vector3(x, transform.position.y, 0);

        while (Vector2.Distance(transform.position, player.position) > warningDistance)
        {
            Vector3 dir = (player.position - transform.position).normalized;
            transform.position += new Vector3(dir.x * approachSpeed * Time.deltaTime, 0, 0);
            yield return null;
        }

        deadEyes.gameObject.SetActive(true);
        yield return new WaitForSeconds(warningTime);
        deadEyes.gameObject.SetActive(false);

        sr.enabled = true;
        anim.Play("TanCongManh(Bongua)");
        yield return DashForward(dashSpeed * 1.3f, dashDistance * 1.3f, skill3Damage);
    }

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

    void Die()
    {
            BossUIManager.Instance?.Hide();

        isDead = true;
        currentState = BossState.Dead;
        StopAllCoroutines();

        if (arenaBarrier) arenaBarrier.SetActive(false);
        if (musicSource.isPlaying) musicSource.Stop();

        deadEyes.gameObject.SetActive(false);
        anim.Play("Chet(BoNgua)");
        Debug.Log("💀 Boss Mantis chết – Arena mở");
    }

    void SpawnSmoke()
    {
        if (smokePrefab)
            Instantiate(smokePrefab, transform.position, Quaternion.identity);
    }

    void PlayMusic()
    {
        if (arenaBgm != null && !musicSource.isPlaying)
        {
            musicSource.clip = arenaBgm;
            musicSource.Play();
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (isAttackActive && col.CompareTag("Player"))
        {
            col.GetComponent<PlayerController>()?.TakeDamageFromEnemy(currentDamage, transform.position);
        }
    }

    void FindBossSlider()
    {
        GameObject sliderObj = GameObject.FindGameObjectWithTag("BossSlider");
        if (sliderObj == null)
            sliderObj = GameObject.Find("BossHealthSlider");

        if (sliderObj != null)
        {
            //bossHealthSlider = sliderObj.GetComponent<UnityEngine.UI.Slider>();
            // ✅ KHÔNG ẨN Ở ĐÂY NỮA
        }
        else
        {
            Debug.LogWarning($"⚠️ {GetType().Name}: Không tìm thấy BossHealthSlider (tag 'BossSlider' hoặc tên 'BossHealthSlider')");
        }
    }

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

// Helper extension
public static class ComponentExtensions
{
    public static T GetOrAddComponent<T>(this GameObject go) where T : Component
    {
        T component = go.GetComponent<T>();
        if (component == null)
            component = go.AddComponent<T>();
        return component;
    }
}