using UnityEngine;
using UnityEngine.AI;

public class Breakable : MonoBehaviour
{
    [Header("References")]
    public GameObject breakZone;
    public GameObject repairZone;
    private NavMeshObstacle obstacle;

    [Header("Health")]
    public int health = 3;
    public int maxHealth = 3;

    [Header("Player Repair")]
    public float playerAttackSpeed = 1f;
    private bool playerInRepairZone = false;
    private float repairCooldown = 0f;
    
    [Header("Zombie Damage")]
    public float zombieAttackSpeed = 2f;
    private float damageCooldown = 0f;
    private Collider[] zombiesOnObstacle = new Collider[10];

    [Header("Audio")]
    public AudioClip repairSound;
    public AudioClip hitSound;
    public AudioClip breakSound;

    [Header("Visual Planks")]
    public GameObject[] planks = new GameObject[3];

    [Header("Particle Effects")]
    public GameObject repairEffectPrefab;
    public GameObject breakEffectPrefab;

    void Start()
    {
        if (breakZone != null)
            obstacle = breakZone.GetComponent<NavMeshObstacle>();
    }

    void Update()
    {
        HandlePlayerRepair();
        HandleZombieDamage();
    }

    void HandlePlayerRepair()
    {
        if (!playerInRepairZone || health >= maxHealth)
            return;

        if (Input.GetKey(KeyCode.F))
        {
            repairCooldown -= Time.deltaTime;
            if (repairCooldown <= 0f)
            {
                Repair();
                repairCooldown = playerAttackSpeed;
            }
        }
    }

    void HandleZombieDamage()
    {
        if (obstacle == null || !obstacle.enabled)
            return;

        // Find all colliders touching the NavMeshObstacle
        // Apply center offset in world space
        Vector3 boxCenter = obstacle.transform.position + obstacle.center;
        float detectionRadius = obstacle.size.y + 1f; // Slightly larger to ensure detection
        int count = Physics.OverlapSphereNonAlloc(boxCenter, detectionRadius, zombiesOnObstacle);

        for (int i = 0; i < count; i++)
        {
            if (zombiesOnObstacle[i].CompareTag("Zombie"))
            {
                NavMeshAgent agent = zombiesOnObstacle[i].GetComponent<NavMeshAgent>();
                if (agent == null)
                    continue;

                // Check if zombie is dead
                ZombieStatsBase stats = zombiesOnObstacle[i].GetComponent<ZombieStatsBase>();
                if (stats != null && stats.IsDead())
                    continue;

                damageCooldown -= Time.deltaTime;
                if (damageCooldown <= 0f)
                {
                    Hit();
                    damageCooldown = zombieAttackSpeed;
                }
                break; // Only one zombie damages per cycle
            }
        }
    }

    public void SetPlayerInRepairZone(bool inZone)
    {
        playerInRepairZone = inZone;
    }

    public void Hit()
    {
        Debug.Log("Breakable hit by zombie!");
        health--;
        health = Mathf.Max(0, health);

        // Trigger break animation on the plank corresponding to current health
        // Health 2 = plank[0] breaks, Health 1 = plank[1] breaks, Health 0 = plank[2] breaks
        int plankIndex = maxHealth - health - 1;
        if (plankIndex >= 0 && plankIndex < planks.Length && planks[plankIndex] != null)
        {
            Debug.Log("Playing break animation on plank index: " + plankIndex);
            Animator animator = planks[plankIndex].GetComponentInChildren<Animator>();
            if (animator != null)
            {                
                animator.SetTrigger("break");
            }
            else
            {
                Debug.LogWarning("No Animator found on plank " + plankIndex);
            }

            // Play break effect at plank position
            PlayEffectAt(breakEffectPrefab, planks[plankIndex].transform.position);
        }

        if (health <= 0 && obstacle != null)
        {
            obstacle.enabled = false;
            PlaySound(breakSound);
        }
        else
        {
            PlaySound(hitSound);
        }
    }

    public void Repair()
    {
        Debug.Log("Breakable repaired!");
        int previousHealth = health;
        health++;
        health = Mathf.Min(maxHealth, health);

        // Trigger repair animation on the plank that was just repaired
        // Repairs the plank that broke when we had this previous health
        int plankIndex = maxHealth - previousHealth - 1;
        if (plankIndex >= 0 && plankIndex < planks.Length && planks[plankIndex] != null)
        {
            Debug.Log("Playing repair animation on plank index: " + plankIndex);
            Animator animator = planks[plankIndex].GetComponentInChildren<Animator>();
            if (animator != null)
            {
                animator.SetTrigger("repair");
            }
            else
            {
                Debug.LogWarning("No Animator found on plank " + plankIndex);
            }

            // Play repair effect at plank position
            PlayEffectAt(repairEffectPrefab, planks[plankIndex].transform.position);
        }

        if (health > 0 && obstacle != null)
        {
            obstacle.enabled = true;
        }

        PlaySound(repairSound);
    }

    void PlaySound(AudioClip clip)
    {
        if (clip == null)
            return;

        AudioSource.PlayClipAtPoint(clip, transform.position);
    }

    void PlayEffectAt(GameObject effectPrefab, Vector3 position)
    {
        if (effectPrefab == null)
            return;

        Instantiate(effectPrefab, position, Quaternion.identity);
    }

    void OnDrawGizmos()
    {
        if (breakZone == null)
            return;

        NavMeshObstacle obs = breakZone.GetComponent<NavMeshObstacle>();
        if (obs == null)
            return;

        // Apply center offset in world space
        Vector3 boxCenter = obs.transform.position + obs.center;
        float detectionRadius = obs.size.y + 1f; // Slightly larger to ensure detection

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(boxCenter, detectionRadius);
    }
}


