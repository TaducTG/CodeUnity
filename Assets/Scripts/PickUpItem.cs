using UnityEngine;
using static UnityEditor.Progress;

public class PickUpItem : MonoBehaviour
{
    public Items itemData; // Gán qua Inspector

    private bool hasPickedUp = false;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasPickedUp) return;

        if (collision.CompareTag("Player"))
        {
            hasPickedUp = true;

            Inventory inventory = collision.GetComponent<Inventory>();
            if (inventory != null)
            {
                inventory.AddItems(itemData,1);

                Destroy(gameObject); // Biến mất sau khi nhặt
            }
        }
    }
}
