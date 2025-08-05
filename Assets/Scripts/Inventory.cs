using System.Collections.Generic;
using NUnit.Framework.Interfaces;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<InventoryItem> items;
    public int maxSlots = 21;

    public InventorySlotUI[] slotUIs; // các slot hiển thị trên UI (kéo vào Inspector)

    void Start()
    {

        for (int i = 0; i < maxSlots; i++)
        {
            items.Add(null);
        }
        RefreshUI();
    }

    public bool AddItems(Items itemData, int amount = 1)
    {
        if (itemData.isStackable)
        {
            // 1. Tìm slot đã có item đó và còn chỗ trống
            foreach (InventoryItem invItem in items)
            {
                if (invItem != null && invItem.itemData == itemData && invItem.quantity < itemData.maxStack)
                {
                    int spaceLeft = itemData.maxStack - invItem.quantity;
                    int amountToAdd = Mathf.Min(spaceLeft, amount);
                    invItem.quantity += amountToAdd;
                    amount -= amountToAdd;
                    if (amount <= 0)
                    {
                        RefreshUI();
                        return true;// đã thêm hết
                    }
                }
            }
        }

        // 2. Nếu còn lượng cần thêm → tìm slot null đầu tiên
        for (int i = 0; i < items.Count && amount > 0; i++)
        {
            if (items[i] == null || items[i].itemData == null)
            {
                int amountToAdd = itemData.isStackable ? Mathf.Min(itemData.maxStack, amount) : 1;
                items[i] = new InventoryItem(itemData, amountToAdd);
                amount -= amountToAdd;
            }
        }
        RefreshUI();
        return amount <= 0;

    }
    // Đếm tổng số item đang có
    public int GetTotalItem(Items itemData)
    {
        int total = 0;
        foreach (var item in items)
        {
            if (item != null && item.itemData == itemData)
                total += item.quantity;
        }
        return total;
    }

    // Xóa item theo số lượng
    public void RemoveItem(Items itemData, int amount)
    {
        for (int i = 0; i < items.Count && amount > 0; i++)
        {
            if (items[i] != null && items[i].itemData == itemData)
            {
                if (items[i].quantity > amount)
                {
                    items[i].quantity -= amount;
                    RefreshUI();
                    return;
                }
                else
                {
                    amount -= items[i].quantity;
                    items[i].itemData = null;
                    items[i] = null;
                }
            }
        }
        RefreshUI();
    }
    public void RefreshUI()
    {
        for (int i = 0; i < slotUIs.Length; i++)
        {
            if (i < items.Count && items[i] != null)
            {
                slotUIs[i].SetItem(items[i]);
            }
            else
            {
                slotUIs[i].ClearSlot();
            }
        }
    }
    // Khi drag drop đổi chỗ 2 slot
    public void SwapItems(int fromIndex, int toIndex)
    {
        InventoryItem temp = items[fromIndex];
        items[fromIndex] = items[toIndex];
        items[toIndex] = temp;

        RefreshUI();
    }

    // Gọi khi UI bị thay đổi do người dùng kéo thả hoặc craft
    public void UpdateFromSlots()
    {
        for (int i = 0; i < slotUIs.Length; i++)
        {
            InventoryItem uiItem = slotUIs[i].CurrentItem;
            items[i] = uiItem != null
                ? new InventoryItem(uiItem.itemData, uiItem.quantity)
                : null;
        }
    }
}
