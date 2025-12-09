using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSpawner : MonoBehaviour
{
    [Header("Wave Settings")]
    public List<WaveData> waves;
    public Transform spawnPoint;

    private int currentWaveIndex = 0;
    private int activeEnemyCount = 0;
    private bool isSpawning = false;

    void Start()
    {
        Debug.Log("[SPAWNER] Start() called.");

        if (waves == null || waves.Count == 0)
        {
            Debug.LogError("[SPAWNER] ❌ Không có wave nào được gán!");
            return;
        }

        EnemyDeathHandler.OnEnemyDied += OnEnemyKilled;
        Debug.Log($"[SPAWNER] Đã đăng ký sự kiện OnEnemyDied. Tổng wave: {waves.Count}");

        StartCoroutine(DelayBeforeFirstWave(2f));
    }

    void OnDestroy()
    {
        EnemyDeathHandler.OnEnemyDied -= OnEnemyKilled;
        Debug.Log("[SPAWNER] OnDestroy: Hủy đăng ký OnEnemyDied.");
    }

    IEnumerator DelayBeforeFirstWave(float delay)
    {
        Debug.Log($"[SPAWNER] Chờ {delay}s trước wave đầu...");
        yield return new WaitForSeconds(delay);
        StartNextWave();
    }

    void StartNextWave()
    {
        Debug.Log($"[SPAWNER] StartNextWave() gọi. currentWaveIndex = {currentWaveIndex}");

        if (currentWaveIndex >= waves.Count)
        {
            Debug.Log("[SPAWNER] ❌ currentWaveIndex vượt quá số wave. Dừng.");
            return;
        }

        WaveData currentWave = waves[currentWaveIndex];
        if (currentWave.enemyPrefab == null)
        {
            Debug.LogError($"[SPAWNER] ❌ Wave {currentWaveIndex + 1} chưa gán enemyPrefab!");
            return;
        }

        isSpawning = true;
        Debug.Log($"[SPAWNER] Bắt đầu spawn Wave {currentWaveIndex + 1}...");
        StartCoroutine(SpawnWave(currentWave));
    }

    IEnumerator SpawnWave(WaveData wave)
    {
        for (int i = 0; i < wave.enemyCount; i++)
        {
            Instantiate(wave.enemyPrefab, spawnPoint.position, Quaternion.identity);
            activeEnemyCount++;
            Debug.Log($"[SPAWNER] Đã spawn enemy #{i + 1}/{wave.enemyCount} của wave {currentWaveIndex + 1}");
            yield return new WaitForSeconds(wave.spawnInterval);
        }

        isSpawning = false;
        Debug.Log($"[SPAWNER] ✅ Wave {currentWaveIndex + 1} đã spawn xong. Tổng enemy còn sống: {activeEnemyCount}");
    }

    void OnEnemyKilled()
    {
        activeEnemyCount--;
        Debug.Log($"[SPAWNER] 🧨 Một enemy vừa chết! Còn lại: {activeEnemyCount}, isSpawning: {isSpawning}");

        if (activeEnemyCount <= 0 && !isSpawning)
        {
            Debug.Log("[SPAWNER] 🎯 Tất cả enemy đã chết và không còn spawn → chuẩn bị wave tiếp theo");
            currentWaveIndex++;

            if (currentWaveIndex < waves.Count)
            {
                Debug.Log($"[SPAWNER] 🔄 Sẽ bắt đầu Wave {currentWaveIndex + 1} sau 1.5s");
                Invoke(nameof(StartNextWave), 1.5f);
            }
            else
            {
                OnAllWavesCompleted();
            }
        }
    }

    void OnAllWavesCompleted()
    {
        Debug.Log("🎉 [SPAWNER] Tất cả wave đã hoàn thành!");
    }
}