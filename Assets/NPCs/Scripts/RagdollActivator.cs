using System.Collections;
using System.Linq;
using UnityEngine;

public class RagdollActivator : MonoBehaviour
{
    [Header("Animation & Gameplay")]
    public Animator animator;
    public Behaviour[] componentsToDisable;

    [Header("Blood Settings")]
    public GameObject[] bloodPrefabs;
    public LayerMask groundLayer;
    public float minBloodLifetime = 180f;
    public float maxBloodLifetime = 300f;
    public float bloodOffset = 0.01f;
    public float bloodRaycastDistance = 5f;

    [Header("Death Decoration")]
    public GameObject[] deathPrefabs;   // optional props (weapon, limb, gore, loot)
    public float deathPrefabChance = 0.05f; // 5% default chance
    public float deathPrefabOffset = 0.05f;

    [Header("Despawn Settings")]
    public float despawnDelay = 30f;

    private Rigidbody[] ragdollBodies;

    private static readonly int HeadshotTrigger = Animator.StringToHash("Headshot");
    private static readonly int GutshotTrigger  = Animator.StringToHash("Gutshot");
    private static readonly int HitTrigger      = Animator.StringToHash("Hit");

    void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        ragdollBodies = GetComponentsInChildren<Rigidbody>().Where(rb => rb.transform != transform).ToArray();
        SetRagdoll(false);
    }

    public void SetRagdoll(bool active)
    {
        foreach (var rb in ragdollBodies)
            rb.isKinematic = !active;

        transform.GetComponent<Rigidbody>().isKinematic = false;
    }

    public void ActivateRagdoll()
    {
        if (animator != null)
            animator.enabled = false;

        if (componentsToDisable != null)
        {
            foreach (var c in componentsToDisable)
                if (c != null) c.enabled = false;
        }

        SetRagdoll(true);

        SpawnRandomBloodOnGround();
        TrySpawnDeathPrefab();

        StartCoroutine(DespawnAfterDelay());
    }

    private IEnumerator DespawnAfterDelay()
    {
        if (despawnDelay > 0f)
            yield return new WaitForSeconds(despawnDelay);

        Destroy(gameObject);
    }

    public void RemoveHead(Transform headBone)
    {
        var joints = headBone.GetComponents<Joint>();
        foreach (var j in joints)
            Destroy(j);

        var headRBs = headBone.GetComponents<Rigidbody>();
        foreach (var r in headRBs)
            Destroy(r);

        var cols = headBone.GetComponents<Collider>();
        foreach (var c in cols)
            Destroy(c);

        headBone.localScale = Vector3.zero;

        SpawnRandomBloodOnGround();
        TrySpawnDeathPrefab();
    }

    public void PlayHeadshotAnimation()
    {
        if (animator == null || !animator.enabled) return;
        animator.SetTrigger(HeadshotTrigger);
        SpawnRandomBloodOnGround();
    }

    public void PlayGutshotAnimation()
    {
        if (animator == null || !animator.enabled) return;
        animator.SetTrigger(GutshotTrigger);
        SpawnRandomBloodOnGround();
    }

    public void PlayNormalHitAnimation()
    {
        if (animator == null || !animator.enabled) return;
        animator.SetTrigger(HitTrigger);
        SpawnRandomBloodOnGround();
    }

    // ---------------- BLUT-SPAWN ----------------
    public void SpawnRandomBloodOnGround()
    {
        if (bloodPrefabs == null || bloodPrefabs.Length == 0)
            return;

        Vector3 origin = transform.position;
        Ray ray = new Ray(origin, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit, bloodRaycastDistance, groundLayer, QueryTriggerInteraction.Ignore))
        {
            int index = Random.Range(0, bloodPrefabs.Length);
            GameObject chosenBlood = bloodPrefabs[index];
            if (chosenBlood == null) return;

            Vector3 spawnPos = hit.point + hit.normal * bloodOffset;

            GameObject bloodInstance = Instantiate(chosenBlood, spawnPos, chosenBlood.transform.rotation);

            float lifeTime = Random.Range(minBloodLifetime, maxBloodLifetime);
            Destroy(bloodInstance, lifeTime);
        }
    }

    // ---------------- TODES-PREFAB-SPAWN ----------------
    private void TrySpawnDeathPrefab()
    {
        if (deathPrefabs == null || deathPrefabs.Length == 0)
            return;

        if (Random.value > deathPrefabChance)
            return; // failed probability

        Vector3 origin = transform.position + Vector3.up;
        Ray ray = new Ray(origin, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit, bloodRaycastDistance, groundLayer))
        {
            int index = Random.Range(0, deathPrefabs.Length);
            GameObject chosen = deathPrefabs[index];
            if (chosen == null) return;

            Quaternion rot = Quaternion.FromToRotation(Vector3.up, hit.normal);
            Vector3 pos = hit.point + hit.normal * deathPrefabOffset;

            Instantiate(chosen, pos, rot);
        }
    }
}
