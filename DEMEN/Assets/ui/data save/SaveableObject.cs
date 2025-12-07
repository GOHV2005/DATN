using UnityEngine;
using System;

public class SaveableObject : MonoBehaviour
{
    [SerializeField] public string guid;

    private void Reset()
    {
        if (string.IsNullOrEmpty(guid))
            guid = Guid.NewGuid().ToString();
    }
}
