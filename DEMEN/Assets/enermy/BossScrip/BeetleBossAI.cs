using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class BossBeetleAI : MonoBehaviour
{
    public enum BossState { Idle, Roar, Turn, Charge, Stunned, Stomp, Cooldown }
    private BossState currentState = BossState.Idle;

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

    // =============== ÂM THANH CÓ VOLUME RIÊNG ===============
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
    public AudioMixerGroup sfxMixerGroup;   // Group "SFX"
    public AudioMixerGroup musicMixerGroup; // Group "Music"

    [Header("ARENA BARRIER")]
    public GameObject arenaBarrier; // Kéo GameObject này từ Inspector
    [Header("UI")]
    public Slider bossHealthSlider;   // kéo slider từ Inspector
    private Health bossHealth;        // tham chiếu Health component

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

    private float lastFootstepTime = 0f;
    private int nextFootstepIndex = 0;

    void Start()
    {

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        if (!player)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // ====== SFX AudioSource ======
        if (GetComponent<AudioSource>() == null)
            gameObject.AddComponent<AudioSource>();
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        if (sfxMixerGroup != null)
            audioSource.outputAudioMixerGroup = sfxMixerGroup;

        // ====== MUSIC AudioSource ======
        musicAudioSource = gameObject.AddComponent<AudioSource>();
        musicAudioSource.playOnAwake = false;
        musicAudioSource.loop = true;
        musicAudioSource.spatialBlend = 0f;
        if (musicMixerGroup != null)
            musicAudioSource.outputAudioMixerGroup = musicMixerGroup;

        // ====== UI Health Slider ======
        bossHealth = GetComponent<Health>();
        if (bossHealth != null)
        {
            bossHealthSlider.maxValue = bossHealth.maxHealth;
            bossHealthSlider.value = bossHealth.currentHealth;
            bossHealthSlider.gameObject.SetActive(false);

            // Đăng ký sự kiện boss chết
            bossHealth.onDeath += OnBossDeath;
        }

        PlayAnim(idleAnim);
    }

    // Khi boss chết
    void OnBossDeath()
    {
        if (bossHealthSlider != null)
            bossHealthSlider.gameObject.SetActive(false);
    }

    public void StartCombat()
    {
        if (playerEnteredArena) return;

        playerEnteredArena = true;

        if (arenaBarrier != null)
            arenaBarrier.SetActive(true);

        currentState = BossState.Roar;
        stateTimer = roarTime;
        PlayAnim(ramAnim);
        PlayRoarSound();

        if (cameraShake)
            cameraShake.Shake(shakeIntensity, roarTime);

        if (!musicAudioSource.isPlaying && bossMusic != null)
        {
            musicAudioSource.clip = bossMusic;
            musicAudioSource.Play();
        }
    }


    void Update()
    {
        if (!player) return;

        bool isInArena = arenaTrigger && arenaTrigger.bounds.Contains(player.position);
        if (!playerEnteredArena) return; // 🔒 CHỐT CHẶN

        // XỬ LÝ NHẠC NỀN + KÍCH HOẠT TRẬN ĐẤN

        // CẬP NHẬT HƯỚNG SPRITE
        if (currentState == BossState.Charge)
        {
            sr.flipX = (rb.linearVelocity.x > 0);
        }
        else if (isStunned)
        {
            sr.flipX = stunnedFlipX;
        }
        else
        {
            sr.flipX = (player.position.x > transform.position.x);
        }
        if (!sliderActive && arenaTrigger != null && arenaTrigger.bounds.Contains(player.position))
        {
            sliderActive = true;
            if (bossHealthSlider != null)
                bossHealthSlider.gameObject.SetActive(true);
        }

        // cập nhật thanh máu
        if (bossHealthSlider != null && bossHealth != null)
        {
            bossHealthSlider.value = bossHealth.currentHealth;
        }
        // HÀNH VI THEO TRẠNG THÁI
        switch (currentState)
        {
            case BossState.Roar:
                RoarBehavior();
                break;
            case BossState.Turn:
                TurnBehavior();
                break;
            case BossState.Charge:
                ChargeBehavior();
                break;
            case BossState.Stunned:
                StunnedBehavior();
                break;
            case BossState.Stomp:
                break;
            case BossState.Cooldown:
                CooldownBehavior();
                break;
        }

        // =============== FOOTSTEP SOUNDS ===============
        if (Mathf.Abs(rb.linearVelocity.x) > 0.1f && !isStunned)
        {
            bool canPlayFootstep =
                currentState == BossState.Turn ||
                currentState == BossState.Cooldown ||
                currentState == BossState.Charge;
           

            if (canPlayFootstep && Time.time - lastFootstepTime >= footstepInterval)
            {
                if (footstepClips != null && footstepClips.Length > 0)
                {
                    AudioClip clip = footstepClips[nextFootstepIndex];
                    audioSource.PlayOneShot(clip, footstepVolume);
                    nextFootstepIndex = (nextFootstepIndex + 1) % footstepClips.Length;
                    lastFootstepTime = Time.time;
                }
            }
        }

        stateTimer -= Time.deltaTime;
    }
    void OnDeath()
    {
        if (bossHealthSlider != null)
            bossHealthSlider.gameObject.SetActive(false);
    }
    // GỌI TỪ ANIMATION EVENT
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
        if (stateTimer <= 0)
        {
            chargeDirection = (player.position.x > transform.position.x) ? Vector2.right : Vector2.left;
            currentState = BossState.Charge;
        }
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
        PlayAnim(ramAnim);
        PlayRoarSound();

        if (cameraShake)
            cameraShake.Shake(shakeIntensity, rockCount * rockDelay);

        for (int i = 0; i < rockCount; i++)
        {
            if (fallingRockPrefab && arenaTrigger)
            {
                Vector3 dropPos = new Vector3(
                    player.position.x,
                    arenaTrigger.bounds.max.y + rockSpawnHeight,
                    0
                );
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
        if (anim != null)
            anim.Play(animName);
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
            Gizmos.DrawLine(
                new Vector3(bounds.min.x, ceilingY, 0),
                new Vector3(bounds.max.x, ceilingY, 0)
            );

            Gizmos.color = Color.red;
            Vector3 rockLineCenter = new Vector3((bounds.min.x + bounds.max.x) * 0.5f, rockSpawnY, 0);
            Gizmos.DrawSphere(rockLineCenter, 0.2f);

            Gizmos.DrawLine(
                new Vector3(bounds.min.x, rockSpawnY, 0),
                new Vector3(bounds.max.x, rockSpawnY, 0)
            );
        }
    }
}