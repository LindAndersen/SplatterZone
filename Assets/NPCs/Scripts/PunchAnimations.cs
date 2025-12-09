using UnityEngine;
using System.Collections;

public class PunchAnimations : MonoBehaviour, IZombieAttackEffect
{
    public enum EffectType { None, Blood, Arcane }
    public enum LifetimeMode { SyncToAttack, CustomLifetime }

    [Header("General")]
    public EffectType effectType = EffectType.None;
    public LifetimeMode lifetimeMode = LifetimeMode.SyncToAttack;

    public GameObject effectPrefab;
    public bool parentToBone = true;

    [Range(0, 2)]
    public int onlyForVariant = 0;

    [Header("Timing")]
    public float startDelay = 0f;
    public float customLifetime = 1.0f;

    [Header("Animation Speed")]
    public float effectAnimSpeed = 1.0f;

    // ----------------------------------------------------
    // Haupteinstieg: wird vom Zombie-Controller aufgerufen
    // ----------------------------------------------------
    public void Execute(ZombieController zombie, int variant, float attackDuration)
    {
        if (effectPrefab == null)
            return;

        if (onlyForVariant != 0 && onlyForVariant != variant)
            return;

        // WICHTIG: Coroutine auf DIESEM Script starten, nicht auf zombie
        StartCoroutine(SpawnEffectDelayed(zombie, attackDuration));
    }

    private IEnumerator SpawnEffectDelayed(ZombieController zombie, float attackDuration)
    {
        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);

        // Lebensdauer EINMAL berechnen
        float attackLeft = (lifetimeMode == LifetimeMode.SyncToAttack)
            ? Mathf.Max(0.01f, attackDuration - startDelay)
            : customLifetime;

        // Effekt instanziieren
        GameObject instance = Instantiate(effectPrefab, transform.position, transform.rotation);

        // -------------------------------
        // ARCANE PROJECTILE INITIALISIERUNG
        // -------------------------------
        ArcaneBullet arcane = instance.GetComponent<ArcaneBullet>();
        if (arcane != null && zombie != null)
        {
            arcane.Init(zombie.target, attackLeft);
        }

        // Blood / normale Effekte an Bone hängen, aber NICHT Arcane-Projektile
        if (parentToBone && arcane == null)
        {
            instance.transform.SetParent(transform, worldPositionStays: true);
            instance.transform.localScale = Vector3.one;
        }

        // Animator Speed anpassen
        Animator anim = instance.GetComponentInChildren<Animator>();
        if (anim != null)
            anim.speed = effectAnimSpeed;

        // Partikel Simulation Speed + Looping anpassen
        ParticleSystem[] pSystems = instance.GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in pSystems)
        {
            var main = ps.main;
            main.simulationSpeed = effectAnimSpeed;
            main.loop = false;                     // wichtig, damit das System irgendwann fertig ist
        }

        // -------------------------------
        // Warten bis Lebensdauer vorbei
        // -------------------------------
        yield return new WaitForSeconds(attackLeft);

        // Emission stoppen
        foreach (var ps in pSystems)
        {
            var emission = ps.emission;
            emission.enabled = false;
        }

        // Warten bis ALLE Partikel fertig sind
        bool alive = true;
        while (alive)
        {
            alive = false;
            foreach (var ps in pSystems)
            {
                if (ps != null && ps.IsAlive())
                {
                    alive = true;
                    break;
                }
            }
            yield return null;
        }

        Destroy(instance);
    }
}
