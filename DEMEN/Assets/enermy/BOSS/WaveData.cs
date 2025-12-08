// WaveData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "New Wave", menuName = "Wave/Wave Data")]
public class WaveData : ScriptableObject
{
    public int enemyCount = 5;
    public float spawnInterval = 1f; // thời gian giữa mỗi lần spawn
}