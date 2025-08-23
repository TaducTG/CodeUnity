using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestUI : MonoBehaviour
{
    [Header("UI References")]
    public Transform requirementsGrid; // Chứa các slot item
    public Transform rewardGrid;
    public GameObject questItemSlotPrefab; // Prefab icon + số lượng
    

    public void ShowQuest(QuestData quest)
    {
        // Tìm Inventory của Player
        Inventory playerInventory = GameObject.FindGameObjectWithTag("Player").GetComponent<Inventory>();

        // 1. Hiển thị đoạn hội thoại

        // 2. Xóa slot cũ
        deleteSlot();

        // 3. Tạo slot mới
        foreach (var target in quest.requiredItems)
        {
            GameObject slotObj = Instantiate(questItemSlotPrefab, requirementsGrid);

            InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();
            if (slotUI == null)
            {
                Debug.LogWarning("Prefab thiếu InventorySlotUI component");
                continue;
            }

            // Gán icon
            if (slotUI.iconImage != null)
                slotUI.iconImage.sprite = target.items.icon;

            // Gán text + màu
            int have = playerInventory.GetTotalItem(target.items);
            if (slotUI.quantityText != null)
            {
                slotUI.quantityText.text = $"{have}/{target.amount}";
                slotUI.quantityText.color = (have >= target.amount) ? Color.green : Color.red;
            }
        }
        // 4. Hiển thị phần thưởng
        foreach (var reward in quest.rewardItems)
        {
            GameObject slotObj = Instantiate(questItemSlotPrefab, rewardGrid);

            InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();
            if (slotUI == null)
            {
                Debug.LogWarning("Prefab thiếu InventorySlotUI component");
                continue;
            }

            // Gán icon
            if (slotUI.iconImage != null)
                slotUI.iconImage.sprite = reward.items.icon;

            // Gán số lượng (luôn màu trắng vì đây là phần thưởng)
            if (slotUI.quantityText != null)
            {
                slotUI.quantityText.text = reward.amount.ToString();
                slotUI.quantityText.color = Color.white;
            }
        }

    }
    public void deleteSlot()
    {
        foreach (Transform child in requirementsGrid)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in rewardGrid)
        {
            Destroy(child.gameObject);
        }
    }
}
