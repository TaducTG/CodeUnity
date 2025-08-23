using UnityEngine;

[System.Serializable]
public class EnemySpawnData
{
    public GameObject enemyPrefab;
    public int poolSize = 3;
    public float spawnInterval = 5f;
}

[System.Serializable]
public class BiomeEnemyConfig
{
    public string biomeName; // Tên Tilemap, ví dụ "GrassLandTilemap"
    public EnemySpawnData[] enemyList;
}
