using UnityEngine;

public class Parasite : ZombieStatsBase
{
    protected override void Awake()
    {
        // zuerst Werte setzen, dann Basis-Initialisierung
        maxHealth   = 300;
        walkSpeed   = 2f;
        attackRange = 2f;
        attackCooldown = 8f;

        base.Awake();
    }
}
