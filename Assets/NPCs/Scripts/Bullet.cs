using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float lifeTime = 3f;
    public float impulseMultiplier = 1.0f;

    [Header("Damage")]
    public int bodyDamage = 25;
    public int headDamage = 100;
    public int gutDamage  = 40;

    [Header("Hit Effects")]
    public GameObject hitEffectPrefab;
    public GameObject headHitEffectPrefab;
    public GameObject gutHitEffectPrefab;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        ContactPoint cp  = collision.GetContact(0);
        Collider hitCol  = collision.collider;

        // Versuche IMMER den Zombie über die Hierarchie zu finden
        ZombieController zombie = hitCol.GetComponentInParent<ZombieController>();
        ZombieStatsBase  stats  = hitCol.GetComponentInParent<ZombieStatsBase>();
        RagdollActivator ragdoll = hitCol.GetComponentInParent<RagdollActivator>();

        // Kein Zombie → evtl. generischer Einschlag, dann Kugel weg
        if (zombie == null || stats == null)
        {
            //SpawnFX(hitEffectPrefab, cp, null);
            Destroy(gameObject);
            return;
        }

        // -------- Trefferzone: nur Collider-Tag zählt --------
        // "Head" → Kopfschuss-Schaden
        // "Gut"  → Bauch-Schaden
        // alles andere (auch leerer Tag) → Body
        string zone = hitCol.tag; // kann "" sein
        int damage;
        GameObject fxPrefab;

        switch (zone)
        {
            case "Head":
                damage   = headDamage;
                // fxPrefab = headHitEffectPrefab;
                fxPrefab = gutHitEffectPrefab;
                break;

            case "Gut":
                damage   = gutDamage;
                fxPrefab = gutHitEffectPrefab;
                break;

            default:
                damage   = bodyDamage;
                fxPrefab = hitEffectPrefab;
                break;
        }

        // Debug zum Testen – später rauslöschen
        //Debug.Log($"Bullet hit {hitCol.name} (tag='{zone}') → damage {damage}");

        // Effekt spawnen (an Zombie-Root hängen, damit er "mitfliegt")
        SpawnFX(fxPrefab, cp, stats.transform);

        // Schaden anwenden – ZombieController übernimmt HP-Check + Kill()
        zombie.ApplyDamage(damage);

        // Ragdoll / Hit-Animation (optional)
        if (ragdoll != null)
        {
            // Du kannst hier selbst entscheiden, ob nur bei Tod Ragdoll aktivieren
            if (stats.IsDead())
            {
                ragdoll.ActivateRagdoll();
            }

            if (zone == "Head")
                ragdoll.PlayHeadshotAnimation();
            else if (zone == "Gut")
                ragdoll.PlayGutshotAnimation();
            else
                ragdoll.PlayNormalHitAnimation();
        }

        // Rückstoß auf getroffenes Körperteil, falls Rigidbody vorhanden
        Rigidbody hitBody = collision.rigidbody;
        if (hitBody == null)
            hitBody = hitCol.GetComponentInParent<Rigidbody>();

        if (hitBody != null)
        {
            Vector3 impulse = rb.linearVelocity * rb.mass * impulseMultiplier;
            hitBody.AddForceAtPosition(impulse, cp.point, ForceMode.Impulse);
        }

        Destroy(gameObject);
    }

    void SpawnFX(GameObject prefab, ContactPoint cp, Transform parent)
    {
        if (prefab == null) return;

        GameObject fx = Instantiate(
            prefab,
            cp.point,
            Quaternion.LookRotation(cp.normal)
        );

        if (parent != null)
            fx.transform.SetParent(parent, true);
    }
}
