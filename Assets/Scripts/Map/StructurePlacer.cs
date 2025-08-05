using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class StructurePlacer : MonoBehaviour
{
    // Bảng đánh dấu các ô đã bị dùng (tránh bị đè)
    private readonly HashSet<Vector3Int> reservedCells = new HashSet<Vector3Int>();

    // ==== Public API ====

    /// <summary>Đánh dấu trước các cell đã có object khác (cây, đá…) để tránh bị đè.</summary>
    public void ReserveCell(Vector3Int cell) => reservedCells.Add(cell);

    public void ReserveCells(IEnumerable<Vector3Int> cells)
    {
        foreach (var c in cells) reservedCells.Add(c);
    }

    public bool IsReserved(Vector3Int cell) => reservedCells.Contains(cell);

    /// <summary>Thử đặt structure tại originCell. Trả về true nếu đặt thành công.</summary>
    public bool TryPlaceStructureAt(Vector3Int originCell, StructureDefinition def)
    {
        if (!ValidateDefinition(def)) return false;

        var footprint = GetFootprintCells(originCell, def.size);

        // 1) Kiểm tra tile hợp lệ và trong biên
        if (!AreCellsOnAllowedTiles(footprint, def.groundTilemap, def.allowedTiles))
            return false;

        // 2) Không bị chiếm
        if (!AreCellsFree(footprint))
            return false;

        // 3) (tuỳ chọn) kiểm tra collider
        if (def.checkPhysicsOverlap && IsPhysicsBlocked(footprint, def.groundTilemap, def.colliderCheckSize, def.blockMask))
            return false;

        // === Đặt nhà (shell) ===
        Vector3 worldCenter = GetFootprintWorldCenter(originCell, def, def.groundTilemap);
        GameObject shell = null;
        if (def.shellPrefab != null)
        {
            shell = Instantiate(def.shellPrefab, worldCenter + def.shellPivotOffset, Quaternion.identity);
        }

        // === Đặt nội thất ===
        foreach (var it in def.interiors)
        {
            var itemCell = originCell + new Vector3Int(it.localCell.x, it.localCell.y, 0);
            // (nội thất vẫn “nằm” trong footprint nên về logic tile & reserve đã OK)
            Vector3 world = def.groundTilemap.CellToWorld(itemCell) + new Vector3(0.5f, 0.5f, 0) + it.localOffset;
            if (it.prefab != null)
            {
                var go = Instantiate(it.prefab, world, Quaternion.identity);
                if (shell != null) go.transform.SetParent(shell.transform, true); // tuỳ bạn có muốn parent vào shell
            }
        }

        // === Đánh dấu các ô đã dùng ===
        ReserveCells(footprint);

        return true;
    }

    /// <summary>Đặt structure ngẫu nhiên trong bounds tilemap, thử tối đa 'attempts' lần.</summary>
    public bool TryPlaceStructureRandom(StructureDefinition def, int attempts = 200)
    {
        if (!ValidateDefinition(def)) return false;

        var bounds = def.groundTilemap.cellBounds;

        for (int i = 0; i < attempts; i++)
        {
            int rx = Random.Range(bounds.xMin, bounds.xMax - def.size.x + 1);
            int ry = Random.Range(bounds.yMin, bounds.yMax - def.size.y + 1);
            Vector3Int origin = new Vector3Int(rx, ry, 0);

            if (TryPlaceStructureAt(origin, def))
                return true;
        }
        return false;
    }

    // ==== Helpers ====

    bool ValidateDefinition(StructureDefinition def)
    {
        if (def == null)
        {
            Debug.LogWarning("StructureDefinition null.");
            return false;
        }
        if (def.groundTilemap == null)
        {
            Debug.LogWarning($"[{def.id}] groundTilemap is null.");
            return false;
        }
        if (def.allowedTiles == null || def.allowedTiles.Length == 0)
        {
            Debug.LogWarning($"[{def.id}] allowedTiles is empty.");
            return false;
        }
        if (def.size.x <= 0 || def.size.y <= 0)
        {
            Debug.LogWarning($"[{def.id}] size must be > 0.");
            return false;
        }
        return true;
    }

    List<Vector3Int> GetFootprintCells(Vector3Int origin, Vector2Int size)
    {
        var list = new List<Vector3Int>(size.x * size.y);
        for (int dx = 0; dx < size.x; dx++)
        {
            for (int dy = 0; dy < size.y; dy++)
            {
                list.Add(new Vector3Int(origin.x + dx, origin.y + dy, 0));
            }
        }
        return list;
    }

    bool AreCellsOnAllowedTiles(List<Vector3Int> cells, Tilemap ground, TileBase[] allowed)
    {
        var allowedSet = new HashSet<TileBase>(allowed);
        var bounds = ground.cellBounds;

        foreach (var c in cells)
        {
            if (!bounds.Contains(c)) return false;      // ngoài biên tilemap
            var tile = ground.GetTile(c);
            if (tile == null) return false;            // ô trống
            if (!allowedSet.Contains(tile)) return false; // không phải tile cho phép
        }
        return true;
    }

    bool AreCellsFree(List<Vector3Int> cells)
    {
        foreach (var c in cells)
        {
            if (reservedCells.Contains(c)) return false;
        }
        return true;
    }

    bool IsPhysicsBlocked(List<Vector3Int> cells, Tilemap ground, Vector2 checkSize, LayerMask blockMask)
    {
        foreach (var c in cells)
        {
            Vector3 world = ground.CellToWorld(c) + new Vector3(0.5f, 0.5f, 0);
            var hits = Physics2D.OverlapBoxAll(world, checkSize, 0, blockMask);
            if (hits != null && hits.Length > 0) return true;
        }
        return false;
    }

    Vector3 GetFootprintWorldCenter(Vector3Int originCell, StructureDefinition def, Tilemap ground)
    {
        // Tâm footprint = origin + (size-1)/2 + 0.5 (đưa tâm ô)
        // Sau đó convert qua World bằng CellToWorld (ổn nếu Grid scale khác 1)
        float cx = originCell.x + (def.size.x - 1) * 0.5f;
        float cy = originCell.y + (def.size.y - 1) * 0.5f;
        Vector3Int centerCell = new Vector3Int(Mathf.RoundToInt(cx), Mathf.RoundToInt(cy), 0);
        // Đưa ra world tâm ô centerCell
        // Để chính xác về nửa ô, cộng thêm (0.5, 0.5)
        Vector3 world = ground.CellToWorld(centerCell) + new Vector3(0.5f, 0.5f, 0);
        return world;
    }
}
[System.Serializable]
public class InteriorItem
{
    public GameObject prefab;
    public Vector2Int localCell;     // toạ độ ô tương đối trong footprint (0,0) = góc trái-dưới
    public Vector3 localOffset;      // tinh chỉnh vị trí world nhỏ (nếu cần)
}

[System.Serializable]
public class StructureDefinition
{
    public string id = "House";
    public GameObject shellPrefab;      // prefab vỏ ngoài (toà nhà)
    public Vector2Int size = new Vector2Int(4, 3); // footprint w×h (theo +X, +Y)

    public Tilemap groundTilemap;       // tilemap nền (vd: grasslandTilemap)
    public TileBase[] allowedTiles;     // các tile cho phép (vd: grassTiles)

    public bool checkPhysicsOverlap = true; // kiểm tra collider chặn (cây/đá…)
    public Vector2 colliderCheckSize = new Vector2(0.95f, 0.95f);
    public LayerMask blockMask;         // layer chặn (các object có collider)

    public Vector3 shellPivotOffset = Vector3.zero; // chỉnh pivot nếu prefab shell không ở giữa

    public List<InteriorItem> interiors = new List<InteriorItem>(); // nội thất
}
