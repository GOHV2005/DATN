using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class BossMantisAI : MonoBehaviour
{
    public enum BossState { Idle, Moving, UsingSkill }

    [Header("References")]
    public Transform player;
    public Animator anim;
    public SpriteRenderer sr;
    public SpriteRenderer deadEyes;
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
    public int skill1Damage = 100;
    public int skill2Damage = 100;
    public int skill3Damage = 80;

    [Header("SFX")]
    public AudioClip walkClip;
    public AudioClip attackClip;
    public AudioClip strongAttackClip;
    [Range(0f, 1f)] public float walkVolume = 0.5f;
    [Range(0f, 1f)] public float attackVolume = 0.7f;
    [Range(0f, 1f)] public float strongAttackVolume = 0.8f;

    [Header("BGM")]
    public AudioClip arenaBgm;
    [Range(0f, 1f)] public float musicVolume = 0.8f;

    [Header("Mixer")]
    public AudioMixerGroup sfxMixerGroup;
    public AudioMixerGroup musicMixerGroup;

    private AudioSource sfxSource;
    private AudioSource musicSource;

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

        // AudioSource cho SFX
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f;
        if (sfxMixerGroup != null)
            sfxSource.outputAudioMixerGroup = sfxMixerGroup;

        // AudioSource cho Music
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.volume = musicVolume;
        if (musicMixerGroup != null)
            musicSource.outputAudioMixerGroup = musicMixerGroup;
    }

    void Update()
    {
        if (!combatStarted && player != null && arenaCollider != null)
        {
            if (arenaCollider.bounds.Contains(player.position))
            {
                combatStarted = true;
                StartCombat();
                PlayArenaBgm();
            }
        }

        // DeadEyes cập nhật theo hướng boss
        if (deadEyes != null)
        {
            Vector3 localPos = deadEyes.transform.localPosition;
            localPos.x = sr.flipX ? 0.79f : -0.79f;
            deadEyes.transform.localPosition = localPos;
            deadEyes.flipX = sr.flipX;
        }

        if (currentState == BossState.Moving)
            WalkTowardsPlayer(walkSpeed);
    }

    void StartCombat()
    {
        combatStarted = true;
        PlayArenaBgm();
        StartCoroutine(PlayStartAttackSequence());
    }

    IEnumerator PlayStartAttackSequence()
    {
        currentState = BossState.UsingSkill;

        for (int i = 0; i < 3; i++)
        {
            PlayAnimWithSfx("TanCongManh(Bongua)", strongAttackClip, strongAttackVolume);
            yield return new WaitForSeconds(0.8f); // thời gian animation, chỉnh theo animation thực tế
        }

        PlayAnim("Dung(BoNgua)");
        currentState = BossState.Moving;
        StartCoroutine(SkillLoop());
    }


    IEnumerator CombatRoutine()
    {
        yield return new WaitForSeconds(2f);
        PlayAnim("Dung(BoNgua)");
        currentState = BossState.Moving;
        StartCoroutine(SkillLoop());
    }

    void WalkTowardsPlayer(float speed)
    {
        if (player == null) return;
        Vector3 dir = (player.position - transform.position).normalized;
        transform.position += new Vector3(dir.x * speed * Time.deltaTime, 0, 0);
        sr.flipX = dir.x > 0;
        PlayAnimWithSfx("DiBo(BoNgua)", walkClip, walkVolume);
    }

    IEnumerator SkillLoop()
    {
        while (true)
        {
            currentState = BossState.UsingSkill;
            float r = Random.Range(0f, 1f);

            if (r <= 0.2f)
                yield return StartCoroutine(Skill1());
            else if (r <= 0.6f)
                yield return StartCoroutine(Skill3());
            else
                yield return StartCoroutine(Skill2CheckNear());

            currentState = BossState.Moving;
            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator Skill1()
    {
        if (deadEyes != null)
        {
            deadEyes.gameObject.SetActive(true);
            yield return new WaitForSeconds(deadEyesTime);
            deadEyes.gameObject.SetActive(false);
        }

        yield return StartCoroutine(PerformAttack(skill1Damage));
    }

    IEnumerator Skill2CheckNear()
    {
        if (player == null) yield break;
        if (Vector2.Distance(player.position, transform.position) > playerNearDistance) yield break;
        yield return StartCoroutine(PerformAttack(skill2Damage));
    }

    IEnumerator Skill3()
    {
        if (player == null) yield break;
        if (Vector2.Distance(player.position, transform.position) <= playerNearDistance) yield break;

        PlayAnimWithSfx("TanCongManh(Bongua)", strongAttackClip, strongAttackVolume);
        yield return new WaitForSeconds(0.8f);

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

        currentAttackDamage = skill3Damage;
        isAttackActive = true;
        PlayAnimWithSfx("TanCong(BoNgua)", attackClip, attackVolume);
        yield return StartCoroutine(AttackForward(attackSpeed, chargeDistance));
        PlayAnim("Dung(BoNgua)");
        isAttackActive = false;
    }

    IEnumerator PerformAttack(int damage)
    {
        currentAttackDamage = damage;
        isAttackActive = true;
        PlayAnimWithSfx("TanCong(BoNgua)", attackClip, attackVolume);
        yield return StartCoroutine(AttackForward(attackSpeed, chargeDistance));
        PlayAnim("Dung(BoNgua)");
        isAttackActive = false;
    }

    IEnumerator AttackForward(float speed, float distance)
    {
        Vector3 dir = (player.position.x > transform.position.x) ? Vector3.right : Vector3.left;
        sr.flipX = dir.x > 0;
        float traveled = 0f;
        while (traveled < distance)
        {
            float step = speed * Time.deltaTime;
            transform.position += dir * step;
            traveled += step;
            yield return null;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isAttackActive)
        {
            PlayerController pc = other.GetComponent<PlayerController>();
            if (pc != null)
                pc.TakeDamageFromEnemy(currentAttackDamage, transform.position);
        }
    }

    void PlayAnim(string animName)
    {
        if (anim != null) anim.Play(animName);
    }

    void PlayAnimWithSfx(string animName, AudioClip clip, float volume)
    {
        PlayAnim(animName);
        if (clip != null) sfxSource.PlayOneShot(clip, volume);
    }

    void PlayArenaBgm()
    {
        if (arenaBgm != null && musicSource != null && !musicSource.isPlaying)
        {
            musicSource.clip = arenaBgm;
            musicSource.pitch = 1f; // đảm bảo nhạc chạy đúng tốc độ
            musicSource.volume = musicVolume;
            musicSource.Play();
        }
    }


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
