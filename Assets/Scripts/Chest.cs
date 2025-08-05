using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{
    public List<InventoryItem> items = new List<InventoryItem>(new InventoryItem[20]); // 20 slot trống
    private bool isOpen = false;
    bool isPlayerNearby = false;
    float health;
    public GameObject chest;
    public void OpenChest()
    {
        ChestUIManager.Instance.OpenChest(this);
    }
    public void CloseChest()
    {
        ChestUIManager.Instance.CloseChest();
    }
    void Update()
    {
        health = chest.GetComponent<DropItem>().health;
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {

            if (!isOpen)
            {
                OpenChest();
                isOpen = true;
            }
            else
            {
                CloseChest();
                isOpen = false;
            }
        }
        if ((!isPlayerNearby && isOpen) || health <= 0)
        {
            CloseChest();
            isOpen = false;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }

}
