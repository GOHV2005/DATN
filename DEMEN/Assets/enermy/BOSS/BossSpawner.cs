using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class BossSpawner : MonoBehaviour
{
    [Header("Wave Settings")]
    public List<WaveData> waves;
    public Transform[] spawnPoints;
    [Header("Boss Spawn Point")]
    public Transform bossSpawnPoint;
    [Header("Boss Music (Code Controlled)")]
    public AudioClip bossMusic;
    public AudioMixerGroup bossMixerGroup;

    [Header("Animation")]
    public Animator animator;

    [Header("Portal Spawn")]
    public GameObject enemyPortalPrefab;     // 🔵 Cổng quái thường
    public GameObject bossPortalPrefab;      // 🔴 Cổng boss

    [Header("Portal Spawn Points")]
    public Transform[] enemyPortalSpawnPoints; // 3 điểm spawn quái
    public Transform bossPortalSpawnPoint;     // 1 điểm spawn boss

    public float portalLifeTime = 3f;


    private AudioSource arenaSource;
    [HideInInspector] public bool allowCombat = false;

    private int currentWaveIndex = 0;
    private int activeEnemyCount = 0;
    private bool isSpawning = false;
    private bool hasCombatStarted = false;
    private bool[] waveSpawned;

    void Awake()
    {
        waveSpawned = new bool[waves.Count];
        CreateArenaAudioSource();

        if (animator == null)
            animator = GetComponent<Animator>();
        allowCombat = false;        // 🔒 NEW GAME: KHÓA COMBAT
        hasCombatStarted = false;
    }

    void CreateArenaAudioSource()
    {
        arenaSource = gameObject.AddComponent<AudioSource>();

        arenaSource.clip = bossMusic;
        arenaSource.outputAudioMixerGroup = bossMixerGroup;

        arenaSource.playOnAwake = false;
        arenaSource.loop = true;

        // 🔥 ÉP NHẠC NỀN
        arenaSource.spatialBlend = 0f; // 2D
        arenaSource.volume = 1f;
        arenaSource.mute = false;

        Debug.Log("🎵 Arena AudioSource created by code");
    }

    // ===================== COMBAT =====================
    public void StartCombat()
    {
        if (!allowCombat)
        {
            Debug.Log("⛔ Combat bị khóa (chưa xong dialogue)");
            return;
        }

        if (hasCombatStarted) return;

        hasCombatStarted = true;

        currentWaveIndex = 0;
        activeEnemyCount = 0;

        for (int i = 0; i < waveSpawned.Length; i++)
            waveSpawned[i] = false;

        PlayBossMusic();
        StartNextWave();
    }


    void PlayBossMusic()
    {
        if (arenaSource == null || bossMusic == null)
        {
            Debug.LogError("❌ Boss music missing");
            return;
        }

        arenaSource.Stop();
        arenaSource.Play();

        Debug.Log($"🎵 Boss music PLAY → {bossMusic.name}");
    }

    void StopBossMusic()
    {
        if (arenaSource != null && arenaSource.isPlaying)
        {
            arenaSource.Stop();
            Debug.Log("🎵 Boss music STOP");
        }
    }

    // ===================== WAVES =====================

    void StartNextWave()
    {
        if (!hasCombatStarted || currentWaveIndex >= waves.Count)
            return;

        if (waveSpawned[currentWaveIndex])
            return;

        waveSpawned[currentWaveIndex] = true;
        isSpawning = true;

        StartCoroutine(SpawnWave(waves[currentWaveIndex]));
    }
    IEnumerator SpawnEnemyThroughPortal(GameObject enemyPrefab, Vector3 position)
    {
        GameObject portal = Instantiate(enemyPortalPrefab, position, Quaternion.identity);

        ParticleSystem ps = portal.GetComponentInChildren<ParticleSystem>();
        if (ps != null)
        {
            ps.Play();
        }

        yield return new WaitForSeconds(0.4f);

        Instantiate(enemyPrefab, position, Quaternion.identity);
        activeEnemyCount++;

        yield return new WaitForSeconds(portalLifeTime);
        Destroy(portal);
    }
    IEnumerator SpawnBossThroughPortal(GameObject bossPrefab, Vector3 position)
    {
        GameObject portal = Instantiate(bossPortalPrefab, position, Quaternion.identity);

        ParticleSystem ps = portal.GetComponentInChildren<ParticleSystem>();
        if (ps != null)
        {
            ps.Play();
        }

        yield return new WaitForSeconds(0.6f); // boss hoành tráng hơn 😈

        GameObject boss = Instantiate(bossPrefab, position, Quaternion.identity);

        // Skip intro nếu có
        if (boss.TryGetComponent(out BossBeetleAI beetle))
            beetle.skipIntro = true;

        if (boss.TryGetComponent(out BossMantisAI mantis))
            mantis.skipIntro = true;

        activeEnemyCount = 1;

        yield return new WaitForSeconds(portalLifeTime);
        Destroy(portal);
    }

    IEnumerator SpawnWave(WaveData wave)
    {
        if (animator != null)
            animator.SetBool("IsSpamming", true);

        // ================= BOSS WAVE =================
        if (wave.isBossWave)
        {
            if (wave.usePortalSpawn && bossPortalPrefab != null && bossPortalSpawnPoint != null)
            {
                yield return StartCoroutine(
                    SpawnBossThroughPortal(
                        wave.enemyPrefab,
                        bossPortalSpawnPoint.position
                    )
                );
            }
            else
            {
                Instantiate(wave.enemyPrefab, bossSpawnPoint.position, Quaternion.identity);
                activeEnemyCount = 1;
            }

            yield return new WaitForSeconds(wave.spawnInterval);
        }
        // ================= NORMAL WAVE =================
        else
        {
            for (int i = 0; i < wave.enemyCount; i++)
            {
                if (wave.usePortalSpawn && enemyPortalSpawnPoints.Length > 0)
                {
                    Transform p = enemyPortalSpawnPoints[
                        Random.Range(0, enemyPortalSpawnPoints.Length)
                    ];

                    StartCoroutine(
                        SpawnEnemyThroughPortal(
                            wave.enemyPrefab,
                            p.position
                        )
                    );
                }
                else
                {
                    Transform spawnPoint = spawnPoints[i % spawnPoints.Length];
                    Instantiate(wave.enemyPrefab, spawnPoint.position, Quaternion.identity);
                    activeEnemyCount++;
                }

                yield return new WaitForSeconds(wave.spawnInterval);
            }
        }

        isSpawning = false;

        if (animator != null)
            animator.SetBool("IsSpamming", false);
    }


    void OnEnable()
    {
        EnemyDeathHandler.OnEnemyDied += OnEnemyKilled;
    }
    void OnDisable()
    {
        EnemyDeathHandler.OnEnemyDied -= OnEnemyKilled;
    }

    void OnEnemyKilled()
    {
        activeEnemyCount--;

        if (activeEnemyCount <= 0 && !isSpawning)
        {
            currentWaveIndex++;

            if (currentWaveIndex < waves.Count)
                StartNextWave();
            else
                OnAllWavesCompleted();
        }
    }

    void OnAllWavesCompleted()
    {
        Debug.Log("🎉 Boss defeated");
        StopBossMusic();

        var boss = FindObjectOfType<BossFinalController>();
        if (boss != null)
            boss.OnBossDefeated();
    }
}
