// Assets/Editor/ShaderIntChecker.cs
using UnityEditor;
using UnityEngine;
using System.IO;

public static class ShaderIntChecker
{
    [MenuItem("Tools/Shaders/Find shaders with (Int) properties")]
    public static void FindIntProperties()
    {
        int count = 0;
        string[] shaderGuids = AssetDatabase.FindAssets("t:Shader");
        foreach (string guid in shaderGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!File.Exists(path)) continue;

            string content = File.ReadAllText(path);
            // Simple text check for deprecated "Int)". You can refine if needed.
            if (content.Contains("(Int)"))
            {
                count++;
                Debug.Log($"[INT FOUND] {path}", AssetDatabase.LoadAssetAtPath<Object>(path));
            }
        }

        if (count == 0)
            Debug.Log("No shaders with (Int) properties found. 🎉");
        else
            Debug.Log($"Total shaders with (Int): {count}. Check the Console list above.");
    }
}
