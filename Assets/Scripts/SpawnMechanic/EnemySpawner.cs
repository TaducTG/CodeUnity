using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EnemySpawner : MonoBehaviour
{
    public Transform player;
    public float spawnRadius = 50f;
    public BiomeEnemyConfig[] biomeConfigs;

    private Dictionary<string, Tilemap> biomeTilemaps = new Dictionary<string, Tilemap>();

    void Start()
    {
        // Lấy tilemap từ Grid
        foreach (var config in biomeConfigs)
        {
            Tilemap map = GameObject.Find(config.biomeName)?.GetComponent<Tilemap>();
            if (map != null)
            {
                biomeTilemaps[config.biomeName] = map;

                // Tạo pool cho enemy trong biome này
                foreach (var enemy in config.enemyList)
                {
                    EnemyPoolManager.Instance.CreatePool(enemy.enemyPrefab, enemy.poolSize);
                    StartCoroutine(SpawnRoutine(config, map, enemy));
                }
            }
            else
            {
                Debug.LogWarning("Tilemap " + config.biomeName + " not found!");
            }
        }
    }

    IEnumerator SpawnRoutine(BiomeEnemyConfig biomeConfig, Tilemap tilemap, EnemySpawnData enemyData)
    {
        while (true)
        {
            yield return new WaitForSeconds(enemyData.spawnInterval);

            Vector3 spawnPos = GetRandomSpawnPosition(tilemap);
            if (spawnPos != Vector3.zero)
            {
                EnemyPoolManager.Instance.SpawnFromPool(enemyData.enemyPrefab, spawnPos, Quaternion.identity);
            }
        }
    }

    Vector3 GetRandomSpawnPosition(Tilemap tilemap)
    {
        Vector3Int playerCell = tilemap.WorldToCell(player.position);
        List<Vector3Int> validCells = new List<Vector3Int>();

        foreach (var pos in tilemap.cellBounds.allPositionsWithin)
        {
            if (!tilemap.HasTile(pos)) continue;

            if (Vector3Int.Distance(pos, playerCell) <= spawnRadius)
                validCells.Add(pos);
        }

        if (validCells.Count == 0) return Vector3.zero;

        Vector3Int randomCell = validCells[Random.Range(0, validCells.Count)];
        return tilemap.CellToWorld(randomCell) + new Vector3(0.5f, 0.5f, 0);
    }
}
