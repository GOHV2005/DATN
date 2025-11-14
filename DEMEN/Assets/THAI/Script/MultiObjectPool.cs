using UnityEngine;
using System.Collections.Generic;

public class MultiObjectPool : MonoBehaviour
{
    [System.Serializable]
    public class PoolItem
    {
        public GameObject prefab;
        public int poolSize;
    }

    public List<PoolItem> itemsToPool;
    private Dictionary<string, List<GameObject>> poolDictionary;

    void Awake()
    {
        poolDictionary = new Dictionary<string, List<GameObject>>();

        foreach (PoolItem item in itemsToPool)
        {
            List<GameObject> objectList = new List<GameObject>();
            for (int i = 0; i < item.poolSize; i++)
            {
                GameObject obj = Instantiate(item.prefab);
                obj.SetActive(false);
                objectList.Add(obj);
            }
            poolDictionary.Add(item.prefab.name, objectList);
        }
    }


    public GameObject GetPooledObject(string prefabName)
    {
        if (poolDictionary.ContainsKey(prefabName))
        {
            foreach (GameObject obj in poolDictionary[prefabName])
            {
                if (!obj.activeInHierarchy)
                    return obj;
            }
        }
        return null;
    }

    public List<string> GetAvailablePrefabNames()
    {
        return new List<string>(poolDictionary.Keys);
    }
}
