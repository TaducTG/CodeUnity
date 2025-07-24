using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryUIManager : MonoBehaviour
{
    [Header("References")]
    public GameObject inventoryPanel; // Gắn panel Inventory
    public Inventory inventory;       // Gắn script Inventory chứa List<InventoryItem>
    public Transform slotParent;      // Gắn đối tượng chứa tất cả các slot UI
    public GameObject slotPrefab;     // Prefab của từng slot (có script InventorySlotUI)

    private bool isOpen = false;
    private List<InventorySlotUI> slotUIs = new List<InventorySlotUI>();
    void Start()
    {
        InitSlots();
    }
    void InitSlots()
    {
        slotUIs.Clear();

        for (int i = 0; i < inventory.items.Count; i++)
        {
            GameObject slotGO = Instantiate(slotPrefab, slotParent);
            InventorySlotUI slotUI = slotGO.GetComponent<InventorySlotUI>();

            slotUI.inventory = inventory;
            slotUI.uiManager = this;
            slotUI.slotIndex = i;

            slotUI.SetItem(inventory.items[i]);

            slotUIs.Add(slotUI);
        }

        inventory.slotUIs = slotUIs.ToArray(); // Nếu Inventory.cs có mảng này
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
    }

    void ToggleInventory()
    {
        isOpen = !isOpen;
        inventoryPanel.SetActive(isOpen);

        if (isOpen)
        {
            RefreshInventoryUI();
            DragItemUI.Instance.Hide(); // 🔴 Ẩn icon kéo nếu có
        }
        else
        {
            DragItemUI.Instance.Hide(); // 🔴 Ẩn luôn khi đóng
        }
    }


    public void RefreshInventoryUI1()
    {
        // Xoá các slot UI cũ
        foreach (Transform child in slotParent)
        {
            Destroy(child.gameObject);
        }

        slotUIs.Clear();

        // Tạo lại slot UI mới từ inventory
        for (int i = 0; i < inventory.items.Count; i++)
        {
            GameObject slotGO = Instantiate(slotPrefab, slotParent);
            InventorySlotUI slotUI = slotGO.GetComponent<InventorySlotUI>();

            slotUI.inventory = inventory;
            slotUI.uiManager = this;
            slotUI.slotIndex = i; // ⭐ Gán đúng chỉ số slot
            slotUI.SetItem(inventory.items[i]);

            slotUIs.Add(slotUI);
        }
    }
    public void RefreshInventoryUI()
    {
        for (int i = 0; i < slotUIs.Count; i++)
        {
            slotUIs[i].SetItem(inventory.items[i]);
        }
    }


    // Hàm gọi sau mỗi lần hoán đổi hoặc gộp để đồng bộ từ UI về inventory data
    public void SyncSlotsToInventory()
    {
        for (int i = 0; i < slotUIs.Count; i++)
        {
            inventory.items[i] = slotUIs[i].CurrentItem;
        }
    }
    public int GetSlotIndex(InventorySlotUI slotUI)
    {
        return slotUIs.IndexOf(slotUI);
    }
}
