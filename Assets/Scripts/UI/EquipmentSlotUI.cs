using UnityEngine;
using UnityEngine.EventSystems;

public class EquipmentSlotUI : InventorySlotUI
{
    public EquipmentSlotType slotType;
    private Player player;

    private void Start()
    {
        player = FindAnyObjectByType<Player>();
    }

    public override void OnDrop(PointerEventData eventData)
    {
        InventorySlotUI draggedSlot = eventData.pointerDrag?.GetComponent<InventorySlotUI>();
        if (draggedSlot == null || draggedSlot == this) return;

        InventoryItem draggedItem = draggedSlot.CurrentItem;
        if (draggedItem == null || draggedItem.itemData == null) return;

        // Chỉ nhận item là EquipmentItem và slotType khớp
        EquipmentItem equip = draggedItem.itemData as EquipmentItem;
        if (equip == null || equip.slotType != slotType)
        {
            Debug.Log("Item không phù hợp với ô trang bị này!");
            return;
        }

        // Nếu slot đang có trang bị → bỏ chỉ số cũ
        if (CurrentItem != null && CurrentItem.itemData is EquipmentItem oldEquip)
        {
            oldEquip.RemoveStats(player);
        }

        // Đặt item mới → cộng chỉ số
        SetItem(draggedItem);
        equip.ApplyStats(player);

        // Xóa slot gốc
        draggedSlot.ClearSlot();
    }

    public override void ClearSlot()
    {
        if (CurrentItem != null && CurrentItem.itemData is EquipmentItem equip)
        {
            equip.RemoveStats(player);
        }
        base.ClearSlot();
    }
}
