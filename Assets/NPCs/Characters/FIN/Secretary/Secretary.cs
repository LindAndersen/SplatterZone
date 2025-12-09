using UnityEngine;

public class Secretary : ZombieStatsBase
{
    protected override void Awake()
    {
        // zuerst Werte setzen, dann Basis-Initialisierung
        maxHealth   = 150;
        walkSpeed   = 3f;
        attackRange = 8f;
        attackCooldown = 10f;

        base.Awake();
    }
}
