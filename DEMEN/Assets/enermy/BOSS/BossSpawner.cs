using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class BossSpawner : MonoBehaviour
{
    [Header("Wave Settings")]
    public List<WaveData> waves;
    public Transform[] spawnPoints;

    [Header("Boss Music (Code Controlled)")]
    public AudioClip bossMusic;
    public AudioMixerGroup bossMixerGroup;

    [Header("Animation")]
    public Animator animator;

    private AudioSource arenaSource;

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
        Debug.Log("🔥 StartCombat()");

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

    IEnumerator SpawnWave(WaveData wave)
    {
        if (animator != null)
            animator.SetBool("IsSpamming", true);

        int total = wave.enemyCount;

        // 🔥 CASE ĐẶC BIỆT: chỉ 1 enemy
        if (total == 1)
        {
            Instantiate(wave.enemyPrefab, spawnPoints[0].position, Quaternion.identity);
            activeEnemyCount++;

            yield return new WaitForSeconds(wave.spawnInterval);
        }
        else
        {
            int half = total / 2;
            int extra = total % 2;

            // Spawn bên trái (0) – nhiều hơn nếu lẻ
            for (int i = 0; i < half + extra; i++)
            {
                Instantiate(wave.enemyPrefab, spawnPoints[0].position, Quaternion.identity);
                activeEnemyCount++;
                yield return new WaitForSeconds(wave.spawnInterval);
            }

            // Spawn bên phải (1)
            for (int i = 0; i < half; i++)
            {
                Instantiate(wave.enemyPrefab, spawnPoints[1].position, Quaternion.identity);
                activeEnemyCount++;
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
