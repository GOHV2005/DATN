using System;
using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
public class SceneSaveData
{
    public string sceneName;
    public Vector3 position;
    public float playTime;

    public SceneSaveData() { }

    public SceneSaveData(string sceneName, Vector3 position, float playTime)
    {
        this.sceneName = sceneName;
        this.position = position;
        this.playTime = playTime;
    }
    public string GetPlayTimeString()
    {
        int minutes = Mathf.FloorToInt(playTime / 60f);
        int seconds = Mathf.FloorToInt(playTime % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

}


[Serializable]
public class SaveData
{
    public List<SceneSaveData> scenes = new List<SceneSaveData>();

    public void AddScene(SceneSaveData sceneData)
    {
        var existing = scenes.Find(s => s.sceneName == sceneData.sceneName);
        if (existing != null)
        {
            existing.position = sceneData.position;
            existing.playTime = sceneData.playTime;
        }
        else
        {
            scenes.Add(sceneData);
        }
    }

    // Trả về sceneData theo tên scene
    public SceneSaveData GetScene(string sceneName)
    {
        return scenes.Find(s => s.sceneName == sceneName);
    }

    public SceneSaveData GetLastScene()
    {
        if (scenes.Count == 0) return null;
        return scenes[scenes.Count - 1];
    }
}