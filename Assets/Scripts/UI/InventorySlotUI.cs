using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
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

    public InventoryItem CurrentItem { get; private set; }

    public Inventory inventory;

    public int slotIndex;
    private void Awake()
    {
        uiManager = FindAnyObjectByType<InventoryUIManager>();
        inventory = FindAnyObjectByType<Inventory>();

        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        canvas = GetComponentInParent<Canvas>();
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
        if (slotIndex >= 0 && slotIndex < inventory.items.Count)
        {
            inventory.items[slotIndex] = CurrentItem;
        }
    }

    public void ClearSlot()
    {
        CurrentItem = null;
        iconImage.sprite = null;
        iconImage.enabled = false;
        quantityText.text = "";
        quantityText.enabled = false;

        // Luôn cập nhật inventory.items theo slotIndex
        if (slotIndex >= 0 && slotIndex < inventory.items.Count)
        {
            inventory.items[slotIndex] = CurrentItem;
        }
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

    public void OnDrop(PointerEventData eventData)
    {
        InventorySlotUI draggedSlot = eventData.pointerDrag?.GetComponent<InventorySlotUI>();
        if (draggedSlot == null || draggedSlot == this) return;

        InventoryItem draggedItem = draggedSlot.CurrentItem;
        InventoryItem targetItem = this.CurrentItem;

        // Nếu slot này rỗng → chuyển thẳng
        if (targetItem == null)
        {
            SetItem(draggedItem);
            draggedSlot.ClearSlot();
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
