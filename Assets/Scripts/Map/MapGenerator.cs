using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    public enum BiomeType { Grassland, Desert, Swamp, Snow }

    [Header("Map Size & Noise")]
    public float noiseScale = 20f;

    [Header("Tilemap & Tile Variants")]
    public BiomeType biomeTilemap;
    public Tilemap grasslandTilemap;

    public TileBase[] grassTiles;
    public float grassChance;
    public TileBase[] dirtTiles;
    public float dirtChance;
    public TileBase[] stoneTiles;
    public float stoneChance;

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
    public float bushSpawnChance = 0.15f;  // 15%

    // Internal
    private bool[,] occupied;
    private BoundsInt bounds;

    public BiomeMapGenerator biomeMapGenerator;

    [Header("Chunk Settings")]
    public int chunkSize = 32; // kích thước 1 chunk
    public Transform player;
    public int viewDistance = 2; // số chunk hiển thị xung quanh player
    private Dictionary<Vector2Int, GameObject> chunkObjects = new();

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
        else if (biomeTilemap == BiomeType.Snow)
            grasslandTilemap = biomeMapGenerator.snowTilemap;
        if (grasslandTilemap == null) yield break;

        ApplyNoiseToTiles(grasslandTilemap);

        PrepareOccupiedGrid();

        TrySpawnLargeObjects();

        SpawnSmallObjects();
    }

    void Update()
    {
        UpdateChunks();
    }

    // ================== CHUNK HELPERS ==================
    private Vector2Int WorldToChunk(Vector3 pos)
    {
        return new Vector2Int(
            Mathf.FloorToInt(pos.x / chunkSize),
            Mathf.FloorToInt(pos.y / chunkSize)
        );
    }

    private GameObject GetOrCreateChunk(Vector2Int coord)
    {
        if (!chunkObjects.ContainsKey(coord))
        {
            GameObject chunkGO = new GameObject($"Chunk_{coord.x}_{coord.y}");
            chunkGO.transform.parent = this.transform;
            chunkObjects[coord] = chunkGO;
        }
        return chunkObjects[coord];
    }

    private void UpdateChunks()
    {
        if (player == null) return;

        Vector2Int currentChunk = WorldToChunk(player.position);

        foreach (var kvp in chunkObjects)
        {
            Vector2Int coord = kvp.Key;
            float dist = Vector2Int.Distance(coord, currentChunk);
            kvp.Value.SetActive(dist <= viewDistance);
        }
    }

    // ================== TILE GENERATION ==================
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
                if (noiseValue < stoneChance && stoneTiles.Length > 0)
                    selectedTile = stoneTiles[Random.Range(0, stoneTiles.Length)];
                else if (noiseValue < dirtChance && dirtTiles.Length > 0)
                    selectedTile = dirtTiles[Random.Range(0, dirtTiles.Length)];
                else if (grassTiles.Length > 0)
                    selectedTile = grassTiles[Random.Range(0, grassTiles.Length)];

                if (selectedTile != null)
                    targetTilemap.SetTile(tilePos, selectedTile);
            }
        }
    }

    // ================== LARGE OBJECTS ==================
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

                    // Spawn vào chunk
                    Vector2Int chunkCoord = WorldToChunk(worldPos);
                    GameObject parentChunk = GetOrCreateChunk(chunkCoord);

                    Instantiate(prefab, worldPos, Quaternion.identity, parentChunk.transform);

                    for (int dx = 0; dx < size.x; dx++)
                        for (int dy = 0; dy < size.y; dy++)
                            occupied[localX + dx, localY + dy] = true;

                    break;
                }
            }
        }
    }

    // ================== SMALL OBJECTS ==================
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
                Vector2Int chunkCoord = WorldToChunk(worldPos);
                GameObject parentChunk = GetOrCreateChunk(chunkCoord);

                // TREE & FLOWER — chỉ spawn trên grass tiles
                if (IsTileInList(currentTile, grassTiles))
                {
                    if (treePrefabs.Length > 0 && Random.value < treeSpawnChance)
                    {
                        Instantiate(treePrefabs[Random.Range(0, treePrefabs.Length)], worldPos, Quaternion.identity, parentChunk.transform);
                        occupied[localX, localY] = true;
                        continue;
                    }
                    else if (flowerPrefabs.Length > 0 && Random.value < flowerSpawnChance)
                    {
                        Instantiate(flowerPrefabs[Random.Range(0, flowerPrefabs.Length)], worldPos, Quaternion.identity, parentChunk.transform);
                        occupied[localX, localY] = true;
                        continue;
                    }
                }

                // ROCK — chỉ spawn trên stone tiles
                else if (IsTileInList(currentTile, stoneTiles))
                {
                    if (rockPrefabs.Length > 0 && Random.value < rockSpawnChance)
                    {
                        Instantiate(rockPrefabs[Random.Range(0, rockPrefabs.Length)], worldPos, Quaternion.identity, parentChunk.transform);
                        occupied[localX, localY] = true;
                        continue;
                    }
                }

                // BUSH — chỉ spawn trên grass hoặc dirt
                if (IsTileInList(currentTile, dirtTiles) || IsTileInList(currentTile, grassTiles))
                {
                    if (bushPrefabs.Length > 0 && Random.value < bushSpawnChance)
                    {
                        Instantiate(bushPrefabs[Random.Range(0, bushPrefabs.Length)], worldPos, Quaternion.identity, parentChunk.transform);
                        occupied[localX, localY] = true;
                        continue;
                    }
                }
            }
        }
    }

    // ================== HELPERS ==================
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

    public bool CanFarmAt(Vector3Int cellPos)
    {
        // Kiểm tra trong grassLandTilemap
        TileBase grassTile = grasslandTilemap.GetTile(cellPos);
        if (grassTile != null && IsTileInList(grassTile, grassTiles))
            return true;

        // Kiểm tra trong dirtLandTilemap
        TileBase dirtTile = grasslandTilemap.GetTile(cellPos);
        if (dirtTile != null && IsTileInList(dirtTile, dirtTiles))
            return true;

        return false;
    }
}
