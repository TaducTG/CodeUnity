using UnityEngine;
using System.Collections.Generic;

public class EnemyPoolManager : MonoBehaviour
{
    public static EnemyPoolManager Instance;

    // Key = prefab name, Value = queue object
    private Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();

    // Map object -> key để returnToPool không bị mismatch tên
    private Dictionary<GameObject, string> objectToKey = new Dictionary<GameObject, string>();

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void CreatePool(GameObject prefab, int poolSize)
    {
        string key = prefab.name;

        if (!poolDictionary.ContainsKey(key))
        {
            poolDictionary[key] = new Queue<GameObject>();

            for (int i = 0; i < poolSize; i++)
            {
                GameObject obj = Instantiate(prefab);
                obj.SetActive(false);
                objectToKey[obj] = key;
                poolDictionary[key].Enqueue(obj);
            }
        }
    }

    public GameObject SpawnFromPool(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        string key = prefab.name;

        if (!poolDictionary.ContainsKey(key) || poolDictionary[key].Count == 0)
        {
            // Pool rỗng => không spawn thêm (giữ giới hạn poolSize)
            return null;
        }

        GameObject obj = poolDictionary[key].Dequeue();
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);
        return obj;
    }

    public void ReturnToPool(GameObject obj)
    {
        //Debug.Log(1);
        if (objectToKey.TryGetValue(obj, out string key))
        {
            obj.SetActive(false);
            poolDictionary[key].Enqueue(obj);
        }
        else
        {
            Debug.LogWarning("Object này không thuộc pool: " + obj.name);
            Destroy(obj); // fallback nếu object không thuộc pool
        }
    }
}
