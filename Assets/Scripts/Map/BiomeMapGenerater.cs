using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BiomeMapGenerator : MonoBehaviour
{
    public enum BiomeType { Grassland, Desert, Swamp, Snow,Sea,Sand }

    [System.Serializable]
    public class BiomeData
    {
        public BiomeType biomeType;
        public Vector2Int startPosition;
        public int tileCount = 100;
    }

    [System.Serializable]
    public class LargeObjectSpawnData
    {
        public GameObject prefab;
        public Vector2Int size; // width x height
        public TileBase requiredTile;
        public int attempts = 100;
    }
    [Header("Fixed Biome Prefabs")]
    public List<GameObject> fixedBiomePrefabs; // Drag prefab bạn tạo vào
    public List<Vector2Int> fixedBiomePositions; // Vị trí spawn tương ứng (bằng ô grid)


    [Header("Tilemaps")]
    public Tilemap biomeMaskTilemap;
    public Tilemap grasslandTilemap;
    public Tilemap desertTilemap;
    public Tilemap swampTilemap;
    public Tilemap snowTilemap;
    public Tilemap seaTilemap;
    public Tilemap sandTilemap;

    [Header("Tiles")]
    public TileBase grassTile;
    public TileBase desertTile;
    public TileBase swampTile;
    public TileBase snowTile;
    public TileBase seaTile;
    public TileBase sandTile;

    [Header("Biomes")]
    public List<BiomeData> biomes;


    [Header("Mask Bounds")]
    public Vector2Int maskMinBounds = new Vector2Int(0, 0);
    public Vector2Int maskMaxBounds = new Vector2Int(99, 99);

    private Dictionary<BiomeType, List<Vector2Int>> biomeTilePositions = new();
    private Dictionary<Vector2Int, BiomeType> tileBiomeMap = new();
    private HashSet<Vector2Int> globalUsedPositions = new();

    public int width => maskMaxBounds.x + 1;
    public int height => maskMaxBounds.y + 1;

    public bool isGenerated { get; private set; }

    void Start()
    {
        
        // tạo phần biển ở rìa ngoài map
        GenerateNoisySeaEdge(15,0.1f,0.5f);

        // tạo các tilebase cho map chính
        GenerateBiomes();

        // Xử lý spawn biển lỗi
        CleanupInvalidSeaTiles(15);

        // Đảm bảo các tilebase spawn trông tự nhiên hơn
        SmoothEdges();

        // Xử lý tilebase spawn lỗi
        FillRemainingWithClosestBiome();

        // Spawn biome rìa ngoài của 1 biome nếu tiếp giáp với 1 biome khác
        GenerateTransitionBiome(BiomeType.Grassland, BiomeType.Sea, BiomeType.Sand, 3);

        // Spawn các tilemap tự tạo
        ApplyFixedBiomePrefabs();

        // Thêm các tilebase vào từng Tilemap cụ thể để xử lý spawn Object
        ApplyBiomesToIndividualTilemaps();

        isGenerated = true;
    }
    void ApplyFixedBiomePrefabs()
    {
        for (int i = 0; i < fixedBiomePrefabs.Count; i++)
        {
            GameObject prefab = fixedBiomePrefabs[i];
            Vector2Int spawnPosition = fixedBiomePositions[i];

            GameObject instance = Instantiate(prefab, new Vector3(spawnPosition.x, spawnPosition.y, 0), Quaternion.identity);
            FixedBiomeArea biomeArea = instance.GetComponent<FixedBiomeArea>();

            if (biomeArea == null || biomeArea.tilemap == null)
            {
                Debug.LogWarning("Invalid FixedBiome prefab: missing component.");
                continue;
            }

            foreach (var localPos in biomeArea.GetLocalTilePositions())
            {
                Vector2Int worldPos = spawnPosition + localPos;

                if (!IsInMask(worldPos)) continue;

                // ❌ Nếu tile đang thuộc biome nào khác thì xóa khỏi các bảng dữ liệu đó
                if (tileBiomeMap.TryGetValue(worldPos, out var oldBiome))
                {
                    biomeTilePositions[oldBiome]?.Remove(worldPos);
                    tileBiomeMap.Remove(worldPos);
                }

                // ✅ Đánh dấu là đã dùng (không cho biome khác sinh vào sau)
                globalUsedPositions.Add(worldPos);
            }

            Debug.Log($"[FixedTilemap] Applied fixed tiles at {spawnPosition}, overwritten previous biomes.");
        }
    }


    void GenerateNoisySeaEdge(float seaThickness = 5f, float noiseScale = 0.1f, float noiseThreshold = 0.5f)
    {
        if (!biomeTilePositions.ContainsKey(BiomeType.Sea))
            biomeTilePositions[BiomeType.Sea] = new List<Vector2Int>();

        float centerX = (maskMinBounds.x + maskMaxBounds.x) / 2f;
        float centerY = (maskMinBounds.y + maskMaxBounds.y) / 2f;

        for (int x = maskMinBounds.x; x <= maskMaxBounds.x; x++)
        {
            for (int y = maskMinBounds.y; y <= maskMaxBounds.y; y++)
            {
                Vector2Int pos = new(x, y);

                // Tính khoảng cách đến biên
                float dx = Mathf.Min(x - maskMinBounds.x, maskMaxBounds.x - x);
                float dy = Mathf.Min(y - maskMinBounds.y, maskMaxBounds.y - y);
                float minDistToEdge = Mathf.Min(dx, dy);

                if (minDistToEdge > seaThickness) continue;

                // Tính noise
                float nx = x * noiseScale;
                float ny = y * noiseScale;
                float noise = Mathf.PerlinNoise(nx, ny);

                // Ghép noise + khoảng cách để tạo hiệu ứng mép gồ ghề
                float value = noise * seaThickness;

                if (value > minDistToEdge - Random.Range(0f, 1f)) // ngưỡng gồ ghề có thể điều chỉnh
                {
                    biomeTilePositions[BiomeType.Sea].Add(pos);
                    tileBiomeMap[pos] = BiomeType.Sea;
                    globalUsedPositions.Add(pos);
                }
            }
        }
    }

    void GenerateBiomes()
    {
        foreach (var biome in biomes)
        {
            if (!biomeTilePositions.ContainsKey(biome.biomeType))
                biomeTilePositions[biome.biomeType] = new List<Vector2Int>();

            HashSet<Vector2Int> biomeUsed = new();
            List<Vector2Int> frontier = new() { biome.startPosition };

            while (biomeTilePositions[biome.biomeType].Count < biome.tileCount && frontier.Count > 0)
            {
                int index = Random.Range(0, frontier.Count);
                Vector2Int current = frontier[index];
                frontier.RemoveAt(index);

                if (!IsInMask(current)) continue;
                if (biomeUsed.Contains(current) || globalUsedPositions.Contains(current)) continue;

                biomeTilePositions[biome.biomeType].Add(current);
                tileBiomeMap[current] = biome.biomeType;

                biomeUsed.Add(current);
                globalUsedPositions.Add(current);

                foreach (Vector2Int dir in Direction4)
                {
                    Vector2Int neighbor = current + dir;
                    if (!biomeUsed.Contains(neighbor) && !globalUsedPositions.Contains(neighbor))
                        frontier.Add(neighbor);
                }
            }
        }
    }

    void CleanupInvalidSeaTiles(int maxDistanceFromEdge = 7)
    {
        if (!biomeTilePositions.ContainsKey(BiomeType.Sea)) return;

        List<Vector2Int> validSeaTiles = new();
        List<Vector2Int> toRemove = new();

        foreach (var pos in biomeTilePositions[BiomeType.Sea])
        {
            int dx = Mathf.Min(pos.x - maskMinBounds.x, maskMaxBounds.x - pos.x);
            int dy = Mathf.Min(pos.y - maskMinBounds.y, maskMaxBounds.y - pos.y);
            int minDistToEdge = Mathf.Min(dx, dy);

            if (minDistToEdge <= maxDistanceFromEdge)
            {
                validSeaTiles.Add(pos);
            }
            else
            {
                toRemove.Add(pos);
            }
        }

        // Cập nhật lại danh sách Sea
        biomeTilePositions[BiomeType.Sea] = validSeaTiles;

        // Xoá trong tileBiomeMap và globalUsed
        foreach (var pos in toRemove)
        {
            tileBiomeMap.Remove(pos);
            globalUsedPositions.Remove(pos);

            // Xoá khỏi tilemap nếu đã vẽ
            seaTilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), null);
        }
    }

    public void GenerateTransitionBiome(BiomeType innerBiome, BiomeType outerBiome, BiomeType transitionBiome, int thickness = 3)
    {
        if (!biomeTilePositions.ContainsKey(innerBiome) || !biomeTilePositions.ContainsKey(outerBiome))
            return;

        if (!biomeTilePositions.ContainsKey(transitionBiome))
            biomeTilePositions[transitionBiome] = new List<Vector2Int>();

        HashSet<Vector2Int> added = new();

        foreach (var pos in biomeTilePositions[innerBiome].ToList())
        {
            foreach (var dir in Direction4)
            {
                Vector2Int neighbor = pos + dir;

                // Nếu ô bên cạnh là outerBiome thì bắt đầu sinh transition
                if (tileBiomeMap.TryGetValue(neighbor, out BiomeType biome) && biome == outerBiome)
                {
                    // Hướng từ outer → inner
                    Vector2Int delta = pos - neighbor;
                    delta = new Vector2Int(Mathf.Clamp(delta.x, -1, 1), Mathf.Clamp(delta.y, -1, 1));

                    for (int i = 1; i <= thickness; i++)
                    {
                        Vector2Int transPos = neighbor + delta * i;
                        if (!IsInMask(transPos)) continue;
                        if (added.Contains(transPos)) continue;

                        // ❗ Chỉ ghi đè nếu tile đó là innerBiome
                        if (tileBiomeMap.TryGetValue(transPos, out var currentBiome) && currentBiome == innerBiome)
                        {
                            // Gỡ khỏi biome cũ
                            biomeTilePositions[innerBiome].Remove(transPos);

                            // Ghi đè
                            tileBiomeMap[transPos] = transitionBiome;
                            biomeTilePositions[transitionBiome].Add(transPos);
                            added.Add(transPos);
                        }
                    }
                }
            }
        }

        globalUsedPositions.UnionWith(added);
        //Debug.Log($"[BiomeTransition] Overwrote {innerBiome} → {transitionBiome} between {outerBiome} and {innerBiome}");
    }

    void SmoothEdges()
    {
        HashSet<Vector2Int> toAdd = new();

        foreach (var pair in biomeTilePositions)
        {
            BiomeType biome = pair.Key;
            List<Vector2Int> positions = pair.Value;

            foreach (Vector2Int pos in positions)
            {
                foreach (Vector2Int dir in Direction4)
                {
                    Vector2Int neighbor = pos + dir;

                    if (tileBiomeMap.ContainsKey(neighbor)) continue;
                    if (!IsInMask(neighbor)) continue;

                    int sameCount = 0;
                    foreach (Vector2Int checkDir in Direction4)
                    {
                        Vector2Int checkPos = neighbor + checkDir;
                        if (tileBiomeMap.TryGetValue(checkPos, out BiomeType b) && b == biome)
                            sameCount++;
                    }

                    if (sameCount >= 3)
                    {
                        toAdd.Add(neighbor);
                        tileBiomeMap[neighbor] = biome;
                    }
                }
            }

            biomeTilePositions[biome].AddRange(toAdd);
            globalUsedPositions.UnionWith(toAdd);
            toAdd.Clear();
        }
    }

    void ApplyBiomesToIndividualTilemaps()
    {
        foreach (var pair in biomeTilePositions)
        {
            BiomeType biome = pair.Key;
            List<Vector2Int> positions = pair.Value;

            Tilemap tilemap = GetTilemapForBiome(biome);
            TileBase tile = GetTileForBiome(biome);

            foreach (Vector2Int pos in positions)
            {
                tilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), tile);
            }
        }
    }

    public void FillRemainingWithClosestBiome()
    {
        // Tính center của từng biome
        Dictionary<BiomeType, Vector2> biomeCenters = new();
        foreach (var pair in biomeTilePositions)
        {
            BiomeType biome = pair.Key;
            List<Vector2Int> positions = pair.Value;

            Vector2 sum = Vector2.zero;
            foreach (var pos in positions)
            {
                sum += pos;
            }

            biomeCenters[biome] = sum / positions.Count;
        }

        // Duyệt qua vùng mask, tìm ô nào chưa có biome thì gán vào biome gần nhất
        for (int x = maskMinBounds.x; x <= maskMaxBounds.x; x++)
        {
            for (int y = maskMinBounds.y; y <= maskMaxBounds.y; y++)
            {
                Vector2Int pos = new(x, y);

                if (tileBiomeMap.ContainsKey(pos)) continue; // đã có rồi
                if (!IsInMask(pos)) continue;

                // Tìm biome gần nhất
                float minDist = float.MaxValue;
                BiomeType closestBiome = BiomeType.Grassland;

                foreach (var pair in biomeCenters)
                {
                    float dist = Vector2.Distance(pos, pair.Value);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closestBiome = pair.Key;
                    }
                }

                // Gán vào biome gần nhất
                tileBiomeMap[pos] = closestBiome;
                biomeTilePositions[closestBiome].Add(pos);
                globalUsedPositions.Add(pos);

                Tilemap targetMap = GetTilemapForBiome(closestBiome);
                TileBase tile = GetTileForBiome(closestBiome);
                targetMap.SetTile(new Vector3Int(pos.x, pos.y, 0), tile);
            }
        }
    }
    Tilemap GetTilemapForBiome(BiomeType biome)
    {
        return biome switch
        {
            BiomeType.Grassland => grasslandTilemap,
            BiomeType.Desert => desertTilemap,
            BiomeType.Swamp => swampTilemap,
            BiomeType.Snow => snowTilemap,
            BiomeType.Sea => seaTilemap,
            BiomeType.Sand => sandTilemap,
            _ => grasslandTilemap
        };
    }

    TileBase GetTileForBiome(BiomeType biome)
    {
        return biome switch
        {
            BiomeType.Grassland => grassTile,
            BiomeType.Desert => desertTile,
            BiomeType.Swamp => swampTile,
            BiomeType.Snow => snowTile,
            BiomeType.Sea => seaTile,
            BiomeType.Sand => sandTile,
            _ => grassTile
        };
    }


    bool IsInMask(Vector2Int pos)
    {
        return pos.x >= maskMinBounds.x && pos.x <= maskMaxBounds.x &&
               pos.y >= maskMinBounds.y && pos.y <= maskMaxBounds.y;
    }

    private static readonly Vector2Int[] Direction4 = new Vector2Int[]
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };
}
