using UnityEngine;
using System.Collections;

/// <summary>
/// Manages a pool of particle effects for efficient reuse without instantiation/destruction.
/// </summary>
public class ParticleEffectPool : MonoBehaviour
{
    [SerializeField] private GameObject effectPrefab;
    [SerializeField] private int poolSize = 5;

    private ParticleSystem[] pool;
    private int nextIndex = 0;

    void Awake()
    {
        InitializePool();
    }

    void InitializePool()
    {
        if (effectPrefab == null)
        {
            Debug.LogWarning("[ParticleEffectPool] Effect prefab is not assigned!");
            return;
        }

        pool = new ParticleSystem[poolSize];
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(effectPrefab, transform);
            obj.name = effectPrefab.name + " (Pooled " + i + ")";
            obj.SetActive(false);

            ParticleSystem ps = obj.GetComponent<ParticleSystem>();
            if (ps == null)
            {
                Debug.LogWarning("[ParticleEffectPool] Effect prefab has no ParticleSystem component!");
                continue;
            }

            pool[i] = ps;
        }
    }

    /// <summary>
    /// Play an effect at the given position.
    /// </summary>
    public void PlayEffect(Vector3 position)
    {
        if (pool == null || pool.Length == 0)
        {
            Debug.LogWarning("[ParticleEffectPool] Pool is not initialized!");
            return;
        }

        ParticleSystem ps = pool[nextIndex];
        nextIndex = (nextIndex + 1) % pool.Length;

        if (ps == null)
        {
            return;
        }

        ps.gameObject.SetActive(true);
        ps.gameObject.transform.position = position;
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.Play();

        // Schedule deactivation after effect finishes
        StartCoroutine(DeactivateAfterDelay(ps));
    }

    IEnumerator DeactivateAfterDelay(ParticleSystem ps)
    {
        float duration = ps.main.duration;
        float maxLifetime = ps.main.startLifetime.constantMax;
        float totalTime = duration + maxLifetime;

        yield return new WaitForSeconds(totalTime);

        if (ps != null && ps.gameObject != null)
        {
            ps.gameObject.SetActive(false);
        }
    }
}
