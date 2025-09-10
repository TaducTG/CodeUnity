using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Components")]
    public Image iconImage;
    public TextMeshProUGUI quantityText;

    [HideInInspector] public Transform originalParent;
    [HideInInspector] public Canvas canvas;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 originalAnchoredPosition;

    public InventoryUIManager uiManager;

    private InventoryItem currentItem;
    public InventoryItem CurrentItem
    {
        get => currentItem;
        set
        {
            currentItem = value;
            UpdateSlotUI();

            // Cập nhật vào Inventory nếu có slotIndex hợp lệ
            if (inventory != null && slotIndex >= 0 && slotIndex < inventory.items.Count)
            {
                inventory.items[slotIndex] = currentItem;
            }
            else if (externalItemList != null && slotIndex >= 0 && slotIndex < externalItemList.Count)
            {
                externalItemList[slotIndex] = currentItem;
            }
        }
    }



    public Inventory inventory;
    public List<InventoryItem> externalItemList; // null nếu là slot inventory
    public int slotIndex;

    private void Awake()
    {
        uiManager = FindAnyObjectByType<InventoryUIManager>();
        inventory = FindAnyObjectByType<Inventory>();

        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        canvas = GetComponentInParent<Canvas>();
    }
    public void UpdateSlotUI()
    {
        if (CurrentItem != null && CurrentItem.itemData != null)
        {
            iconImage.sprite = CurrentItem.itemData.icon;
            iconImage.enabled = true;

            if (CurrentItem.itemData.isStackable && CurrentItem.quantity > 1)
            {
                quantityText.text = CurrentItem.quantity.ToString();
            }
            else
            {
                quantityText.text = ""; // Không hiện số nếu chỉ có 1
            }
        }
        else
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
            quantityText.text = "";
        }
    }
    public void SetItem(InventoryItem newItem)
    {
        CurrentItem = newItem;

        if (CurrentItem != null && CurrentItem.itemData != null)
        {
            iconImage.sprite = CurrentItem.itemData.icon;
            iconImage.enabled = true;

            quantityText.text = CurrentItem.itemData.isStackable ? CurrentItem.quantity.ToString() : "";
            quantityText.enabled = true;
        }
        else
        {
            ClearSlot();
        }
        // Luôn cập nhật inventory.items theo slotIndex

    }

    public virtual void ClearSlot()
    {
        CurrentItem = null;
        iconImage.sprite = null;
        iconImage.enabled = false;
        quantityText.text = "";
        quantityText.enabled = false;

        // Luôn cập nhật inventory.items theo slotIndex

    }
    // === Hold ===

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CurrentItem != null)
        {
            ItemTooltipUI.Instance.Show(CurrentItem);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ItemTooltipUI.Instance.Hide();
    }

    // === DRAG ===
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (CurrentItem == null) return;
        DragItemUI.Instance.Show(CurrentItem.itemData.icon);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // icon đã di chuyển theo chuột ở DragItemUI.Update()
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        DragItemUI.Instance.Hide();
    }

    public virtual void OnDrop(PointerEventData eventData)
    {
        InventorySlotUI draggedSlot = eventData.pointerDrag?.GetComponent<InventorySlotUI>();

        EquipmentSlotUI draggedEquipSlot = eventData.pointerDrag?.GetComponent<EquipmentSlotUI>();

        if (draggedEquipSlot != null && draggedEquipSlot.CurrentItem != null)
        {
            // 👉 xử lý move từ Equipment về Inventory
            InventoryItem draggedEquipment = draggedEquipSlot.CurrentItem;

            // Bỏ stat nếu là equipment
            if (draggedEquipment.itemData is EquipmentItem oldEquip)
            {
                oldEquip.RemoveStats(draggedEquipSlot.player);
            }

            // Cho item vào inventory slot
            SetItem(draggedEquipment);
            draggedEquipSlot.ClearSlot();
            uiManager.SyncEquipmentToInventory();
        }

        if (draggedSlot == null || draggedSlot == this) return;

        InventoryItem draggedItem = draggedSlot.CurrentItem;
        InventoryItem targetItem = this.CurrentItem;

        // Nếu slot này rỗng → chuyển thẳng
        if (targetItem == null)
        {
            SetItem(draggedItem);
            draggedSlot.ClearSlot();
            return;
        }

        if (slotIndex == inventory.items.Count - 1)
        {
            // Nếu đang có item → xóa item cũ
            if (targetItem != null)
            {
                Debug.Log($"Item cũ '{targetItem.itemData.itemName}' đã bị xóa khỏi slot rác.");
            }

            // Ghi đè bằng item mới
            SetItem(draggedItem);
            inventory.items[slotIndex] = draggedItem;

            // Xóa khỏi slot gốc
            draggedSlot.ClearSlot();

            uiManager.SyncSlotsToInventory();
            return;
        }

        // Nếu cùng loại và stack được
        else if (draggedItem.itemData == targetItem.itemData && targetItem.itemData.isStackable)
        {
            int total = draggedItem.quantity + targetItem.quantity;
            int maxStack = targetItem.itemData.maxStack;

            int newAmount = Mathf.Min(total, maxStack);
            int leftover = total - newAmount;

            targetItem.quantity = newAmount;
            SetItem(targetItem);

            if (leftover > 0)
            {
                draggedItem.quantity = leftover;
                draggedSlot.SetItem(draggedItem);
            }
            else
            {
                draggedSlot.ClearSlot();
            }
        }
        // Không stack được → hoán đổi
        else
        {
            // ✅ Swap in UI
            SetItem(draggedItem);
            draggedSlot.SetItem(targetItem);

            // ✅ Swap in Data
            int indexA = slotIndex;
            int indexB = draggedSlot.slotIndex;

            InventoryItem temp = inventory.items[indexA];
            inventory.items[indexA] = inventory.items[indexB];
            inventory.items[indexB] = temp;
        }

        uiManager.SyncSlotsToInventory();
    }


}
