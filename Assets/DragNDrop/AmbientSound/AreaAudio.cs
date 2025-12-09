using UnityEngine;

public class AreaAudio : MonoBehaviour
{
    public Transform player;           // Player-Referenz
    public float triggerDistance = 10f;

    public GameObject audioPrefab;     // Prefab mit AudioSource (PlayOnAwake aus)
    
    GameObject activeAudio;
    AudioSource activeSource;

    void Update()
    {
        if (player == null || audioPrefab == null) return;

        float dist = Vector3.Distance(player.position, transform.position);
        bool inside = dist <= triggerDistance;

        if (inside)
            PlayAudio();
        else
            StopAudio();

        if (activeAudio != null)
            activeAudio.transform.position = transform.position;
    }

    void PlayAudio()
    {
        if (activeAudio != null) return;

        activeAudio = Instantiate(audioPrefab, transform.position, Quaternion.identity);
        activeSource = activeAudio.GetComponent<AudioSource>();

        if (activeSource == null)
        {
            Destroy(activeAudio);
            activeAudio = null;
            return;
        }

        activeSource.loop = true;
        activeSource.Play();
    }

    void StopAudio()
    {
        if (activeAudio == null) return;

        activeSource.Stop();
        Destroy(activeAudio);
        activeAudio = null;
        activeSource = null;
    }
}
