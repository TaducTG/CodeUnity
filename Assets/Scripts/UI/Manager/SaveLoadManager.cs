using UnityEngine;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Save / Load toàn bộ game
/// - Player position, inventory, equipment
/// - MapObject
/// - Placeable (kèm chest inventory nếu có)
/// - Enemy reset về Pool
/// </summary>

public class SaveLoadManager : MonoBehaviour
{
    public string saveFileName = "save.json";

    // ======================
    // Public API
    // ======================

    public void SaveGame()
    {
        SaveData data = new SaveData();

        // --- Lưu Player ---
        Player player = FindFirstObjectByType<Player>();
        Inventory inventory = player.GetComponent<Inventory>();

        if (inventory == null)
        {
            Debug.Log("Không tìm thấy Inventory trong Player");
        }
        
        if (player != null)
        {
            data.playerData = new PlayerData
            {
                x = player.transform.position.x,
                y = player.transform.position.y,
                z = player.transform.position.z,
                health = player.GetComponent<Stat>().health,
                mana = player.GetComponent<Stat>().mana,
                inventoryItems = ConvertInventory(inventory.items),
                equipmentItems = ConvertEquipment(inventory.equipment)
            };
        }

        // --- Lưu MapObjects ---
        foreach (var obj in GameObject.FindGameObjectsWithTag("MapObject"))
        {
            string chunkName = obj.transform.parent != null ? obj.transform.parent.name : null;

            data.mapObjects.Add(new MapObjectData
            {
                prefabName = obj.name.Replace("(Clone)", ""),
                x = obj.transform.position.x,
                y = obj.transform.position.y,
                z = obj.transform.position.z,
                chunkName = chunkName
            });
        }

        // --- Lưu Placeables ---
        foreach (var obj in GameObject.FindGameObjectsWithTag("Placeable"))
        {
            PlaceableData pd = new PlaceableData
            {
                prefabName = obj.name.Replace("(Clone)", ""),
                x = obj.transform.position.x,
                y = obj.transform.position.y,
                z = obj.transform.position.z
            };

            Chest chest = obj.GetComponent<Chest>();
            if (chest != null)
            {
                pd.chestItems = ConvertInventory(chest.items);
            }

            data.placeables.Add(pd);
        }

        // --- Ghi file ---
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(Application.persistentDataPath + "/" + saveFileName, json);
        Debug.Log("Game Saved! Path: " + Application.persistentDataPath);
    }

    public void LoadGame()
    {
        string path = Application.persistentDataPath + "/" + saveFileName;
        if (!File.Exists(path)) return;

        string json = File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        // Xóa Placeable và MapObject hiện tại
        foreach (var obj in GameObject.FindGameObjectsWithTag("MapObject"))
            Destroy(obj);
        foreach (var obj in GameObject.FindGameObjectsWithTag("Placeable"))
            Destroy(obj);

        // Spawn lại MapObjects
        foreach (var mo in data.mapObjects)
        {
            GameObject prefab = PrefabDatabase.Instance.GetPrefab(mo.prefabName);
            if (prefab != null)
            {
                // tìm chunk cha
                Transform parentChunk = null;
                if (!string.IsNullOrEmpty(mo.chunkName))
                {
                    GameObject chunkObj = GameObject.Find(mo.chunkName);
                    if (chunkObj != null)
                        parentChunk = chunkObj.transform;
                }

                // spawn vào chunk
                GameObject go = Instantiate(
                    prefab,
                    new Vector3(mo.x, mo.y, mo.z),
                    Quaternion.identity,
                    parentChunk
                );
            }
            else
            {
                Debug.LogWarning("Không tìm thấy prefab MapObject: " + mo.prefabName);
            }
        }

        // Spawn lại Placeables
        foreach (var pl in data.placeables)
        {
            GameObject prefab = PrefabDatabase.Instance.GetPrefab(pl.prefabName);
            if (prefab != null)
            {
                GameObject go = Instantiate(prefab, new Vector3(pl.x, pl.y, pl.z), Quaternion.identity);

                // Nếu là chest thì restore inventory
                Chest chest = go.GetComponent<Chest>();
                if (chest != null)
                {
                    chest.items = RestoreInventory(pl.chestItems);
                }
            }
            else
            {
                Debug.LogWarning("Không tìm thấy prefab Placeable: " + pl.prefabName);
            }
        }

        // --- Enemy trả về Pool ---
        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Stat stat = enemy.GetComponent<Stat>();
            if (stat != null)
            {
                stat.health = 0;
            }

        }

