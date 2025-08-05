using UnityEngine.Tilemaps;
using UnityEngine;

[System.Serializable]
public class LargeObjectSpawnData
{
    public GameObject prefab;
    public Vector2Int size; // kích thước (width x height)
    public TileBase requiredTile;
    public int attempts = 100;
}
