using System.Collections.Generic;
using UnityEngine;

public class ChestUIManager : MonoBehaviour
{
    public static ChestUIManager Instance;

    public GameObject slotPrefab;
    public Transform chestSlotParent;
    public GameObject chestUI;

    private Chest currentChest;
    private List<InventorySlotUI> chestSlots = new List<InventorySlotUI>();

    void Awake()
    {
        Transform canvas = GameObject.Find("Canvas")?.transform;

        if (canvas == null)
        {
            Debug.LogError("Không tìm thấy Canvas!");
            return;
        }

        // Tìm root Crafting trong Canvas
        GameObject chestRoot = canvas.Find("Chest").gameObject;

        chestUI = chestRoot;


        Instance = this;
        chestUI.SetActive(false);
    }

    public void OpenChest(Chest chest)
    {
        currentChest = chest;
        chestUI.SetActive(true);
        GenerateChestSlots();
    }

    public void CloseChest()
    {
        chestUI.SetActive(false);
        foreach (Transform child in chestSlotParent)
        {
            Destroy(child.gameObject);
        }
        chestSlots.Clear();
        currentChest = null;
    }

    private void GenerateChestSlots()
    {
        for (int i = 0; i < currentChest.items.Count; i++)
        {
            GameObject slotGO = Instantiate(slotPrefab, chestSlotParent);
            InventorySlotUI slotUI = slotGO.GetComponent<InventorySlotUI>();

            slotUI.inventory = null;
            slotUI.externalItemList = currentChest.items;
            slotUI.slotIndex = i;

            slotUI.SetItem(currentChest.items[i]);
            chestSlots.Add(slotUI);
        }
    }
}
