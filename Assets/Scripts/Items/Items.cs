using UnityEngine;

public enum ItemType
{
    Consumable,
    Equipment,
    Material,
    Placeable,  // ✅ thêm loại mới
}


public class Items : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public GameObject prefabs;

    public ItemType itemType;

    public bool isStackable;
    public int maxStack = 99;

    public float price;

    public string[] description;
}
