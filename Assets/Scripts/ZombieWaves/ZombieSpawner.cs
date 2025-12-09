using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ZombiePrefabConfig
{
    public GameObject prefab;
    public int minCountWave1 = 2;
    public int maxCountWave1 = 5;
    public int minCountIncreasePerWave = 1;
    public int maxCountIncreasePerWave = 2;
}

public class ZombieSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    private Transform[] spawnpoints;
    public string zombieTag = "Zombie";

    [Header("Zombie Prefabs")]
    public List<ZombiePrefabConfig> zombiePrefabs = new List<ZombiePrefabConfig>();

    [Header("Wave Settings")]
    public int initialZombieCount = 10;
    public float spawnRate = 1f;

    private float lastSpawnTime = 0f;
    private int currentWave = 0;
    private int zombiesSpawnedThisWave = 0;
    private int targetZombiesThisWave = 0;

    void Start()
    {
        if (spawnpoints == null || spawnpoints.Length == 0)
        {
            spawnpoints = GetComponentsInChildren<Transform>();
        }

        // Initialize first wave
        currentWave = 0;
        targetZombiesThisWave = initialZombieCount;
        zombiesSpawnedThisWave = 0;
    }

    void Update()
    {
        if (zombiePrefabs.Count == 0)
            return;

        lastSpawnTime += Time.deltaTime;
        if (lastSpawnTime >= spawnRate)
        {
            lastSpawnTime = 0f;

            int currentZombies = GameObject.FindGameObjectsWithTag(zombieTag).Length;

            // Check if wave is complete
            if (zombiesSpawnedThisWave >= targetZombiesThisWave && currentZombies == 0)
            {
                // Move to next wave (add 20% to zombie count)
                currentWave++;
                targetZombiesThisWave = Mathf.RoundToInt(initialZombieCount * (1f + (currentWave * 0.2f)));
                zombiesSpawnedThisWave = 0;
                Debug.Log($"[ZombieSpawner] Wave {currentWave + 1} started! Target zombies: {targetZombiesThisWave}");
            }

            // Spawn if we haven't reached target yet
            if (zombiesSpawnedThisWave < targetZombiesThisWave)
            {
                SpawnZombie();
                zombiesSpawnedThisWave++;
            }
        }
    }

    void SpawnZombie()
    {
        if (spawnpoints.Length == 0) return;

        // Get a random prefab based on wave difficulty
        GameObject prefabToSpawn = GetRandomZombiePrefab();
        if (prefabToSpawn == null) return;

        int spawnIndex = Random.Range(0, spawnpoints.Length);
        Transform spawnpoint = spawnpoints[spawnIndex];
        Instantiate(prefabToSpawn, spawnpoint.position, Quaternion.identity);
    }

    GameObject GetRandomZombiePrefab()
    {
        List<GameObject> validPrefabs = new List<GameObject>();

        // Determine min/max counts for this wave and collect eligible prefabs
        foreach (ZombiePrefabConfig config in zombiePrefabs)
        {
            int minCount = config.minCountWave1 + (currentWave * config.minCountIncreasePerWave);
            int maxCount = config.maxCountWave1 + (currentWave * config.maxCountIncreasePerWave);

            // Add prefab multiple times based on its range to create weighted selection
            for (int i = 0; i < Random.Range(minCount, maxCount + 1); i++)
            {
                if (config.prefab != null)
                    validPrefabs.Add(config.prefab);
            }
        }

        if (validPrefabs.Count == 0)
            return null;

        return validPrefabs[Random.Range(0, validPrefabs.Count)];
    }

    public int GetCurrentWave()
    {
        return currentWave + 1; // 1-indexed for UI display
    }

    public int GetZombiesRemainingThisWave()
    {
        return targetZombiesThisWave - zombiesSpawnedThisWave;
    }
}
