using UnityEngine;

public class Bloater : ZombieStatsBase
{
    protected override void Awake()
    {
        // zuerst Werte setzen, dann Basis-Initialisierung
        maxHealth   = 200;
        walkSpeed   = 2f;
        attackRange = 2f;
        attackCooldown = 7f;

        base.Awake();
    }
}
