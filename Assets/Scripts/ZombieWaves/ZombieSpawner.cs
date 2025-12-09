using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

[System.Serializable]
public class ZombiePrefabConfig
{
    public GameObject prefab;
    public int countWave1 = 2;
}

public class ZombieSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    private Transform[] spawnpoints;
    public string zombieTag = "Zombie";

    [Header("Zombie Prefabs")]
    public List<ZombiePrefabConfig> zombiePrefabs = new List<ZombiePrefabConfig>();

    [Header("Wave Settings")]
    public float timeBetweenWaves = 20f;

    private int currentWave = 0;
    private bool waveInProgress = false;
    private int zombiesSpawnedThisWave = 0;

    void Start()
    {
        if (spawnpoints == null || spawnpoints.Length == 0)
        {
            spawnpoints = GetComponentsInChildren<Transform>();
        }

        // Start first wave
        StartCoroutine(WaveLoop());
    }

    void Update()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetZombieProgress(zombiesSpawnedThisWave);
            if (!waveInProgress)
            {
                UIManager.Instance.zombiesKilledThisWave = 0;
            }
        }
    }

    IEnumerator WaveLoop()
    {
        while (true)
        {
            currentWave++;
            Debug.Log($"[ZombieSpawner] Starting Wave {currentWave}");
            
            if (UIManager.Instance != null)
                UIManager.Instance.SetWaveCounter(currentWave);

            SpawnWave();
            waveInProgress = true;

            // Wait for all zombies to be killed
            yield return new WaitUntil(() => AllZombiesDead());

            waveInProgress = false;
            Debug.Log($"[ZombieSpawner] Wave {currentWave} complete! Next wave in {timeBetweenWaves} seconds...");

            // Wait before starting next wave
            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    void SpawnWave()
    {
        if (zombiePrefabs.Count == 0 || spawnpoints.Length == 0)
            return;

        zombiesSpawnedThisWave = 0;

        foreach (ZombiePrefabConfig config in zombiePrefabs)
        {
            if (config.prefab == null)
                continue;

            // Calculate count for this wave (20% increase per wave)
            int count = Mathf.RoundToInt(config.countWave1 * Mathf.Pow(1.2f, currentWave - 1));

            // Spawn all zombies of this type
            for (int i = 0; i < count; i++)
            {
                int spawnIndex = Random.Range(0, spawnpoints.Length);
                Transform spawnpoint = spawnpoints[spawnIndex];
                Instantiate(config.prefab, spawnpoint.position, Quaternion.identity);
                zombiesSpawnedThisWave++;
            }

            Debug.Log($"[ZombieSpawner] Spawned {count} of {config.prefab.name} for wave {currentWave}");
        }
    }

    bool AllZombiesDead()
    {
        return GameObject.FindGameObjectsWithTag(zombieTag).Length == 0;
    }

    public int GetCurrentWave()
    {
        return currentWave;
    }

    public bool IsWaveInProgress()
    {
        return waveInProgress;
    }

    public int GetZombiesSpawnedThisWave()
    {
        return zombiesSpawnedThisWave;
    }
}
