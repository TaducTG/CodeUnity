// Attach this to a CameraController GameObject (not the camera itself if needed)
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 0, -10);

    void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }
}
