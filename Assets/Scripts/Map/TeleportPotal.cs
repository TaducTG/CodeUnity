using UnityEngine;

public class TeleportPortal : MonoBehaviour
{
    public string portalId; // Đặt trong prefab
    public Transform destination;

    private void Start()
    {
        if (!string.IsNullOrEmpty(portalId))
        {
            TeleportManager.Instance?.RegisterPortal(portalId, this);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && Input.GetKeyDown(KeyCode.E))
        {
            if (destination != null)
            {
                other.transform.position = destination.position;
            }
            else
            {
                Debug.LogWarning($"Portal {portalId} không có destination.");
            }
        }
    }
}
