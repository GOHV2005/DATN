using System;
using UnityEngine;

[Serializable]
public class SaveData
{
    [SerializeField] private string sceneName;
    [SerializeField] private float playTime;
    [SerializeField] private float posX, posY, posZ;

    public SaveData(Vector3 pos, string scene, float time)
    {
        posX = pos.x; posY = pos.y; posZ = pos.z;
        sceneName = scene;
        playTime = time;
    }

    public Vector3 GetPosition() => new Vector3(posX, posY, posZ);
    public string GetSceneName() => sceneName;
    public float GetPlayTime() => playTime;
}
