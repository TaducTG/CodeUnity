using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EquipmentSlotUI : MonoBehaviour, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public EquipmentSlotType slotType;
    public InventoryItem CurrentItem;
    public Image iconImage;
    public Player player;

    public InventoryUIManager uiManager;

    // 👉 Danh sách các Equipment đang được trang bị
    public static Dictionary<EquipmentSlotType, EquipmentItem> equippedItems
        = new Dictionary<EquipmentSlotType, EquipmentItem>();

    void Awake()
    {
        player = FindAnyObjectByType<Player>();
        uiManager = FindAnyObjectByType<InventoryUIManager>();
    }

    public void SetItem(InventoryItem newItem)
    {
        CurrentItem = newItem;
        iconImage.sprite = (newItem != null) ? newItem.itemData.icon : null;
        iconImage.enabled = (newItem != null);

        // Cập nhật vào danh sách
        if (newItem != null && newItem.itemData is EquipmentItem equip)
        {
            equippedItems[slotType] = equip;
        }
        else
        {
            equippedItems.Remove(slotType);
        }
    }

    public void ClearSlot()
    {
        CurrentItem = null;
        iconImage.sprite = null;
        iconImage.enabled = false;

        // Xoá khỏi danh sách
        if (equippedItems.ContainsKey(slotType))
        {
            equippedItems.Remove(slotType);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (CurrentItem == null) return;
        DragItemUI.Instance.Show(CurrentItem.itemData.icon);
    }

    public void OnDrag(PointerEventData eventData) { }

    public void OnEndDrag(PointerEventData eventData)
    {
        DragItemUI.Instance.Hide();
    }

    public void OnDrop(PointerEventData eventData)
    {
        InventorySlotUI draggedSlot = eventData.pointerDrag?.GetComponent<InventorySlotUI>();
        if (draggedSlot == null) return;

        InventoryItem draggedItem = draggedSlot.CurrentItem;
        if (draggedItem == null) return;

        EquipmentItem equip = draggedItem.itemData as EquipmentItem;
        if (equip == null || equip.slotType != slotType)
            return;

        // Nếu đang có đồ → remove stat
        if (CurrentItem != null && CurrentItem.itemData is EquipmentItem oldEquip)
        {
            oldEquip.RemoveStats(player);
        }

        // Equip mới
        SetItem(draggedItem);
        equip.ApplyStats(player);

        // Xoá khỏi inventory slot
        draggedSlot.ClearSlot();

        uiManager.SyncEquipmentToInventory();
    }
}
