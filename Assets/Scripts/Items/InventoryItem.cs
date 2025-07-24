using NUnit.Framework.Interfaces;
using UnityEngine;

[System.Serializable]
public class InventoryItem
{
    public Items itemData;
    public int quantity;

    public InventoryItem(Items data, int quantity = 1)
    {
        this.itemData = data;
        this.quantity = quantity;
    }
}

