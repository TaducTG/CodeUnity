using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{

    public enum BiomeType { Grassland, Desert, Swamp, Snow }
    [Header("Map Size & Noise")]
    public int width = 400;
    public int height = 400;
    public float noiseScale = 20f;

    [Header("Tilemap & Tile Variants")]
    public BiomeType biomeTilemap;
    private Tilemap grasslandTilemap;
    public Tilemap riverTilemap;

    public TileBase[] grassTiles;
    public TileBase[] dirtTiles;
    public TileBase[] stoneTiles;
    public TileBase riverTile;

    public int number = 2;
    public int riverWidth = 4; // Ngang
    public int riverHeight = 5; // Dọc
    [Header("Object Prefabs")]
    public GameObject[] treePrefabs;
    public GameObject[] rockPrefabs;
    public GameObject[] bushPrefabs;
    public GameObject[] flowerPrefabs;

    [Header("Large Objects")]
    public GameObject[] largeObjectPrefabs; // VD: Nhà
    public Vector2Int[] largeObjectSizes;   // Kích thước tương ứng từng prefab
    public TileBase requiredTileForLargeObject; // VD: grassTile
    public int maxAttemptsPerLargeObject = 50;


    [Header("Spawn Chance (0–1)")]
    public float flowerSpawnChance = 0.08f;
    public float treeSpawnChance = 0.05f;  // 5%
    public float rockSpawnChance = 0.03f;  // 3%
    public float bushSpawnChance = 0.15f;  // 2%


    // Internal
    private bool[,] occupied;
    private BoundsInt bounds;

    public BiomeMapGenerator biomeMapGenerator;

    void Start()
    {
        StartCoroutine(InitAfterBiomeGenerated());
    }

    IEnumerator InitAfterBiomeGenerated()
    {
        yield return new WaitUntil(() => biomeMapGenerator.isGenerated);

        if (biomeTilemap == BiomeType.Grassland)
            grasslandTilemap = biomeMapGenerator.grasslandTilemap;
        else if (biomeTilemap == BiomeType.Desert)
            grasslandTilemap = biomeMapGenerator.desertTilemap;

        if (grasslandTilemap == null) yield break;


        ApplyNoiseToTiles(grasslandTilemap);
        PrepareOccupiedGrid();

        TrySpawnLargeObjects();


        SpawnSmallObjects();
    }

    private void ApplyNoiseToTiles(Tilemap targetTilemap)
    {
        if (targetTilemap == null) return;

        float noiseScale = 10f;
        BoundsInt bounds = targetTilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);
                TileBase currentTile = targetTilemap.GetTile(tilePos);
                if (currentTile == null) continue;

                float noiseValue = Mathf.PerlinNoise(x / noiseScale, y / noiseScale);
                TileBase selectedTile = null;
                if (noiseValue < 0.35f && stoneTiles.Length > 0)
                    selectedTile = stoneTiles[Random.Range(0, stoneTiles.Length)];
                else if (noiseValue < 0.40f && dirtTiles.Length > 0)
                    selectedTile = dirtTiles[Random.Range(0, dirtTiles.Length)];
                else if (grassTiles.Length > 0)
                    selectedTile = grassTiles[Random.Range(0, grassTiles.Length)];

                if (selectedTile != null)
                    targetTilemap.SetTile(tilePos, selectedTile);
            }
        }
    }

    private void TrySpawnLargeObjects()
    {
        if (largeObjectPrefabs.Length != largeObjectSizes.Length)
        {
            Debug.LogError("Prefab và kích thước không khớp!");
            return;
        }

        for (int i = 0; i < largeObjectPrefabs.Length; i++)
        {
            GameObject prefab = largeObjectPrefabs[i];
            Vector2Int size = largeObjectSizes[i];

            for (int attempt = 0; attempt < maxAttemptsPerLargeObject; attempt++)
            {
                int localX = Random.Range(0, bounds.size.x - size.x);
                int localY = Random.Range(0, bounds.size.y - size.y);
                Vector3Int baseCell = new Vector3Int(bounds.x + localX, bounds.y + localY, 0);

                bool canPlace = true;

                for (int dx = 0; dx < size.x && canPlace; dx++)
                {
                    for (int dy = 0; dy < size.y && canPlace; dy++)
                    {
                        int ix = localX + dx;
                        int iy = localY + dy;
                        Vector3Int checkPos = new Vector3Int(baseCell.x + dx, baseCell.y + dy, 0);

                        if (ix >= occupied.GetLength(0) || iy >= occupied.GetLength(1)) { canPlace = false; break; }
                        if (occupied[ix, iy]) { canPlace = false; break; }
                        if (grasslandTilemap.GetTile(checkPos) != requiredTileForLargeObject) { canPlace = false; break; }
                    }
                }

                if (canPlace)
                {
                    Vector3 worldPos = grasslandTilemap.CellToWorld(baseCell) + new Vector3(size.x / 2f, size.y / 2f, 0);
                    Instantiate(prefab, worldPos, Quaternion.identity);

                    for (int dx = 0; dx < size.x; dx++)
                        for (int dy = 0; dy < size.y; dy++)
                            occupied[localX + dx, localY + dy] = true;

                    break;
                }
            }
        }
    }

    private void SpawnSmallObjects()
    {
        if (grasslandTilemap == null) return;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                int localX = x - bounds.xMin;
                int localY = y - bounds.yMin;

                if (occupied[localX, localY]) continue;

                Vector3Int tilePos = new Vector3Int(x, y, 0);
                TileBase currentTile = grasslandTilemap.GetTile(tilePos);
                if (currentTile == null) continue;

                Vector3 worldPos = grasslandTilemap.CellToWorld(tilePos) + new Vector3(0.5f, 0.5f, 0);

                // TREE & FLOWER — chỉ spawn trên grass tiles
                if (IsTileInList(currentTile, grassTiles))
                {
                    if (treePrefabs.Length > 0 && Random.value < treeSpawnChance)
                    {
                        Instantiate(treePrefabs[Random.Range(0, treePrefabs.Length)], worldPos, Quaternion.identity);
                        occupied[localX, localY] = true;
                        continue;
                    }
                    else if (flowerPrefabs.Length > 0 && Random.value < flowerSpawnChance)
                    {
                        Instantiate(flowerPrefabs[Random.Range(0, flowerPrefabs.Length)], worldPos, Quaternion.identity);
                        occupied[localX, localY] = true;
                        continue;
                    }
                }

                // ROCK — chỉ spawn trên stone tiles
                else if (IsTileInList(currentTile, stoneTiles))
                {
                    if (rockPrefabs.Length > 0 && Random.value < rockSpawnChance)
                    {
                        Instantiate(rockPrefabs[Random.Range(0, rockPrefabs.Length)], worldPos, Quaternion.identity);
                        occupied[localX, localY] = true;
                        continue;
                    }
                }

                // BUSH — chỉ spawn trên grass hoặc dirt
                else if (IsTileInList(currentTile, dirtTiles) || IsTileInList(currentTile, grassTiles))
                {
                    if (bushPrefabs.Length > 0 && Random.value < bushSpawnChance)
                    {
                        Instantiate(bushPrefabs[Random.Range(0, bushPrefabs.Length)], worldPos, Quaternion.identity);
                        occupied[localX, localY] = true;
                        continue;
                    }
                }
            }
        }
    }


    private bool IsTileInList(TileBase tile, TileBase[] tileList)
    {
        foreach (TileBase t in tileList)
        {
            if (t == tile)
                return true;
        }
        return false;
    }
    private void PrepareOccupiedGrid()
    {
        if (grasslandTilemap == null) return;
        bounds = grasslandTilemap.cellBounds;
        occupied = new bool[bounds.size.x, bounds.size.y];
    }
}
