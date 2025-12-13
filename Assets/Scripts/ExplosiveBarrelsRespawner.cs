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
        // Get all children, including inactive ones
        int childCount = transform.childCount;
        
        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child != null)
            {
                child.gameObject.SetActive(true);
            }
        }
    }
}

