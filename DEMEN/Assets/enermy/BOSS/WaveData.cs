// WaveData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "Wave", menuName = "Game/Wave Data")]
public class WaveData : ScriptableObject
{
    public int enemyCount = 5;
    [Tooltip("Thời gian (giây) giữa mỗi enemy được spawn")]
    public float spawnInterval = 1f;

    [Tooltip("Chọn loại enemy cho wave này")]
    public GameObject enemyPrefab; // ← Mỗi wave chọn 1 loại
}