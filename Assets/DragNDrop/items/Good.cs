using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Good : MonoBehaviour
{
    [Header("Audio Prefab")]
    public GameObject audioPrefab;   // Prefab mit AudioSource drin

    [Header("Heal")]
    public int healAmount = 20;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.GainHealth(healAmount);
        }

        PlayPrefabSound();

        Destroy(gameObject);
    }

    void PlayPrefabSound()
    {
        if (audioPrefab == null)
        {
            Debug.LogWarning("Good: Kein Audio-Prefab zugewiesen!");
            return;
        }

        // Prefab instanziert → muss eine AudioSource enthalten!
        GameObject go = Instantiate(audioPrefab, transform.position, Quaternion.identity);

        AudioSource src = go.GetComponent<AudioSource>();
        if (src == null)
        {
            Debug.LogWarning("Good: Prefab hat keine AudioSource!");
            Destroy(go);
            return;
        }

        src.Play();

        // Objekt nach Clip-Länge wieder löschen
        Destroy(go, src.clip.length);
    }
}
