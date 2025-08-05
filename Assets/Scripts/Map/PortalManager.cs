using System.Collections.Generic;
using UnityEngine;

public class TeleportManager : MonoBehaviour
{
    public static TeleportManager Instance;

    private Dictionary<string, TeleportPortal> portalRegistry = new Dictionary<string, TeleportPortal>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject); // Nếu bạn qua scene
    }

    public void RegisterPortal(string id, TeleportPortal portal)
    {
        if (portalRegistry.ContainsKey(id))
        {
            // Đã có 1 cổng trùng ID → gán đối xứng
            var otherPortal = portalRegistry[id];
            if (otherPortal != null)
            {
                portal.destination = otherPortal.transform;
                otherPortal.destination = portal.transform;
                Debug.Log($"Cổng {id} đã được kết nối hai chiều.");
            }
        }
        else
        {
            portalRegistry[id] = portal;
        }
    }
}
