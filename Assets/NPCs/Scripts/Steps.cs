using UnityEngine;

public class Steps : MonoBehaviour
{
    private Animator animator;
    public GameObject audioPrefab; // your footstep prefab

    private AudioSource sourceInstance;

    [Header("Step Settings")]
    public float basePitch = 1f;
    public float pitchMultiplier = 1f;
    public float minPitch = 0.5f;
    public float maxPitch = 2f;

    void Start()
    {
        animator = GetComponent<Animator>();

        // Create an instance of your prefab so the AudioSource is active
        GameObject obj = Instantiate(audioPrefab, transform);
        sourceInstance = obj.GetComponent<AudioSource>();

        // Ensure footstep audio is 3D and follows this character so players can localize it.
        if (sourceInstance != null)
        {
            sourceInstance.spatialBlend = 1f;                 // fully 3D
            sourceInstance.rolloffMode = AudioRolloffMode.Logarithmic;
            sourceInstance.minDistance = 1.5f;
            sourceInstance.maxDistance = 30f;
            sourceInstance.spread = 0f;                      // keep mono; adjust if you want wider stereo
            sourceInstance.playOnAwake = false;
            sourceInstance.loop = false;                     // we manually trigger playback
        }
    }

    void Update()
    {
        if (sourceInstance == null) return;

        float speed = animator.GetFloat("Speed");

        float targetPitch = basePitch + speed * pitchMultiplier;
        
        sourceInstance.pitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);

        if (speed > 0.1f)
        {
            if (!sourceInstance.isPlaying)
                sourceInstance.Play();
        }
        else
        {
            sourceInstance.Stop();
        }
    }
}
