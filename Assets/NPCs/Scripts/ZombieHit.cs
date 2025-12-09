using System.Collections;
using UnityEngine;

/// Angriffseffekt für den Zombie:
/// - Führt nach einstellbarer Anim-Zeit einen Hit aus
/// - Spielt optional nach eigener Anim-Zeit einen Sound ab (Prefab)
/// - Punch1/Punch2-Points werden hier verwaltet
public class ZombieHit : MonoBehaviour, IZombieAttackEffect
{
    [Header("References")]
    [Tooltip("Child-Objekt 'root' des Zombies. Wenn leer, wird automatisch nach einem Transform namens 'root' gesucht.")]
    private Transform root;

    [Header("Attack Points (optional)")]
    [Tooltip("Optionaler Angriffspunkt für Punch1 (z.B. rechte Hand). Wenn leer, wird nach Child 'Punch1' gesucht.")]
    public Transform punch1Point;
    [Tooltip("Optionaler Angriffspunkt für Punch2 (z.B. linke Hand). Wenn leer, wird nach Child 'Punch2' gesucht.")]
    public Transform punch2Point;

    [Header("Damage")]
    public int damage = 10;

    [Header("Damage Handling")]
    [Tooltip("Wenn aktiviert, wird der Schaden NICHT hier abgezogen, sondern extern geregelt.")]
    public bool damageHandledExternally = false;

    [Header("Hit Timing (normale Anim-Zeit 0–1)")]
    [Range(0f, 1f)]
    public float punch1HitTime = 0.3f;
    [Range(0f, 1f)]
    public float punch2HitTime = 0.5f;

    [Header("Sound Timing (normale Anim-Zeit 0–1)")]
    [Tooltip("Zeitpunkt in der Animation, wann der Sound bei Punch1 abgespielt wird.")]
    [Range(0f, 1f)]
    public float punch1SoundTime = 0.3f;
    [Tooltip("Zeitpunkt in der Animation, wann der Sound bei Punch2 abgespielt wird.")]
    [Range(0f, 1f)]
    public float punch2SoundTime = 0.5f;

    [Header("Sound Prefabs")]
    [Tooltip("Prefab mit AudioSource (und optional SoundAdj) für Punch-Variante 1.")]
    public GameObject punch1SoundPrefab;
    [Tooltip("Prefab mit AudioSource (und optional SoundAdj) für Punch-Variante 2.")]
    public GameObject punch2SoundPrefab;

    private bool attackProcessing = false;
    private bool refsInitialized = false;

    // ----------------------------------------------------------------
    public void Execute(ZombieController zombie, int variant, float attackDuration)
    {
        if (zombie == null || zombie.target == null || zombie.Stats == null)
            return;

        if (!refsInitialized)
            InitReferences(zombie.transform);

        if (root == null)
            return;

        if (attackProcessing)
            return;

        float hitTimeNorm   = (variant == 2) ? punch2HitTime   : punch1HitTime;
        float soundTimeNorm = (variant == 2) ? punch2SoundTime : punch1SoundTime;

        Debug.LogWarning($"ZombieHit: Execute Variant={variant} HitTimeNorm={hitTimeNorm} SoundTimeNorm={soundTimeNorm}");

        hitTimeNorm   = Mathf.Clamp01(hitTimeNorm);
        soundTimeNorm = Mathf.Clamp01(soundTimeNorm);

        float hitDelay   = attackDuration * hitTimeNorm;
        float soundDelay = attackDuration * soundTimeNorm;

        attackProcessing = true;
        StartCoroutine(DoHitAfterDelay(zombie, variant, hitDelay));

        // Sound getrennt vom Hit getimed
        if (GetSoundPrefab(variant) != null)
            StartCoroutine(DoSoundAfterDelay(zombie, variant, soundDelay));
    }

    // ----------------------------------------------------------------
    private void InitReferences(Transform zombieTransform)
    {
        if (root == null)
            root = FindByNameOrFallback(zombieTransform, "root");

        if (punch1Point == null)
            punch1Point = FindByName(zombieTransform, "Punch1");

        if (punch2Point == null)
            punch2Point = FindByName(zombieTransform, "Punch2");

        refsInitialized = true;
    }

    // ----------------------------------------------------------------
    private IEnumerator DoHitAfterDelay(ZombieController zombie, int variant, float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        if (zombie == null || zombie.target == null || zombie.Stats == null || root == null)
        {
            attackProcessing = false;
            yield break;
        }

        float dist      = Vector3.Distance(root.position, zombie.target.position);
        float hitRadius = zombie.Stats.attackRange;

        bool hit = dist <= hitRadius + 1f;

        if (hit)
        {
            if (!damageHandledExternally)
            {
                PlayerHealth health = zombie.target.GetComponent<PlayerHealth>();
                if (health != null)
                    health.LooseLife(damage);
            }
        }

        attackProcessing = false;
    }

    // ----------------------------------------------------------------
    private IEnumerator DoSoundAfterDelay(ZombieController zombie, int variant, float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        if (zombie == null || root == null)
            yield break;

        PlayHitSound(variant);
    }

    // ----------------------------------------------------------------
    private void PlayHitSound(int variant)
    {
        GameObject prefab = GetSoundPrefab(variant);
        if (prefab == null)
            return;

        Transform soundPoint = GetSoundPoint(variant);
        Vector3 pos = (soundPoint != null) ? soundPoint.position : root.position;

        GameObject instance = Instantiate(prefab, pos, Quaternion.identity);

        AudioSource src = instance.GetComponent<AudioSource>();
        if (src != null)
        {
            src.spatialBlend = 1f;
            src.rolloffMode = AudioRolloffMode.Logarithmic;
            src.minDistance = 1.5f;
            src.maxDistance = 30f;
            src.playOnAwake = false;
            src.loop = false;

            if (src.clip != null)
                Destroy(instance, src.clip.length + 0.1f);
            else
                Destroy(instance, 2f);
        }
        else
        {
            Destroy(instance, 2f);
        }
    }

    private GameObject GetSoundPrefab(int variant)
    {
        if (variant == 1)
        {
            Debug.LogWarning("ZombieHit: Punch1");
            return punch1SoundPrefab;
        }
        if (variant == 2)
        {
            Debug.LogWarning("ZombieHit: Punch2");
            return punch2SoundPrefab;
        }
        return null;
    }

    private Transform GetSoundPoint(int variant)
    {
        if (variant == 1 && punch1Point != null)
            return punch1Point;
        if (variant == 2 && punch2Point != null)
            return punch2Point;
        return root;
    }

    // ----------------------------------------------------------------
    private Transform FindByNameOrFallback(Transform rootTransform, string name)
    {
        Transform t = FindByName(rootTransform, name);
        return t != null ? t : rootTransform;
    }

    private Transform FindByName(Transform rootTransform, string name)
    {
        foreach (var t in rootTransform.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == name)
                return t;
        }
        return null;
    }
}
