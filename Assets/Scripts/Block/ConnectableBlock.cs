using UnityEngine;

[System.Flags]
public enum NeighborDirection
{
    None = 0,
    Top = 1 << 0,
    Bottom = 1 << 1,
    Left = 1 << 2,
    Right = 1 << 3
}

public class ConnectableBlock : MonoBehaviour
{
    public string blockID = "Wood"; // định danh loại block
    public Sprite[] connectionSprites; // sprite tương ứng với từng tổ hợp kết nối
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
       
        Items item = gameObject.GetComponent<PickUpItem>().itemData;
        UpdateConnection(item);
    }

    public void UpdateConnection(Items item)
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

        }
        NeighborDirection connection = NeighborDirection.None;
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right};
        NeighborDirection[] flags = { NeighborDirection.Top, NeighborDirection.Bottom, NeighborDirection.Left, NeighborDirection.Right };

        for (int i = 0; i < directions.Length; i++)
        {
            Vector2 checkPos = (Vector2)transform.position + directions[i];
            //Debug.Log($"[DEBUG] Đang kiểm tra vị trí hàng xóm: {checkPos} từ block tại {transform.position}");
            Collider2D hit = Physics2D.OverlapPoint(checkPos);
            if (hit != null)
            {
                //Debug.Log($"[DEBUG] Đã tìm thấy object tại {checkPos}: {hit.gameObject.name}");
                ConnectableBlock otherBlock = hit.GetComponent<ConnectableBlock>();
                PickUpItem otherItemComp = hit.GetComponent<PickUpItem>();

                if (otherBlock != null && otherItemComp != null)
                {
                    Items otherItem = otherItemComp.itemData;
                    if (otherItem != null && otherItem == item)
                    {
                       
                        connection |= flags[i];
                    }

                }
            }
        }

        int index = (int)connection;
        

        if (index >= 0 && index < connectionSprites.Length)
        {
            if (connectionSprites[index] == null)
            {
                Debug.LogWarning("Sprite tại index " + index + " là NULL trong connectionSprites.");
            }
            else
            {
                spriteRenderer.sprite = connectionSprites[index];
            }
        }
        else
        {
            Debug.LogWarning($"Index {index} nằm ngoài phạm vi mảng connectionSprites (Length: {connectionSprites.Length})");
        }
    }

    public void UpdateConnection(Items item, Vector2Int gridPos, int[,] map)
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        NeighborDirection connection = NeighborDirection.None;

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        NeighborDirection[] flags = { NeighborDirection.Top, NeighborDirection.Bottom, NeighborDirection.Left, NeighborDirection.Right };

        for (int i = 0; i < directions.Length; i++)
        {
            Vector2Int checkPos = gridPos + directions[i];

            // check trong phạm vi map
            if (checkPos.x >= 0 && checkPos.x < map.GetLength(0) &&
                checkPos.y >= 0 && checkPos.y < map.GetLength(1))
            {
                if (map[checkPos.x, checkPos.y] == 1) // wall tồn tại
                {
                    connection |= flags[i];
                }
            }
        }

        int index = (int)connection;
        if (index >= 0 && index < connectionSprites.Length)
        {
            spriteRenderer.sprite = connectionSprites[index];
        }
        else
        {
            Debug.LogWarning($"[ConnectableBlock] Index {index} out of range (sprites length {connectionSprites.Length})");
        }
    }


    public void UpdateNeighbors(Vector2 pos, Items item, bool destroy = false)
    {
        if (destroy)
        {
            Destroy(gameObject);
        }
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        foreach (var dir in directions)
        {
            Collider2D hit = Physics2D.OverlapPoint(pos + dir);
            if (hit != null)
            {
                var connectable = hit.GetComponent<ConnectableBlock>();
                var pickUp = hit.GetComponent<PickUpItem>();

                if (connectable != null && pickUp != null && pickUp.itemData == item)
                {
                    connectable.UpdateConnection(item);
                }
            }
        }
    }

}
