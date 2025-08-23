using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [SerializeField] private Inventory playerInventory; // Kéo Inventory của người chơi vào Inspector
    private List<QuestData> activeQuests = new List<QuestData>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    public bool HasQuest(QuestData quest)
    {
        return activeQuests.Contains(quest);
    }
    // Nhận quest mới
    public void AddQuest(QuestData quest)
    {
        if (!activeQuests.Contains(quest))
        {
            activeQuests.Add(quest);
            Debug.Log($"Quest '{quest.questName}' đã được nhận.");
        }
    }

    // Kiểm tra tiến độ tất cả quest đang làm
    public void CheckQuestsProgress()
    {
        foreach (QuestData quest in activeQuests)
        {
            if (IsQuestCompleted(quest))
            {
                CompleteQuest(quest);
            }
        }
    }

    // Xác định quest đã hoàn thành hay chưa
    public bool IsQuestCompleted(QuestData quest)
    {
        foreach (var target in quest.requiredItems) // Mảng các item cần thu thập
        {
            int have = playerInventory.GetTotalItem(target.items);
            if (have < target.amount)
                return false; // Nếu thiếu bất kỳ item nào => chưa xong
        }
        return true;
    }

    // Hoàn thành quest
    public void CompleteQuest(QuestData quest)
    {
        Debug.Log($"Quest '{quest.questName}' đã hoàn thành!");

        // Trừ các item yêu cầu khỏi Inventory
        foreach (var target in quest.requiredItems)
        {
            playerInventory.RemoveItem(target.items, target.amount);
        }

        foreach(var target in quest.rewardItems)
        {
            playerInventory.AddItems(target.items, target.amount);
        }

        activeQuests.Remove(quest);
    }
}
