using Unity.Cinemachine;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class BossBeetleAI : MonoBehaviour
{
    public enum BossState
    {
        Idle,
        Intro,
        Roar,
        Turn,
        Charge,
        Stunned,
        Stomp,
        Cooldown
    }

    private BossState currentState = BossState.Idle;
    public bool skipIntro = false; // nếu true → bỏ qua dialogue + cinematic
    [Header("ARENA DOOR")]
    public Animator doorAnimator;
    public Collider2D doorCollider;

    [Header("DOOR EFFECTS")]
    public GameObject dustEffectPrefab;
    public Transform dustSpawnPoint;

    [Header("ARENA")]
    public Collider2D arenaTrigger;
    public Transform player;

    [Header("SHOCKWAVE")]
    public ShockWavesManager shockWavesManager;

    [Header("CHARGE")]
    public float chargeSpeed = 8f;
    public float obstacleCheckDistance = 1f;
    public LayerMask obstacleLayer;

    [Header("STOMP SKILL")]
    public GameObject fallingRockPrefab;
    public int rockCount = 5;
    public float rockDelay = 0.25f;
    [Tooltip("Chiều cao rơi đá tính từ TRẦN của arena (arenaTrigger.bounds.max.y)")]
    public float rockSpawnHeight = 7f;

    [Header("SKILL PROBABILITIES")]
    [Range(0f, 1f)] public float chargeChance = 0.6f;

    [Header("TIME SETTINGS")]
    public float stunnedTime = 1.5f;
    public float cooldownTime = 1f;
    public float roarTime = 2.5f;

    [Header("CAMERA SHAKE")]
    public CameraShake cameraShake;
    public float shakeIntensity = 0.45f;

    [Header("ANIMATION")]
    public string idleAnim = "Dung(bohung)";
    public string ramAnim = "ram(bohung)";
    public string chargeAnim = "chaynhanh(bohung)";

    // =============== SOUND ===============
    [Header("SOUND EFFECTS")]
    public AudioClip roarSound;
    [Range(0f, 1f)] public float roarVolume = 1f;
    public AudioClip stunImpactSound;
    [Range(0f, 1f)] public float stunVolume = 1f;
    public AudioClip rockDropSound;
    [Range(0f, 1f)] public float rockDropVolume = 1f;

    [Header("FOOTSTEP SOUNDS")]
    public AudioClip[] footstepClips;
    [Range(0f, 1f)] public float footstepVolume = 0.7f;
    public float footstepInterval = 0.4f;

    [Header("BOSS MUSIC")]
    public AudioClip bossMusic;
    [Range(0f, 1f)] public float musicVolume = 0.8f;
    private AudioSource musicAudioSource;

    [Header("AUDIO MIXER")]
    public AudioMixerGroup sfxMixerGroup;
    public AudioMixerGroup musicMixerGroup;

    [Header("ARENA BARRIER")]
    public GameObject arenaBarrier;

    [Header("UI")]
    private Slider bossHealthSlider;
    private Health bossHealth;

    [Header("CINEMATIC")]
    public CinemachineCamera introCam;
    public CinemachineCamera gameplayCam;
    public float fallbackIntroDuration = 2.0f;

    [Header("INTRO TIMING")]
    public float introMinDuration = 4f;

    [Header("Dialogue")]
    public Dialogue bossIntroDialogue;

    // runtime
    private bool introPlayed = false;
    private string introPrefKey;

    private bool sliderActive = false;
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private Vector2 chargeDirection;
    private float stateTimer;
    private bool playerEnteredArena = false;
    private AudioSource audioSource;
    private bool isStunned = false;
    private bool stunnedFlipX = false;
    private bool introEnded = false;
    private bool introRamPlayed = false;

    private float lastFootstepTime = 0f;
    private int nextFootstepIndex = 0;

    void Awake()
    {
        introPrefKey = $"BossIntroPlayed_{gameObject.name}_{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}";
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        if (!player)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // AUDIO SFX
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        if (sfxMixerGroup != null) audioSource.outputAudioMixerGroup = sfxMixerGroup;

        // BOSS MUSIC
        musicAudioSource = gameObject.AddComponent<AudioSource>();
        musicAudioSource.playOnAwake = false;
        musicAudioSource.loop = true;
        musicAudioSource.spatialBlend = 0f;
        if (musicMixerGroup != null) musicAudioSource.outputAudioMixerGroup = musicMixerGroup;

        FindBossSlider();

        bossHealth = GetComponent<Health>();
        if (bossHealth != null && bossHealthSlider != null)
        {
            bossHealthSlider.maxValue = bossHealth.maxHealth;
            bossHealthSlider.value = bossHealth.currentHealth;
            bossHealth.onDeath += OnBossDeath;

            // ✅ Chỉ ẩn nếu là boss CHÍNH (không phải spawn)
            if (!skipIntro)
            {
                bossHealthSlider.gameObject.SetActive(false);
            }
            // Nếu là spawn (skipIntro = true), giữ nguyên trạng thái active trong scene
        }

        PlayAnim(idleAnim);
    }

    void OnDestroy()
    {
        if (DialogueSystem.Instance != null)
            DialogueSystem.Instance.OnDialogueComplete -= OnIntroDialogueComplete;
    }

#if UNITY_EDITOR
    // ✅ RESET KHI THOÁT CHẾ ĐỘ PLAY (chỉ trong Editor)
    void OnApplicationQuit()
    {
        // Khi bạn stop play mode trong Editor → reset PlayerPrefs để test lại dễ dàng
        if (UnityEditor.EditorApplication.isPlaying)
        {
            PlayerPrefs.DeleteKey(introPrefKey);
        }
    }
#endif

    // ✅ MENU CLICK CHUỘT PHẢI TRONG INSPECTOR
    [ContextMenu("Reset Intro for Testing")]
    public void ResetIntroForTesting()
    {
        PlayerPrefs.DeleteKey(introPrefKey);
        Debug.Log($"✅ Đã reset intro cho boss '{gameObject.name}' trong scene hiện tại.");
    }

    // ✅ CÔNG KHAI: gọi từ UI button, test script, v.v.
    public void ForceResetIntro()
    {
        PlayerPrefs.DeleteKey(introPrefKey);
        introPlayed = false;
        introEnded = false;
        currentState = BossState.Idle;
        if (anim != null) PlayAnim(idleAnim);
        Debug.Log("🔄 Boss intro đã được reset hoàn toàn.");
    }

    // ---------------- Intro / camera / dialogue ----------------
    public void StartIntroSequence()
    {
        if (introPlayed) return;
        introPlayed = true;

        // ✅ NẾU LÀ SPAWNED → BỎ QUA DIALOGUE + CINEMATIC
        if (skipIntro)
        {
            StartCombat();
            // ✅ BẮT ĐẦU TỪ COOLDOWN → CHỌN KỸ NĂNG NGẪU NHIÊN NGAY
            currentState = BossState.Cooldown;
            stateTimer = 0f; // chọn kỹ năng ở frame tiếp theo
            return;
        }

        ActivateIntroCam(true);

        if (bossMusic != null && musicAudioSource != null)
        {
            musicAudioSource.clip = bossMusic;
            musicAudioSource.volume = musicVolume;
            musicAudioSource.Play();
        }

        if (bossIntroDialogue != null && !PlayerPrefs.HasKey(introPrefKey))
        {
            UIManager.IsTalkingToNPC = true;
            player?.GetComponent<PlayerController>()?.SetDialogueState(true);

            DialogueSystem.Instance.OnDialogueComplete += OnIntroDialogueComplete;
            DialogueSystem.Instance.StartDialogue(bossIntroDialogue);
        }
        else
        {
            StartIntroAnimation();
        }
    }

    void OnIntroDialogueComplete()
    {
        DialogueSystem.Instance.OnDialogueComplete -= OnIntroDialogueComplete;
        PlayerPrefs.SetInt(introPrefKey, 1);
        PlayerPrefs.Save();

        UIManager.IsTalkingToNPC = false;
        player?.GetComponent<PlayerController>()?.SetDialogueState(false);

        StartIntroAnimation();
    }

    void StartIntroAnimation()
    {
        currentState = BossState.Intro;
        stateTimer = introMinDuration;
        introRamPlayed = false;
    }

    IEnumerator EndIntroAndStartCombat()
    {
        if (introEnded) yield break;
        introEnded = true;

        yield return new WaitForSeconds(0.2f);

        ActivateIntroCam(false);
        UIManager.IsTalkingToNPC = false;
        player?.GetComponent<PlayerController>()?.SetDialogueState(false);
        FacePlayerImmediately();
        StartCombat();

        currentState = BossState.Roar;
        stateTimer = roarTime;
    }

    void FacePlayerImmediately()
    {
        if (!player || sr == null) return;
        sr.flipX = player.position.x > transform.position.x;
    }

    void ActivateIntroCam(bool active)
    {
        if (introCam == null || gameplayCam == null) return;
        introCam.Priority = active ? 50 : 0;
        gameplayCam.Priority = active ? 0 : 50;
    }

    public void StartCombat()
    {
        if (playerEnteredArena) return;
        playerEnteredArena = true;

        if (doorAnimator != null)
            doorAnimator.SetTrigger("Close");
        if (doorCollider != null)
            doorCollider.enabled = true;
        if (bossHealthSlider != null)
            bossHealthSlider.gameObject.SetActive(true);
        if (bossMusic != null && !musicAudioSource.isPlaying)
        {
            musicAudioSource.clip = bossMusic;
            musicAudioSource.volume = musicVolume;
            musicAudioSource.Play();
        }
    }

    void Update()
    {
        if (!player || !arenaTrigger) return;

        bool isInArena = arenaTrigger.bounds.Contains(player.position);

        if (isInArena && !playerEnteredArena)
        {
            // Để trống nếu bạn bắt buộc phải qua intro
        }

        if (!playerEnteredArena && currentState != BossState.Intro)
            return;

        if (!sliderActive && isInArena)
        {
            sliderActive = true;
            bossHealthSlider?.gameObject.SetActive(true);
        }

        bossHealthSlider?.SetValueWithoutNotify(bossHealth?.currentHealth ?? 0);

        switch (currentState)
        {
            case BossState.Intro: IntroBehavior(); break;
            case BossState.Roar: RoarBehavior(); break;
            case BossState.Turn: TurnBehavior(); break;
            case BossState.Charge: ChargeBehavior(); break;
            case BossState.Stunned: StunnedBehavior(); break;
            case BossState.Cooldown: CooldownBehavior(); break;
        }

        stateTimer -= Time.deltaTime;
    }

    void IntroBehavior()
    {
        rb.linearVelocity = Vector2.zero;
        FacePlayerImmediately();

        if (!introRamPlayed && (introMinDuration - stateTimer) >= 0.5f)
        {
            PlayAnim(ramAnim);
            PlayRoarSound();
            introRamPlayed = true;
        }

        if (stateTimer <= 0)
        {
            StartCoroutine(EndIntroAndStartCombat());
        }
    }

    public void AnimEvent_RamShockwave()
    {
        if (shockWavesManager != null)
            shockWavesManager.CallShockWaves();
    }

    void RoarBehavior()
    {
        rb.linearVelocity = Vector2.zero;
        if (stateTimer <= 0)
        {
            if (audioSource.isPlaying && audioSource.clip == roarSound)
                audioSource.Stop();
            currentState = BossState.Turn;
            stateTimer = 0.1f;
        }
    }

    void TurnBehavior()
    {
        rb.linearVelocity = Vector2.zero;
        FacePlayerImmediately();
        chargeDirection = sr.flipX ? Vector2.right : Vector2.left;
        currentState = BossState.Charge;
    }

    void ChargeBehavior()
    {
        rb.linearVelocity = new Vector2(chargeDirection.x * chargeSpeed, 0f);
        PlayAnim(chargeAnim);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, chargeDirection, obstacleCheckDistance, obstacleLayer);
        if (hit.collider != null)
        {
            rb.linearVelocity = Vector2.zero;
            if (stunImpactSound != null)
                audioSource.PlayOneShot(stunImpactSound, stunVolume);
            stunnedFlipX = sr.flipX;
            isStunned = true;
            currentState = BossState.Stunned;
            stateTimer = stunnedTime;
        }
    }

    void StunnedBehavior()
    {
        rb.linearVelocity = Vector2.zero;
        PlayAnim("choan(bohung)");
        if (stateTimer <= 0)
        {
            isStunned = false;
            currentState = BossState.Stomp;
            StartCoroutine(StompRoutine());
        }
    }

    IEnumerator StompRoutine()
    {
        FacePlayerImmediately();
        PlayAnim(ramAnim);
        PlayRoarSound();

        if (cameraShake)
            cameraShake.Shake(shakeIntensity, rockCount * rockDelay);

        for (int i = 0; i < rockCount; i++)
        {
            if (fallingRockPrefab && arenaTrigger)
            {
                Vector3 dropPos = new Vector3(player.position.x, arenaTrigger.bounds.max.y + rockSpawnHeight, 0);
                Instantiate(fallingRockPrefab, dropPos, Quaternion.identity);
                if (rockDropSound != null)
                    audioSource.PlayOneShot(rockDropSound, rockDropVolume);
            }
            yield return new WaitForSeconds(rockDelay);
        }

        if (audioSource.isPlaying && audioSource.clip == roarSound)
            audioSource.Stop();

        currentState = BossState.Cooldown;
        stateTimer = cooldownTime;
    }

    void CooldownBehavior()
    {
        rb.linearVelocity = Vector2.zero;
        PlayAnim(idleAnim);

        if (stateTimer <= 0)
        {
            if (Random.value < chargeChance)
            {
                currentState = BossState.Turn;
                stateTimer = 0.1f;
            }
            else
            {
                currentState = BossState.Stomp;
                StartCoroutine(StompRoutine());
            }
        }
    }

    void PlayRoarSound()
    {
        if (audioSource != null && roarSound != null)
        {
            if (audioSource.isPlaying)
                audioSource.Stop();
            audioSource.clip = roarSound;
            audioSource.volume = roarVolume;
            audioSource.Play();
        }
    }

    void PlayAnim(string animName)
    {
        anim?.Play(animName);
    }

    void FindBossSlider()
    {
        GameObject sliderObj = GameObject.FindGameObjectWithTag("BossSlider");
        if (sliderObj == null)
            sliderObj = GameObject.Find("BossHealthSlider");

        if (sliderObj != null)
        {
            bossHealthSlider = sliderObj.GetComponent<UnityEngine.UI.Slider>();
            // ✅ KHÔNG ẨN Ở ĐÂY NỮA
        }
        else
        {
            Debug.LogWarning($"⚠️ {GetType().Name}: Không tìm thấy BossHealthSlider (tag 'BossSlider' hoặc tên 'BossHealthSlider')");
        }
    }

    void OnBossDeath()
    {
        bossHealthSlider?.gameObject.SetActive(false);
        musicAudioSource?.Stop();
    }

    private void OnDrawGizmos()
    {
        if (sr != null)
        {
            Gizmos.color = Color.cyan;
            Vector2 direction = sr.flipX ? Vector2.right : Vector2.left;
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + direction * obstacleCheckDistance);
        }

        if (arenaTrigger != null)
        {
            Bounds bounds = arenaTrigger.bounds;
            float ceilingY = bounds.max.y;
            float rockSpawnY = ceilingY + rockSpawnHeight;

            Gizmos.color = Color.green;
            Gizmos.DrawLine(new Vector3(bounds.min.x, ceilingY, 0), new Vector3(bounds.max.x, ceilingY, 0));

            Gizmos.color = Color.red;
            Vector3 rockLineCenter = new Vector3((bounds.min.x + bounds.max.x) * 0.5f, rockSpawnY, 0);
            Gizmos.DrawSphere(rockLineCenter, 0.2f);
            Gizmos.DrawLine(new Vector3(bounds.min.x, rockSpawnY, 0), new Vector3(bounds.max.x, rockSpawnY, 0));
        }
    }
}