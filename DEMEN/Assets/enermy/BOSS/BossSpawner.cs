// BossSpawner.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSpawner : MonoBehaviour
{
    [Header("Trigger Settings")]
    public Collider2D bossTriggerZone; // Kéo vùng trigger vào đây
    public string playerTag = "Player"; // Đảm bảo player có tag này

    [Header("Wave Settings")]
    public List<WaveData> waves;
    public Transform spawnPoint;

    private int currentWaveIndex = 0;
    private int activeEnemyCount = 0;
    private bool isSpawning = false;
    private bool hasCombatStarted = false; // ← DÙNG ĐỂ KIỂM TRA ĐÃ BẮT ĐẦU CHƯA

    void Start()
    {
        if (waves == null || waves.Count == 0)
        {
            Debug.LogError("[BossSpawner] ❌ Chưa gán wave!");
            return;
        }

        if (bossTriggerZone == null)
        {
            Debug.LogWarning("[BossSpawner] ⚠️ Chưa gán BossTriggerZone → sẽ auto-start wave.");
            StartCombat();
        }
        else
        {
            // Đảm bảo trigger là "Is Trigger"
            bossTriggerZone.isTrigger = true;
            EnemyDeathHandler.OnEnemyDied += OnEnemyKilled;
            Debug.Log("[BossSpawner] Đợi player vào vùng boss...");
        }
    }

    void OnDestroy()
    {
        EnemyDeathHandler.OnEnemyDied -= OnEnemyKilled;
    }

    // 🔥 KÍCH HOẠT KHI PLAYER VÀO VÙNG
    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasCombatStarted) return;

        if (other.CompareTag(playerTag))
        {
            Debug.Log("[BossSpawner] 👑 Player đã vào vùng boss! Bắt đầu combat...");
            StartCombat();
        }
    }

    void StartCombat()
    {
        if (hasCombatStarted) return;

        hasCombatStarted = true;
        StartCoroutine(DelayBeforeFirstWave(1f)); // delay nhỏ trước wave 1
    }

    // ================ PHẦN WAVE (giữ nguyên như cũ) ================

    IEnumerator DelayBeforeFirstWave(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartNextWave();
    }

    void StartNextWave()
    {
        if (currentWaveIndex >= waves.Count) return;

        WaveData currentWave = waves[currentWaveIndex];
        if (currentWave.enemyPrefab == null)
        {
            Debug.LogError($"[BossSpawner] Wave {currentWaveIndex + 1} chưa gán enemyPrefab!");
            return;
        }

        isSpawning = true;
        StartCoroutine(SpawnWave(currentWave));
    }

    IEnumerator SpawnWave(WaveData wave)
    {
        for (int i = 0; i < wave.enemyCount; i++)
        {
            Instantiate(wave.enemyPrefab, spawnPoint.position, Quaternion.identity);
            activeEnemyCount++;
            yield return new WaitForSeconds(wave.spawnInterval);
        }

        isSpawning = false;
        Debug.Log($"[BossSpawner] ✅ Wave {currentWaveIndex + 1} đã spawn xong.");
    }

    void OnEnemyKilled()
    {
        activeEnemyCount--;

        if (activeEnemyCount <= 0 && !isSpawning)
        {
            currentWaveIndex++;
            if (currentWaveIndex < waves.Count)
            {
                Debug.Log($"[BossSpawner] ➡️ Bắt đầu Wave {currentWaveIndex + 1}...");
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
        Debug.Log("🎉 [BossSpawner] Tất cả wave hoàn thành! Boss có thể bắt đầu chiến đấu thực sự.");
        // Sau này bạn có thể gọi: bossAI.StartBossFight();
    }
}