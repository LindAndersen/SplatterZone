using UnityEngine;
using System.Collections.Generic;

public class RadarUI : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;           // El jugador
    public RectTransform radarArea;    // RectTransform del RadarImage
    public GameObject blipPrefab;      // Prefab del puntito

    [Header("Config")]
    public float radarRange = 50f;     // Distancia máxima que muestra el radar

    private Dictionary<GameObject, GameObject> enemyBlips = new Dictionary<GameObject, GameObject>();

    void Update()
    {
        if (player == null || radarArea == null || blipPrefab == null)
            return;

        // Get all enemies
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Zombie");
        float radarRadius = radarArea.rect.width * 0.5f;

        // Remove blips for enemies that no longer exist
        List<GameObject> enemiesToRemove = new List<GameObject>();
        foreach (GameObject enemy in enemyBlips.Keys)
        {
            if (enemy == null)
                enemiesToRemove.Add(enemy);
        }
        foreach (GameObject enemy in enemiesToRemove)
        {
            Destroy(enemyBlips[enemy]);
            enemyBlips.Remove(enemy);
        }

        // Update or create blips for each enemy
        foreach (GameObject enemy in enemies)
        {
            // Check if zombie is dead
            ZombieStatsBase stats = enemy.GetComponent<ZombieStatsBase>();
            if (stats != null && stats.IsDead())
            {
                // Remove blip if zombie is dead
                if (enemyBlips.ContainsKey(enemy))
                {
                    Destroy(enemyBlips[enemy]);
                    enemyBlips.Remove(enemy);
                }
                continue;
            }

            Vector3 offset = enemy.transform.position - player.position;
            Vector2 offset2D = new Vector2(offset.x, offset.z);

            // If too far, hide/remove blip
            if (offset2D.magnitude > radarRange)
            {
                if (enemyBlips.ContainsKey(enemy))
                {
                    Destroy(enemyBlips[enemy]);
                    enemyBlips.Remove(enemy);
                }
                continue;
            }

            // Rotate offset based on player's facing direction (Y rotation)
            float playerYRotation = player.eulerAngles.y * Mathf.Deg2Rad;
            Vector2 rotatedOffset = new Vector2(
                offset2D.x * Mathf.Cos(playerYRotation) - offset2D.y * Mathf.Sin(playerYRotation),
                offset2D.x * Mathf.Sin(playerYRotation) + offset2D.y * Mathf.Cos(playerYRotation)
            );

            // Normalize and calculate radar position
            Vector2 normalizedPos = rotatedOffset / radarRange;
            Vector2 radarPos = normalizedPos * radarRadius;

            // Create blip if it doesn't exist
            if (!enemyBlips.ContainsKey(enemy))
            {
                GameObject blip = Instantiate(blipPrefab, radarArea);
                enemyBlips[enemy] = blip;
            }

            // Update blip position
            RectTransform blipRect = enemyBlips[enemy].GetComponent<RectTransform>();
            blipRect.anchoredPosition = radarPos;
        }
    }
}

