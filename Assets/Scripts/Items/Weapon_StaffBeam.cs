using UnityEngine;

[CreateAssetMenu(fileName = "New Staff Beam", menuName = "Items/Weapon/StaffBeam")]
public class Weapon_StaffBeam : WeaponItem
{
    public float range = 10f;
    public float beamDuration = 0.1f;
    public GameObject beamVFXPrefab; // Prefab chứa LineRenderer hoặc hiệu ứng
    
    public override void ATK(GameObject user, GameObject mainHand)
    {

        Vector3 origin = user.transform.position;
        origin.z = 0f;

        Vector3 target = GetMouseWorldPosition();
        target.z = 0f;

        Vector3 direction = (target - origin).normalized;

        // Nếu chuột trùng đúng player thì direction sẽ = (0,0,0)
        if (direction == Vector3.zero)
        {
            direction = Vector3.up; // ép cho nó có hướng mặc định
        }
        int layerMask = ~LayerMask.GetMask("Default"); // bỏ qua layer Player
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, range, layerMask);
        Vector3 hitPoint;

        if (hit.collider != null)
        {
            hitPoint = hit.point;
        }
        else
        {
            hitPoint = origin + direction * range;
        }
        hitPoint.z = 0f;


        //Debug.Log($"Origin={origin}, Target={target}, HitPoint={hitPoint}, Dir={direction}");

        // Beam effect
        if (beamVFXPrefab != null)
        {
            GameObject beam = GameObject.Instantiate(beamVFXPrefab);
            LineRenderer lr = beam.GetComponent<LineRenderer>();
            if (lr != null)
            {
                lr.positionCount = 2;
                lr.SetPosition(0, origin);
                lr.SetPosition(1, hitPoint);

                // Gradient từ xanh (đầu) sang trắng (cuối)
                Gradient gradient = new Gradient();
                gradient.SetKeys(
                    new GradientColorKey[] {
            new GradientColorKey(Color.green, 0f),   // 0% chiều dài
            new GradientColorKey(Color.white, 1f)    // 100% chiều dài
                    },
                    new GradientAlphaKey[] {
            new GradientAlphaKey(1f, 0f),            // độ trong suốt ở đầu
            new GradientAlphaKey(1f, 1f)             // độ trong suốt ở cuối
                    }
                );
                lr.colorGradient = gradient;

                // Sorting để nó không bị che
                lr.sortingLayerName = "VFX";
                lr.sortingOrder = 10;
            }
            GameObject.Destroy(beam, beamDuration);
        }

        // Gợi ý thêm debug line để dễ thấy trên scene
        //Debug.DrawLine(origin, hitPoint, Color.red, 0.5f);
    }



    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        worldPos.z = 0f; // Fix cho game 2D
        return worldPos;
    }

}
