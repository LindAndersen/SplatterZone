using UnityEngine;

public class Paler : ZombieStatsBase
{
    protected override void Awake()
    {
        // zuerst Werte setzen, dann Basis-Initialisierung
        maxHealth   = 100;
        walkSpeed   = 5f;
        attackRange = 3f;
        attackCooldown = 5f;

        base.Awake();
    }
}
