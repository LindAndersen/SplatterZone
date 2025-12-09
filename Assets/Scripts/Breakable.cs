using UnityEngine;
using UnityEngine.AI;

public class Breakable : MonoBehaviour
{
    public int health = 3;
    public int maxHealth = 3;
    private NavMeshObstacle obstacle;
    void Start()
    {
        obstacle = GetComponent<NavMeshObstacle>();
        if (obstacle != null)
        {
            obstacle.enabled = true;
        }
    }

    public void hit()
    {
        Debug.Log("Breakable hit!");
        health--;
        if (health <= 0)
        {
            obstacle.enabled = false;
            health = 0;
        }
    }

    public void repair()
    {
        Debug.Log("Breakable repaired!");
        health = Mathf.Min(maxHealth, health + 1);
        if (obstacle != null && health > 0)
        {
            obstacle.enabled = true;
        }
    }
}
