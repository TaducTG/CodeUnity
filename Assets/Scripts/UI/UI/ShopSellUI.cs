using NUnit.Framework.Interfaces;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShopSellUI : MonoBehaviour, IDropHandler
{
    public Inventory playerInventory;
    public Items coinItemData; // coin cũng là 1 ItemData (giống các item khác)

    public void OnDrop(PointerEventData eventData)
    {
        InventorySlotUI draggedSlot = eventData.pointerDrag?.GetComponent<InventorySlotUI>();
        if (draggedSlot == null || draggedSlot.CurrentItem == null) return;

        InventoryItem sellingItem = draggedSlot.CurrentItem;

        // Tính giá bán = quantity * price
        int totalCoin = Mathf.RoundToInt(sellingItem.quantity * sellingItem.itemData.price);

        if (totalCoin <= 0) return;

        // Xóa item khỏi inventory
        draggedSlot.ClearSlot();
        playerInventory.items[draggedSlot.slotIndex] = null;

        // Thêm coin vào inventory
        playerInventory.AddItems(coinItemData, totalCoin);

        Debug.Log($"Đã bán {sellingItem.itemData.itemName} x{sellingItem.quantity} → +{totalCoin} coin");
    }
}
