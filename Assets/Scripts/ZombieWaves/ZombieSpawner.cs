using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    [Header("Resetting Scene")]
    public GameObject explosiveBarrelsRespawner;
    public GameObject player;

    private int currentWave = 0;
    private bool waveInProgress = false;
    private int zombiesSpawnedThisWave = 0;
    private float timeUntilNextWave = 0f;

    // Public getter for UI to access countdown
    public float TimeUntilNextWave => timeUntilNextWave;

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
        if (UIManager.Instance != null && !waveInProgress)
        {
            UIManager.Instance.SetZombieProgress(zombiesSpawnedThisWave);
            UIManager.Instance.zombiesKilledThisWave = 0;
        }
    }

    IEnumerator WaveLoop()
    {
        while (true)
        {
            currentWave++;
            //Debug.Log($"[ZombieSpawner] Starting Wave {currentWave}");
            
            if (UIManager.Instance != null)
                UIManager.Instance.SetWaveCounter(currentWave);

            yield return StartCoroutine(SpawnWaveAsync());
            waveInProgress = true;

            // Wait for all zombies to be killed
            yield return new WaitUntil(() => AllZombiesDead());

            waveInProgress = false;
            //Debug.Log($"[ZombieSpawner] Wave {currentWave} complete! Next wave in {timeBetweenWaves} seconds...");

            // Reset Explosive Barrels
            if (explosiveBarrelsRespawner != null)
            {
                var respawner = explosiveBarrelsRespawner.GetComponent<ExplosiveBarrelsRespawner>();
                if (respawner != null)
                {
                    respawner.RespawnAll();
                }
            }

            // Heal Player
            if (player != null)
            {
                var playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.GainHealth(playerHealth.maxHealth);
                }
            }

            // Wait before starting next wave with countdown
            timeUntilNextWave = timeBetweenWaves;
            while (timeUntilNextWave > 0)
            {
                timeUntilNextWave -= Time.deltaTime;
                yield return null;
            }
            timeUntilNextWave = 0f;
        }
    }

    IEnumerator SpawnWaveAsync()
    {
        if (zombiePrefabs.Count == 0 || spawnpoints.Length == 0)
            yield break;

        zombiesSpawnedThisWave = 0;

        foreach (ZombiePrefabConfig config in zombiePrefabs)
        {
            if (config.prefab == null)
                continue;

            // Calculate count for this wave (20% increase per wave)
            int count = Mathf.RoundToInt(config.countWave1 * Mathf.Pow(1.2f, currentWave - 1));

            // Spawn all zombies of this type, yielding between spawns to spread load
            for (int i = 0; i < count; i++)
            {
                int spawnIndex = Random.Range(0, spawnpoints.Length);
                Transform spawnpoint = spawnpoints[spawnIndex];
                Instantiate(config.prefab, spawnpoint.position, Quaternion.identity);
                zombiesSpawnedThisWave++;

                // Yield every 3 zombies to avoid frame hitches
                if (i % 3 == 2)
                    yield return null;
            }

            //Debug.Log($"[ZombieSpawner] Spawned {count} of {config.prefab.name} for wave {currentWave}");
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