        foreach (var obj in GameObject.FindGameObjectsWithTag("Item"))
            Destroy(obj);
        // Đặt lại Player
        Player player = FindFirstObjectByType<Player>();
        if (player != null)
        {
            player.transform.position = new Vector3(
                data.playerData.x,
                data.playerData.y,
                data.playerData.z
            );
            player.GetComponent<Stat>().health = data.playerData.health;
            player.GetComponent<Stat>().mana = data.playerData.mana;
            player.inventory.items = RestoreInventory(data.playerData.inventoryItems);
            player.inventory.equipment = RestoreEquipment(data.playerData.equipmentItems);
        }

        // Resume game
        Time.timeScale = 1f;

        Debug.Log("Game Loaded!");
    }


    // ======================
    // Helper Methods
    // ======================

    private List<InventoryItemData> ConvertInventory(List<InventoryItem> items)
    {
        List<InventoryItemData> result = new List<InventoryItemData>();

        if (items == null)
            return result;

        for (int i = 0; i < items.Count; i++)
        {
            var inv = items[i];

            if (inv == null || inv.itemData == null)
            {
                // Giữ slot rỗng
                result.Add(new InventoryItemData
                {
                    itemName = null,
                    quantity = 0,
                    isEmpty = true
                });
            }
            else
            {
                result.Add(new InventoryItemData
                {
                    itemName = inv.itemData.name,
                    quantity = inv.quantity,
                    isEmpty = false
                });
            }
        }

        return result;
    }


    private List<EquipmentItemData> ConvertEquipment(List<EquipmentItem> equipment)
    {
        List<EquipmentItemData> result = new List<EquipmentItemData>();
        foreach (var eq in equipment)
        {
            if (eq != null)
            {
                result.Add(new EquipmentItemData
                {
                    itemName = eq.name,
                    slotType = eq.slotType
                });
            }
        }
        return result;
    }

    private List<InventoryItem> RestoreInventory(List<InventoryItemData> data)
    {
        List<InventoryItem> result = new List<InventoryItem>();
        Items[] allItems = Resources.LoadAll<Items>("");

        foreach (var d in data)
        {
            if (d == null || d.isEmpty || string.IsNullOrEmpty(d.itemName))
            {
                // Slot rỗng
                result.Add(null);
            }
            else
            {
                Items found = System.Array.Find(allItems, i => i.name == d.itemName);
                if (found != null)
                {
                    result.Add(new InventoryItem(found, d.quantity));
                }
                else
                {
                    Debug.LogWarning("Không tìm thấy Item: " + d.itemName);
                    result.Add(null);
                }
            }
        }
        return result;
    }


    private List<EquipmentItem> RestoreEquipment(List<EquipmentItemData> data)
    {
        List<EquipmentItem> result = new List<EquipmentItem>();
        EquipmentItem[] allEquip = Resources.LoadAll<EquipmentItem>("");

        foreach (var d in data)
        {
            if (d == null || d.isEmpty || string.IsNullOrEmpty(d.itemName))
            {
                result.Add(null);
            }
            else
            {
                EquipmentItem found = System.Array.Find(allEquip, i => i.name == d.itemName);
                if (found != null)
                {
                    result.Add(found);
                }
                else
                {
                    Debug.LogWarning("Không tìm thấy Equipment: " + d.itemName);
                    result.Add(null);
                }
            }
        }
        return result;
    }





    [System.Serializable]
    public class SaveData
    {
        public PlayerData playerData;
        public List<MapObjectData> mapObjects = new List<MapObjectData>();
        public List<PlaceableData> placeables = new List<PlaceableData>();
    }

    [System.Serializable]
    public class PlayerData
    {
        public float x, y, z;
        public float health;
        public float mana;
        public List<InventoryItemData> inventoryItems = new List<InventoryItemData>();
        public List<EquipmentItemData> equipmentItems = new List<EquipmentItemData>();
    }

    [System.Serializable]
    public class MapObjectData
    {
        public string prefabName;
        public float x, y, z;
        public string chunkName;
    }

    [System.Serializable]
    public class PlaceableData
    {
        public string prefabName;
        public float x, y, z;
        public List<InventoryItemData> chestItems = new List<InventoryItemData>();
    }

    [System.Serializable]
    public class InventoryItemData
    {
        public string itemName;
        public int quantity;
        public bool isEmpty; // slot này rỗng
    }


    [System.Serializable]
    public class EquipmentItemData
    {
        public string itemName;
        public EquipmentSlotType slotType;
        public bool isEmpty; // slot này rỗng
    }

}
