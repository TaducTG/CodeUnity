using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class FixedBiomeArea : MonoBehaviour
{
    
    public Tilemap tilemap; // chính là tilemap bạn dùng để vẽ vùng cố định

    public List<Vector2Int> GetLocalTilePositions()
    {
        List<Vector2Int> positions = new();
        BoundsInt bounds = tilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cell = new(x, y, 0);
                if (tilemap.HasTile(cell))
                {
                    positions.Add((Vector2Int)cell);
                }
            }
        }

        return positions;
    }
}
