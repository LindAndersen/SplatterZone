using UnityEngine;

public abstract class ZombieStatsBase : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Movement")]
    public float walkSpeed = 3.5f;
    public float attackRange = 2.0f;

    [Header("Combat")]
    public float attackCooldown = 1.5f;

    protected virtual void Awake()
    {
        // immer aktuellen maxHealth-Wert übernehmen
        currentHealth = maxHealth;
    }

    public virtual void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0)
            currentHealth = 0;
    }

    public bool IsDead()
    {
        return currentHealth <= 0;
    }
}
