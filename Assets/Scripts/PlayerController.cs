using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float attackSpeed = 2f;
    private float attackCooldown = 0f;
    void OnTriggerStay(Collider other)
    {
        Breakable breakable = other.gameObject.GetComponent<Breakable>();
        if (breakable != null && (attackSpeed - attackCooldown) >= attackSpeed) 
        {
            attackCooldown = attackSpeed;
            breakable.repair();
        }
        attackCooldown -= Time.deltaTime;
    }
}
