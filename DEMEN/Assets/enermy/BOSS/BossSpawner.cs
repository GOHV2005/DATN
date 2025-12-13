// BossSpawner.cs — spawn enemy ĐỀU ở 2 điểm + tự động chuyển scene "END"
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // ✅ THÊM DÒNG NÀY

public class BossSpawner : MonoBehaviour
{
    [Header("Trigger Settings")]
    public Collider2D bossTriggerZone;
    public string playerTag = "Player";

    [Header("Wave Settings")]
    public List<WaveData> waves;
    public Transform[] spawnPoints;

    [Header("Animation")]
    public Animator animator; // 👈 THÊM: reference đến Animator

    private int currentWaveIndex = 0;
    private int activeEnemyCount = 0;
    private bool isSpawning = false;
    private bool hasCombatStarted = false;
    private bool[] waveSpawned;

    void Start()
    {
        if (waves == null || waves.Count == 0)
        {
            Debug.LogError("[BossSpawner] ❌ Chưa gán wave!");
            return;
        }
        if (arena != null)
            arena.Stop(); // dừng nhạc lúc load scene
        if (spawnPoints == null || spawnPoints.Length != 2)
        {
            Debug.LogError("[BossSpawner] ❌ Cần đúng 2 spawn points!");
            return;
        }

        EnemyDeathHandler.OnEnemyDied += OnEnemyKilled;
        waveSpawned = new bool[waves.Count];

        // 👇 TỰ ĐỘNG LẤY Animator nếu chưa gán
        if (animator == null)
            animator = GetComponent<Animator>();

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
            arena.Play();
            StartCombat();
        }
    }

    void StartCombat()
    {
        if (hasCombatStarted) return;
        hasCombatStarted = true;
        StartNextWave();
    }

    void StartNextWave()
    {
        if (!hasCombatStarted) return;
        if (currentWaveIndex >= waves.Count) return;

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

        waveSpawned[currentWaveIndex] = true;
        isSpawning = true;
        StartCoroutine(SpawnWave(wave));
    }

    IEnumerator SpawnWave(WaveData wave)
    {
        // 👇 BẬT ANIMATION SPAWN ("SpamEne")
        if (animator != null)
            animator.SetBool("IsSpamming", true);

        int total = wave.enemyCount;
        int half = total / 2;
        int extra = total % 2;

        Debug.Log($"[SPAWN] Wave {currentWaveIndex + 1}: {total} enemy → {half + extra} ở điểm 0, {half} ở điểm 1");

        for (int i = 0; i < half + extra; i++)
        {
            Instantiate(wave.enemyPrefab, spawnPoints[0].position, Quaternion.identity);
            activeEnemyCount++;
            yield return new WaitForSeconds(wave.spawnInterval);
        }

        for (int i = 0; i < half; i++)
        {
            Instantiate(wave.enemyPrefab, spawnPoints[1].position, Quaternion.identity);
            activeEnemyCount++;
            yield return new WaitForSeconds(wave.spawnInterval);
        }

        isSpawning = false;
        Debug.Log($"[SPAWN] Hoàn tất spawn {total} enemy chia đều giữa 2 điểm.");

        // 👇 TẮT ANIMATION SPAWN → VỀ IDLE ("DungYen")
        if (animator != null)
            animator.SetBool("IsSpamming", false);
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
        Debug.Log("🎉 Tất cả wave đã hoàn thành! Chuyển sang scene END.");
        SceneManager.LoadScene("END"); // ✅ CHUYỂN SCENE
    }
}
