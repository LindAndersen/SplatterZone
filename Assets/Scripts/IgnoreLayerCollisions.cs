using UnityEngine;

public class IgnoreLayerCollisions : MonoBehaviour
{
    [Header("Layers to Ignore")]
    public string[] layersToIgnore = new string[] { "Character", "Projectile" };

    void Start()
    {
        Collider thisCollider = GetComponent<Collider>();
        if (thisCollider == null)
        {
            Debug.LogWarning("IgnoreLayerCollisions: No collider found on this GameObject!");
            return;
        }

        int thisLayer = gameObject.layer;

        foreach (string layerName in layersToIgnore)
        {
            int layer = LayerMask.NameToLayer(layerName);
            if (layer == -1)
            {
                Debug.LogWarning($"IgnoreLayerCollisions: Layer '{layerName}' not found!");
                continue;
            }

            // Ignore collision between this object's layer and the specified layer
            Physics.IgnoreLayerCollision(thisLayer, layer, true);
        }
    }
}
