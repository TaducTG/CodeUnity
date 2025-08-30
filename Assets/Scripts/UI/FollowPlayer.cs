using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class FollowPlayer : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public Vector3 offset = new Vector3(0, 0, -10);

    [Header("Tilemap Bounds")]
    public Tilemap[] tilemaps;

    private Camera cam;
    private Tilemap currentTilemap;

    // Cache sẵn worldBounds của từng tilemap
    private Dictionary<Tilemap, Bounds> tilemapBounds = new Dictionary<Tilemap, Bounds>();

    void Start()
    {
        cam = Camera.main;

        // Tính sẵn bounds cho tất cả tilemap
        foreach (Tilemap tm in tilemaps)
        {
            tilemapBounds[tm] = CalculateWorldBounds(tm);
        }
    }

    void LateUpdate()
    {
        if (target == null || cam == null) return;

        // Tìm tilemap hiện tại mà player đứng trên
        currentTilemap = GetTilemapAtPlayer();

        Vector3 desiredPos = target.position + offset;

        if (currentTilemap != null && tilemapBounds.ContainsKey(currentTilemap))
        {
            Bounds worldBounds = tilemapBounds[currentTilemap];

            float camHeight = cam.orthographicSize;
            float camWidth = camHeight * cam.aspect;

            float minX = worldBounds.min.x + camWidth;
            float maxX = worldBounds.max.x - camWidth;
            float minY = worldBounds.min.y + camHeight;
            float maxY = worldBounds.max.y - camHeight;

            desiredPos.x = Mathf.Clamp(desiredPos.x, minX, maxX);
            desiredPos.y = Mathf.Clamp(desiredPos.y, minY, maxY);
        }

        transform.position = desiredPos;
    }

    Tilemap GetTilemapAtPlayer()
    {
        Vector3 playerWorldPos = target.position;

        foreach (Tilemap tm in tilemaps)
        {
            Vector3Int cellPos = tm.WorldToCell(playerWorldPos);
            if (tm.HasTile(cellPos))
            {
                return tm;
            }
        }
        return null;
    }

    Bounds CalculateWorldBounds(Tilemap tilemap)
    {
        BoundsInt cellBounds = tilemap.cellBounds;
        bool foundFirst = false;
        Vector3 min = Vector3.zero;
        Vector3 max = Vector3.zero;

        foreach (var pos in cellBounds.allPositionsWithin)
        {
            if (tilemap.HasTile(pos))
            {
                Vector3 worldMin = tilemap.CellToWorld(pos);
                Vector3 worldMax = worldMin + tilemap.cellSize;

                if (!foundFirst)
                {
                    min = worldMin;
                    max = worldMax;
                    foundFirst = true;
                }
                else
                {
                    min = Vector3.Min(min, worldMin);
                    max = Vector3.Max(max, worldMax);
                }
            }
        }

        Bounds worldBounds = new Bounds();
        worldBounds.SetMinMax(min, max);
        return worldBounds;
    }
}
