using UnityEngine;
[CreateAssetMenu(fileName = "New Tools", menuName = "Items/Tools")]
public class Tools : Items
{
    public float damage;
    public float speed;
    public float tier;

    [Header("FarmTools")]
    public GameObject farmLand;
    public void UseFarmTool(Vector3 mouseWorldPos, MapGenerator mapGenerator)
    {
        if (farmLand == null)
        {
            Debug.LogWarning("FarmTool has no farmLand prefab assigned!");
            return;
        }
        if (mapGenerator == null)
        {
            Debug.LogError("MapGenerator chưa được truyền vào UseFarmTool!");
            return;
        }

        // --- Snap theo lưới ---
        mouseWorldPos.z = 0f;
        Vector3Int cellPos = mapGenerator.grasslandTilemap.WorldToCell(mouseWorldPos);
        Vector3 placementPos = mapGenerator.grasslandTilemap.GetCellCenterWorld(cellPos);

        // --- Kiểm tra tile có farm được không ---
        if (!mapGenerator.CanFarmAt(cellPos))
        {
            Debug.Log("Không thể farm ở ô này (chỉ cho phép Grass hoặc Ground).");
            return;
        }

        // --- Kiểm tra đã có farmland chưa ---
        Collider2D hit = Physics2D.OverlapCircle((Vector2)placementPos, 0.2f);
        if (hit != null && hit.CompareTag("Farm_land"))
        {
            Debug.Log("Ô này đã có farmland rồi!");
            return;
        }

        // --- Spawn farmland ---
        GameObject spawned = GameObject.Instantiate(farmLand, placementPos, Quaternion.identity);
        Vector3 pos = spawned.transform.position;
        pos.z = 9f; // đặt z cố định
        spawned.transform.position = pos;

        Debug.Log("Tạo farmland thành công tại " + cellPos);
    }
}
