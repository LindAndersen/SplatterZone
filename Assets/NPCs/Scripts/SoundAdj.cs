using UnityEngine;

/// Stellt Start- und Endzeit für einen Sound ein.
/// startTime: Zeitpunkt im Clip, an dem gestartet wird
/// cutTime:   Zeitpunkt im Clip, an dem gestoppt wird (falls < clip.length)
[RequireComponent(typeof(AudioSource))]
public class SoundAdj : MonoBehaviour
{
    public float startTime = 0f;    // Startposition im Clip
    public float cutTime = -1f;     // Stoppunkt im Clip (–1 = bis Clipende)

    private AudioSource a;

    void Start()
    {
        a = GetComponent<AudioSource>();
        if (a == null || a.clip == null) return;

        startTime = Mathf.Clamp(startTime, 0f, a.clip.length);
        a.time = startTime;
        a.Play();

        if (cutTime > 0f)
        {
            cutTime = Mathf.Clamp(cutTime, startTime, a.clip.length);
            float remaining = cutTime - startTime;

            if (remaining > 0f)
                Invoke(nameof(StopAndDestroy), remaining);
        }
        else
        {
            Destroy(gameObject, a.clip.length - startTime);
        }
    }

    private void StopAndDestroy()
    {
        if (a != null)
            a.Stop();

        Destroy(gameObject);
    }
}
