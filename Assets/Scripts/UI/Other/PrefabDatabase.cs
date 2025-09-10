using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PrefabEntry
{
    public string prefabName;
    public GameObject prefab;
}

public class PrefabDatabase : MonoBehaviour
{
    public static PrefabDatabase Instance;

    [SerializeField] private List<PrefabEntry> prefabs = new List<PrefabEntry>();
    private Dictionary<string, GameObject> prefabDict;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        prefabDict = new Dictionary<string, GameObject>();
        foreach (var entry in prefabs)
        {
            if (entry.prefab != null && !prefabDict.ContainsKey(entry.prefabName))
                prefabDict.Add(entry.prefabName, entry.prefab);
        }
    }

    public GameObject GetPrefab(string name)
    {
        if (prefabDict.TryGetValue(name, out var prefab))
            return prefab;

        Debug.LogWarning("Prefab not found: " + name);
        return null;
    }

    // Cho Editor Script truy cập
    public void SetPrefabs(List<PrefabEntry> newPrefabs)
    {
        prefabs = newPrefabs;
    }
}
