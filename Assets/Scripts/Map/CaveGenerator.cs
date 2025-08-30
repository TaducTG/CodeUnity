using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class CaveGenerator : MonoBehaviour
{
    [Header("Prefabs & Tile")]
    public Tilemap floorTilemap;
    public TileBase[] floorTiles;
    public List<WallRule> wallRules;

    [Header("Map Settings")]
    public int width = 50, height = 50;
    [Range(0, 100)] public int fillPercent = 45;
    public int smoothIterations = 5;
    public Vector2Int startPosition = Vector2Int.zero;

    [Header("Chunk Settings")]
    public int chunkSize = 16;
    public int viewDistance = 3;
    public Transform player;

    [Header("Parents")]
    public Transform wallParent;

    [Header("Fixed Biome Prefabs")]
    public List<GameObject> fixedBiomePrefabs;
    public List<Vector2Int> fixedBiomePositions;

    [Header("Small Objects")]
    public List<GameObject> smallObjects;
    [Range(0f, 1f)] public float smallObjectChance = 0.05f;

    private int[,] map;
    private Dictionary<WallType, GameObject> wallDict;
    private Dictionary<Vector2Int, GameObject> chunkObjects = new Dictionary<Vector2Int, GameObject>();
    private Vector2Int lastPlayerChunk = Vector2Int.zero;
    private System.Random rand;

    void Start()
    {
        BuildWallDict();

        GenerateMap();

        SpawnFixedPrefabs();

        UpdateChunks(true); // Bật chunk ban đầu
    }

    void Update()
    {
        UpdateChunks();
    }

    void BuildWallDict()
    {
        wallDict = new Dictionary<WallType, GameObject>();
        foreach (var rule in wallRules)
            wallDict[rule.type] = rule.prefab;
    }

    void GenerateMap()
    {
        map = new int[width, height];
        rand = new System.Random();

        // Random fill
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                map[x, y] = (x == 0 || y == 0 || x == width - 1 || y == height - 1)
                            ? 1 : (rand.Next(100) < fillPercent ? 1 : 0);

        // Smooth
        for (int i = 0; i < smoothIterations; i++)
        {
            int[,] newMap = new int[width, height];
            for (int x = 1; x < width - 1; x++)
                for (int y = 1; y < height - 1; y++)
                {
                    int wallCount = CountNeighbors(x, y);
                    newMap[x, y] = wallCount > 4 ? 1 : (wallCount < 4 ? 0 : map[x, y]);
                }
            map = newMap;
        }

        // Expand walls
        for (int x = 1; x < width - 1; x++)
            for (int y = 1; y < height - 1; y++)
                if (map[x, y] == 1)
                    for (int dx = -1; dx <= 1; dx++)
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            int nx = x + dx, ny = y + dy;
                            if (map[nx, ny] == 0) map[nx, ny] = 2;
                        }
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (map[x, y] == 2) map[x, y] = 1;

        // Tạo đường đi giữa entrance/exit
        if (fixedBiomePositions.Count >= 2)
        {
            Vector2Int entrance = fixedBiomePositions[0];
            Vector2Int exit = fixedBiomePositions[1];
            ClearAreaAround(ref map, entrance);
            ClearAreaAround(ref map, exit);

            List<Vector2Int> path = FindPath(entrance, exit);
            if (path != null)
                foreach (var pos in path) map[pos.x, pos.y] = 0;
        }

        // Clear cũ
        foreach (Transform child in wallParent) Destroy(child.gameObject);
        chunkObjects.Clear();

        // Generate chunks
        for (int cx = 0; cx < width; cx += chunkSize)
            for (int cy = 0; cy < height; cy += chunkSize)
            {
                Vector2Int chunkCoord = new Vector2Int(cx / chunkSize, cy / chunkSize);
                GameObject chunkGO = new GameObject($"Chunk_{chunkCoord.x}_{chunkCoord.y}");
                chunkGO.transform.parent = wallParent;
                chunkObjects[chunkCoord] = chunkGO;

                for (int x = cx; x < Mathf.Min(cx + chunkSize, width); x++)
                    for (int y = cy; y < Mathf.Min(cy + chunkSize, height); y++)
                    {
                        Vector3Int cellPos = new Vector3Int(x + startPosition.x, y + startPosition.y, 0);
                        Vector3 worldPos = cellPos + new Vector3(0.5f, 0.5f, 0);

                        // Floor
                        if (floorTiles.Length > 0)
                            floorTilemap.SetTile(cellPos, floorTiles[rand.Next(floorTiles.Length)]);

                        // Wall or Object
                        if (map[x, y] == 1)
                        {
                            WallType type = GetWallType(x, y);
                            if (wallDict.TryGetValue(type, out GameObject prefab))
                                Instantiate(prefab, worldPos, Quaternion.identity, chunkGO.transform);
                        }
                        else
                        {
                            Vector3 WorldPos = new Vector3(x, y, 0); // hoặc convert sang world pos nếu có offset
                            SpawnSmallObject(x, y, WorldPos);
                        }
                    }
            }
    }

    // ===== Spawn Small Objects vào trong chunk =====
    private void SpawnSmallObject(int x, int y, Vector3 worldPos)
    {
        // Chỉ spawn ở floor
        if (map[x, y] == 0)
        {
            GameObject prefab = smallObjects[Random.Range(0, smallObjects.Count)];

            GameObject obj = Instantiate(
                prefab,
                worldPos,
                Quaternion.identity,
                gameObject.transform
            );
        }
    }

    // ===== Spawn Fixed Prefabs (không thuộc chunk) =====
    void SpawnFixedPrefabs()
    {
        for (int i = 0; i < Mathf.Min(fixedBiomePrefabs.Count, fixedBiomePositions.Count); i++)
        {
            Vector2Int pos = fixedBiomePositions[i];
            GameObject prefab = fixedBiomePrefabs[i];
            Vector3 worldPos = new Vector3(pos.x + startPosition.x + 0.5f, pos.y + startPosition.y + 0.5f, 0);

            Instantiate(prefab, worldPos, Quaternion.identity);
        }
    }

    void UpdateChunks(bool force = false)
    {
        if (player == null) return;

        Vector2Int currentChunk = new Vector2Int(
            Mathf.FloorToInt((player.position.x - startPosition.x) / chunkSize),
            Mathf.FloorToInt((player.position.y - startPosition.y) / chunkSize)
        );

        if (force || currentChunk != lastPlayerChunk)
        {
            foreach (var kvp in chunkObjects)
            {
                Vector2Int coord = kvp.Key;
                float dist = Vector2Int.Distance(coord, currentChunk);
                kvp.Value.SetActive(dist <= viewDistance);
            }
            lastPlayerChunk = currentChunk;
        }
    }


    void ClearAreaAround(ref int[,] map, Vector2Int center)
    {
        for (int dx = -5; dx <= 5; dx++)
            for (int dy = -5; dy <= 5; dy++)
            {
                int nx = center.x + dx;
                int ny = center.y + dy;
                if (InMap(nx, ny))
                    map[nx, ny] = 0;
            }
    }

    int CountNeighbors(int x, int y)
    {
        int count = 0;
        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
                if (!(dx == 0 && dy == 0))
                    count += InMap(x + dx, y + dy) ? map[x + dx, y + dy] : 1;
        return count;
    }

    bool InMap(int x, int y) => x >= 0 && x < width && y >= 0 && y < height;

    WallType GetWallType(int x, int y)
    {
        // Kiểm tra 8 hướng xung quanh
        bool n = InMap(x, y + 1) && map[x, y + 1] == 1;
        bool s = InMap(x, y - 1) && map[x, y - 1] == 1;
        bool e = InMap(x + 1, y) && map[x + 1, y] == 1;
        bool w = InMap(x - 1, y) && map[x - 1, y] == 1;

        bool ne = InMap(x + 1, y + 1) && map[x + 1, y + 1] == 1;
        bool nw = InMap(x - 1, y + 1) && map[x - 1, y + 1] == 1;
        bool se = InMap(x + 1, y - 1) && map[x + 1, y - 1] == 1;
        bool sw = InMap(x - 1, y - 1) && map[x - 1, y - 1] == 1;

        int count = 0;
        if (n) count++;
        if (s) count++;
        if (e) count++;
        if (w) count++;
        if (ne) count++;
        if (nw) count++;
        if (se) count++;
        if (sw) count++;

        if (count == 0)
            return WallType.Isolated;
        if (n && s && e && w && ne && nw && se && sw)
            return WallType.Center;

        if (!n && s && e && w) return WallType.North;
        if (!s && n && e && w) return WallType.South;
        if (!e && n && s && w) return WallType.East;
        if (!w && n && s && e) return WallType.West;

        if (n && s && e && w && ne && nw && se && !sw) return WallType.SouthWestM;
        if (n && s && e && w && ne && nw && !se && sw) return WallType.SouthEastM;
        if (n && s && e && w && ne && !nw && se && sw) return WallType.NorthWestM;
        if (n && s && e && w && !ne && nw && se && sw) return WallType.NorthEastM;

        if (!n && !e && w && s) return WallType.NorthEast;
        if (!n && e && w && s && !se) return WallType.NorthEast;
        if (n && !e && w && s && !nw) return WallType.NorthEast;

        if (!n && !w && e && s) return WallType.NorthWest;
        if (!n && e && w && s && !sw) return WallType.NorthWest;
        if (n && e && !w && s && !ne) return WallType.NorthWest;

        if (!s && !e && n && w) return WallType.SouthEast;
        if (n && e && w && !s && !ne) return WallType.SouthEast;
        if (n && !e && w && s && !sw) return WallType.SouthEast;

        if (!s && !w && e && n) return WallType.SouthWest;
        if (n && e && w && !s && !nw) return WallType.SouthWest;
        if (n && e && !w && s && !se) return WallType.SouthWest;

        // Mặc định nếu không khớp các rule đặc biệt
        return WallType.Center;
    }

    //================= A* Pathfinding ===================
    class Node
    {
        public Vector2Int pos;
        public int gCost;
        public int hCost;
        public Node parent;

        public int fCost => gCost + hCost;

        public Node(Vector2Int pos)
        {
            this.pos = pos;
        }
    }

    List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
    {
        Node startNode = new Node(start);
        Node endNode = new Node(end);

        List<Node> openSet = new List<Node> { startNode };
        HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();

        Dictionary<Vector2Int, Node> allNodes = new Dictionary<Vector2Int, Node>();
        allNodes[start] = startNode;

        Vector2Int[] directions = {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };

        while (openSet.Count > 0)
        {
            Node current = openSet[0];
            foreach (var node in openSet)
            {
                if (node.fCost < current.fCost || (node.fCost == current.fCost && node.hCost < current.hCost))
                    current = node;
            }

            openSet.Remove(current);
            closedSet.Add(current.pos);

            if (current.pos == end)
            {
                List<Vector2Int> path = new List<Vector2Int>();
                Node temp = current;
                while (temp != null)
                {
                    path.Add(temp.pos);
                    temp = temp.parent;
                }
                path.Reverse();
                return path;
            }

            foreach (var dir in directions)
            {
                Vector2Int neighborPos = current.pos + dir;

                if (!InMap(neighborPos.x, neighborPos.y) || map[neighborPos.x, neighborPos.y] != 0 || closedSet.Contains(neighborPos))
                    continue;

                int tentativeGCost = current.gCost + 1;
                if (!allNodes.TryGetValue(neighborPos, out Node neighbor))
                {
                    neighbor = new Node(neighborPos);
                    allNodes[neighborPos] = neighbor;
                }

                if (tentativeGCost < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = tentativeGCost;
                    neighbor.hCost = Mathf.Abs(neighbor.pos.x - end.x) + Mathf.Abs(neighbor.pos.y - end.y);
                    neighbor.parent = current;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return null;
    }
}
