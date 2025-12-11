using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages respawning of explosive barrel children.
/// Tracks barrels and can reinstantiate destroyed ones while keeping alive ones intact.
/// </summary>
public class ExplosiveBarrelsRespawner : MonoBehaviour
{
    /// <summary>
    /// Respawns all barrels by enabling them.
    /// </summary>
    public void RespawnAll()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
        //Debug.Log("[ExplosiveBarrelsRespawner] All barrels respawned (enabled).");
    }
}

