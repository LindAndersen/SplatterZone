using UnityEngine;
using UnityEngine.AI;

public class ZombieFollow : MonoBehaviour
{
    private Transform target;
    private NavMeshAgent agent;
    
    void Start()
    {
        target = GameObject.FindWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (agent == null) return;
        if (target == null) return;

        // Try to set player as destination
        agent.SetDestination(target.position);
    }
}
