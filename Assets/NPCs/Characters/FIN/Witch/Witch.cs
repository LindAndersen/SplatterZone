using UnityEngine;

public class Witch : ZombieStatsBase
{
    protected override void Awake()
    {
        maxHealth = 150;
        walkSpeed = 4.5f;
        attackRange = 8.0f;
        attackCooldown = 6f;

        base.Awake();
    }
}

