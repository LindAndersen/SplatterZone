using Unity.VisualScripting;
using UnityEngine;

public class ZombieControllerBarricade : MonoBehaviour
{
    public float attackSpeed = 2f;
    private float attackCooldown = 0f;
    void OnTriggerStay(Collider other)
    {
        Breakable breakable = other.gameObject.GetComponent<Breakable>();
        if (breakable != null && (attackSpeed - attackCooldown) >= attackSpeed) 
        {
            attackCooldown = attackSpeed;
            breakable.hit();
        }
        attackCooldown -= Time.deltaTime;
    }
}
