using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum QuestDialogueState
{
    Start,
    InProgress,
    Complete
}

public class QuestGiver : MonoBehaviour
{
    [Header("Quest Settings")]
    public QuestData questData;

    [Header("UI References")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public QuestUI questUI;

    [Header("Interaction Settings")]
    public KeyCode interactKey = KeyCode.E;      // Mở / đóng
    public KeyCode nextLineKey = KeyCode.Space;  // Chuyển câu
    public float interactDistance = 2f;

    private Transform player;
    private List<string> currentDialogue;
    private int dialogueIndex;
    private QuestDialogueState dialogueState;
    private bool dialogueOpen = false;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        dialoguePanel.SetActive(false);
    }

    private void Update()
    {
        float distance = Vector2.Distance(player.position, transform.position);

        // Nhấn E để mở/đóng khi đứng gần NPC
        if (distance <= interactDistance && Input.GetKeyDown(interactKey))
        {
            if (!dialogueOpen)
                OpenDialogue();
            else
                CloseDialogue();
        }

        // Nhấn Space để chuyển câu nếu panel đang mở
        if (dialogueOpen && Input.GetKeyDown(nextLineKey))
        {
            NextDialogue();
        }
    }

    private void OpenDialogue()
    {
        bool hasQuest = QuestManager.Instance.HasQuest(questData);
        //bool completed = QuestManager.Instance.IsQuestCompleted(questData);

        if (!hasQuest)
        {
            dialogueState = QuestDialogueState.Start;
            currentDialogue = questData.startDialogue;
        }
        //else if (completed)
        //{
        //    dialogueState = QuestDialogueState.Complete;
        //    currentDialogue = questData.completeDialogue;
        //}
        else
        {
            dialogueState = QuestDialogueState.InProgress;
            currentDialogue = questData.inProgressDialogue;
        }

        dialogueIndex = 0;
        dialoguePanel.SetActive(true);
        dialogueText.text = currentDialogue[dialogueIndex];
        dialogueOpen = true;

        if (dialogueState == QuestDialogueState.InProgress)
        {
            questUI.deleteSlot();
            questUI.ShowQuest(questData);
        }
    }

    private void NextDialogue()
    {
        dialogueIndex++;

        if (dialogueIndex < currentDialogue.Count)
        {
            // Vẫn còn câu thoại -> hiển thị tiếp
            dialogueText.text = currentDialogue[dialogueIndex];
        }
        else
        {
            if (dialogueState == QuestDialogueState.Start)
            {
                // Nhận quest
                QuestManager.Instance.AddQuest(questData);

                // Hiển thị quest yêu cầu item
                questUI.ShowQuest(questData);

                // Chuyển sang in-progress dialogue
                dialogueState = QuestDialogueState.InProgress;
                currentDialogue = questData.inProgressDialogue;
                dialogueIndex = 0;
                dialogueText.text = currentDialogue[dialogueIndex];
            }
            else if (dialogueState == QuestDialogueState.Complete)
            {
                // Nếu đang ở CompleteDialogue và vừa kết thúc câu cuối -> đóng luôn
                HandleDialogueEnd();
                CloseDialogue();
                questUI.deleteSlot();
            }
            else
            {
                // InProgress hoặc các trạng thái khác -> đóng bình thường
                HandleDialogueEnd();
                if (QuestManager.Instance.IsQuestCompleted(questData))
                {
                    dialogueState = QuestDialogueState.Complete;
                    currentDialogue = questData.completeDialogue;
                    dialogueIndex = 0;
                    dialogueText.text = currentDialogue[dialogueIndex];
                }
                //CloseDialogue();
            }
        }

        if(dialogueState == QuestDialogueState.InProgress)
        {
            questUI.deleteSlot();
            questUI.ShowQuest(questData);
        }
    }


    private void HandleDialogueEnd()
    {
        switch (dialogueState)
        {
            case QuestDialogueState.Start:
                QuestManager.Instance.AddQuest(questData);
                questUI.ShowQuest(questData);
                break;

            case QuestDialogueState.InProgress:
                questUI.ShowQuest(questData);
                break;

            case QuestDialogueState.Complete:
                QuestManager.Instance.CompleteQuest(questData);
                break;
        }
    }

    private void CloseDialogue()
    {
        dialoguePanel.SetActive(false);
        dialogueOpen = false;
        questUI.deleteSlot();
    }
}
