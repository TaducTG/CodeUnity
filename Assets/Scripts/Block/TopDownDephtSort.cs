using UnityEngine;

public class TopDownDepthSort : MonoBehaviour
{
    void LateUpdate()
    {
        Vector3 pos = transform.position;
        pos.z = pos.y; // hoặc pos.z = -pos.y;
        transform.position = pos;
    }
}
