using UnityEngine;
using UnityEditor;
using System.IO;

public class ItemPrefabLinker : EditorWindow
{
    [MenuItem("Tools/Link Items with Prefabs (2-way)")]
    public static void LinkItemsAndPrefabs()
    {
        string[] itemGuids = AssetDatabase.FindAssets("t:Items");
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");

        // Map theo tên
        var itemsByName = new System.Collections.Generic.Dictionary<string, Items>();
        foreach (var guid in itemGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Items item = AssetDatabase.LoadAssetAtPath<Items>(path);
            if (item != null)
            {
                itemsByName[item.name] = item;
            }
        }

        foreach (var guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            string prefabName = prefab.name;

            if (itemsByName.TryGetValue(prefabName, out Items item))
            {
                // --- Link prefab vào Items ---
                if (item.prefabs != prefab)
                {
                    item.prefabs = prefab;
                    EditorUtility.SetDirty(item);
                }

                // --- Link Items vào PickUpItem ---
                PickUpItem pickup = prefab.GetComponent<PickUpItem>();
                if (pickup != null && pickup.itemData != item)
                {
                    pickup.itemData = item;
                    EditorUtility.SetDirty(prefab);
                }
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("✅ Linked Items <-> Prefabs thành công!");
    }
}
