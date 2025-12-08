// BossSpawner.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Boss chỉ đứng yên và spawn enemy theo từng wave.
/// Cấu hình wave qua Inspector bằng ScriptableObject WaveData.
/// </summary>
public class BossSpawner : MonoBehaviour
{
    [Header("Wave Settings")]
    public List<WaveData> waves;
    public GameObject enemyPrefab; // Phải có EnemyDeathHandler
    public Transform spawnPoint;

    [Header("Game End")]
    public bool endGameAfterAllWaves = true;

    private int currentWaveIndex = 0;
    private int activeEnemyCount = 0;
    private bool isSpawning = false;

    void Start()
    {
        if (waves == null || waves.Count == 0)
        {
            Debug.LogError("Vui lòng gán các Wave vào BossSpawner trong Inspector!");
            return;
        }

        // Đăng ký sự kiện: khi enemy chết → gọi OnEnemyKilled
        EnemyDeathHandler.OnEnemyDied += OnEnemyKilled;

        StartCoroutine(DelayBeforeFirstWave(2f));
    }

    void OnDestroy()
    {
        // Hủy đăng ký để tránh lỗi
        EnemyDeathHandler.OnEnemyDied -= OnEnemyKilled;
    }

    IEnumerator DelayBeforeFirstWave(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartNextWave();
    }

    void StartNextWave()
    {
        if (currentWaveIndex >= waves.Count) return;

        WaveData wave = waves[currentWaveIndex];
        isSpawning = true;
        StartCoroutine(SpawnWave(wave));
    }

    IEnumerator SpawnWave(WaveData wave)
    {
        for (int i = 0; i < wave.enemyCount; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(wave.spawnInterval);
        }
        isSpawning = false;
        Debug.Log($"Wave {currentWaveIndex + 1} đã spawn xong.");
    }

    void SpawnEnemy()
    {
        Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        activeEnemyCount++;
    }

    void OnEnemyKilled()
    {
        activeEnemyCount--;
        Debug.Log($"Còn lại {activeEnemyCount} enemy");

        if (activeEnemyCount <= 0 && !isSpawning)
        {
            currentWaveIndex++;
            if (currentWaveIndex < waves.Count)
            {
                Debug.Log("Chuẩn bị wave tiếp theo...");
                Invoke(nameof(StartNextWave), 2f);
            }
            else
            {
                OnAllWavesCompleted();
            }
        }
    }

    void OnAllWavesCompleted()
    {
        Debug.Log("✅ Tất cả wave đã hoàn thành! Màn chơi kết thúc.");

        // Bạn có thể thêm logic kết thúc ở đây sau, ví dụ:
        // - Hiển thị text "You Win"
        // - Dừng thời gian: Time.timeScale = 0f;
        // - Load scene thắng: SceneManager.LoadScene("WinScene");

        // Hiện tại chỉ log để bạn dễ test.
    }
}