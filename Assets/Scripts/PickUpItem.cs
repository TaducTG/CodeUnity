using UnityEngine;
using static UnityEditor.Progress;

public class PickUpItem : MonoBehaviour
{
    public Items itemData; // Gán qua Inspector
    public bool block;
    private bool hasPickedUp = false;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasPickedUp) return;

        if (collision.CompareTag("Player") && !block)
        {
            hasPickedUp = true;

            Inventory inventory = collision.GetComponent<Inventory>();
            if (inventory != null)
            {
                if (inventory.AddItems(itemData, 1)) // chỉ xóa nếu có thể nhặt vào balo
                {
                    Destroy(gameObject);// Biến mất sau khi nhặt
                }
  
            }
        }
    }
}
