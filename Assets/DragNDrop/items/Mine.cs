using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Mine : MonoBehaviour
{
    [Header("FX")]
    public GameObject gutHitEffectPrefab;

    [Header("Audio Prefab")]
    public GameObject explosionAudioPrefab;   // Prefab mit AudioSource drin

    [Header("Damage")]
    public int damage = 50;

    void OnTriggerEnter(Collider other)
    {
       
        if (!other.CompareTag("Player"))
            return;

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.LooseLife(damage);
        }

        // Sound + FX + Schaden
        PlayExplosionSoundPrefab();


        Vector3 explosionPos = transform.position + Vector3.up * 1.5f;
        SpawnFX(gutHitEffectPrefab, explosionPos, other.transform);

        // Mine verschwindet
        Destroy(gameObject);
    }

    void PlayExplosionSoundPrefab()
    {
        if (explosionAudioPrefab == null)
        {
            Debug.LogWarning("Mine: Kein Explosion-AudioPrefab zugewiesen!");
            return;
        }

        GameObject go = Instantiate(explosionAudioPrefab, transform.position, Quaternion.identity);

        AudioSource src = go.GetComponent<AudioSource>();
        if (src == null)
        {
            Debug.LogWarning("Mine: Explosion-AudioPrefab hat keine AudioSource!");
            Destroy(go);
            return;
        }

        src.Play();

        if (src.clip != null)
            Destroy(go, src.clip.length);
        else
            Destroy(go, 2f);
    }

    void SpawnFX(GameObject prefab, Vector3 cp, Transform parent)
    {
        if (prefab == null) return;

        GameObject fx = Instantiate(prefab, cp, Quaternion.identity);

        if (parent != null)
            fx.transform.SetParent(parent, true);
    }
}
