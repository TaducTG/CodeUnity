using UnityEngine;

public class TopDownDepthSort : MonoBehaviour
{
    void LateUpdate()
    {
        Vector3 pos = transform.position;
        pos.z = pos.y / 1000f;
        transform.position = pos;
    }
}
