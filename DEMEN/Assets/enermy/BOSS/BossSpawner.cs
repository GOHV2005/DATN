// BossSpawner.cs — phần spawn đã được sửa để ĐÚNG SỐ LƯỢNG
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSpawner : MonoBehaviour
{
    [Header("Trigger Settings")]
    public Collider2D bossTriggerZone;
    public string playerTag = "Player";

    [Header("Wave Settings")]
    public List<WaveData> waves;
    public Transform spawnPoint;

    private int currentWaveIndex = 0;
    private int activeEnemyCount = 0;
    private bool isSpawning = false;
    private bool hasCombatStarted = false;

    // ➤ THÊM: Ngăn wave bị spawn lại
    private bool[] waveSpawned;

    void Start()
    {
        if (waves == null || waves.Count == 0)
        {
            Debug.LogError("[BossSpawner] ❌ Chưa gán wave!");
            return;
        }

        EnemyDeathHandler.OnEnemyDied += OnEnemyKilled;
        waveSpawned = new bool[waves.Count]; // theo dõi wave nào đã spawn

        if (bossTriggerZone == null)
        {
            StartCombat();
        }
        else
        {
            bossTriggerZone.isTrigger = true;
        }
    }

    void OnDestroy()
    {
        EnemyDeathHandler.OnEnemyDied -= OnEnemyKilled;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasCombatStarted && other.CompareTag(playerTag))
        {
            StartCombat();
        }
    }

    void StartCombat()
    {
        if (hasCombatStarted) return;
        hasCombatStarted = true;
        StartNextWave(); // không cần delay nếu bạn muốn
    }

    void StartNextWave()
    {
        if (!hasCombatStarted) return;
        if (currentWaveIndex >= waves.Count) return;

        // ➤ KIỂM TRA: WAVE NÀY ĐÃ SPAWN CHƯA?
        if (waveSpawned[currentWaveIndex])
        {
            Debug.LogWarning($"[BossSpawner] Wave {currentWaveIndex + 1} đã spawn rồi! Bỏ qua.");
            return;
        }

        WaveData wave = waves[currentWaveIndex];
        if (wave.enemyPrefab == null)
        {
            Debug.LogError($"[BossSpawner] Wave {currentWaveIndex + 1} thiếu enemyPrefab!");
            return;
        }

        waveSpawned[currentWaveIndex] = true; // đánh dấu đã spawn
        isSpawning = true;
        StartCoroutine(SpawnWave(wave));
    }

    IEnumerator SpawnWave(WaveData wave)
    {
        int countToSpawn = wave.enemyCount;
        Debug.Log($"[SPAWN] Bắt đầu spawn {countToSpawn} enemy cho Wave {currentWaveIndex + 1}");

        for (int i = 0; i < countToSpawn; i++)
        {
            Instantiate(wave.enemyPrefab, spawnPoint.position, Quaternion.identity);
            activeEnemyCount++;
            yield return new WaitForSeconds(wave.spawnInterval);
        }

        isSpawning = false;
        Debug.Log($"[SPAWN] Đã spawn đúng {countToSpawn} enemy.");
    }

    void OnEnemyKilled()
    {
        activeEnemyCount--;

        if (activeEnemyCount <= 0 && !isSpawning)
        {
            currentWaveIndex++;
            if (currentWaveIndex < waves.Count)
            {
                StartNextWave();
            }
            else
            {
                OnAllWavesCompleted();
            }
        }
    }

    void OnAllWavesCompleted()
    {
        Debug.Log("🎉 Tất cả wave đã hoàn thành!");
    }
}