using UnityEngine;
using System.Collections.Generic;

public class ObstacleSpawner : MonoBehaviour
{
    public MultiObjectPool objectPool;
    public float spawnInterval = 2f;
    public float minY = -2f;
    public float maxY = 2f;
    public float spawnX = 10f;

    private List<string> prefabNames;

    void Start()
    {
        if (objectPool == null)
        {
            Debug.LogError("ObjectPool is not assigned!");
            return;
        }
        prefabNames = objectPool.GetAvailablePrefabNames();
        InvokeRepeating("SpawnObstacle", 1f, spawnInterval);
    }

    void SpawnObstacle()
    {
        if (prefabNames.Count == 0) return;

        string randomPrefab = prefabNames[Random.Range(0, prefabNames.Count)];
        GameObject obstacle = objectPool.GetPooledObject(randomPrefab);

        if (obstacle != null)
        {
            float randomY = Random.Range(minY, maxY);

            // Spawn just behind the spawner's position
            Vector3 spawnPosition = transform.position + new Vector3(-1f, randomY - transform.position.y, 0);
            obstacle.transform.position = spawnPosition;
            obstacle.SetActive(true);
        }
    }

}
