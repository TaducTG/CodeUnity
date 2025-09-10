#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(PrefabDatabase))]
public class PrefabDatabaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PrefabDatabase db = (PrefabDatabase)target;

        if (GUILayout.Button("Auto Load Prefabs from Assets/Prefabs"))
        {
            UpdateDatabase(db);
        }
    }

    [MenuItem("Tools/Update Prefab Database")]
    public static void UpdatePrefabDatabaseFromMenu()
    {
        PrefabDatabase db = FindFirstObjectByType<PrefabDatabase>();
        if (db == null)
        {
            Debug.LogError("Không tìm thấy PrefabDatabase trong scene! Hãy tạo 1 GameObject và gắn script PrefabDatabase vào.");
            return;
        }

        UpdateDatabase(db);
        Debug.Log("PrefabDatabase updated từ menu Tools.");
    }

    private static void UpdateDatabase(PrefabDatabase db)
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" });
        List<PrefabEntry> entries = new List<PrefabEntry>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                entries.Add(new PrefabEntry
                {
                    prefabName = prefab.name,
                    prefab = prefab
                });
            }
        }

        db.SetPrefabs(entries);
        EditorUtility.SetDirty(db);

        Debug.Log($"PrefabDatabase updated: {entries.Count} prefabs loaded.");
    }
}
#endif
