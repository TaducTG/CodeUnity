using System.Collections.Generic;
using UnityEngine;

public enum QuestType { CollectItem, KillEnemy, Talk }

[System.Serializable]
public class QuestItemRequirement
{
    public Items items; // Item ScriptableObject
    public int amount;
}

[CreateAssetMenu(fileName = "New Quest", menuName = "Quests/Quest")]
public class QuestData : ScriptableObject
{
    public string questName;

    [Header("Dialogue")]
    public List<string> startDialogue;     // Danh sách câu khi bắt đầu
    public List<string> inProgressDialogue;
    public List<string> completeDialogue;  // Danh sách câu khi hoàn thành

    public QuestType questType;

    // Nếu là nhiệm vụ CollectItem
    public QuestItemRequirement[] requiredItems;

    // Nếu là nhiệm vụ KillEnemy
    public string targetEnemyID;
    public int targetEnemyAmount;

    public QuestItemRequirement[] rewardItems;
}
