using UnityEngine;
using UnityEngine.AI;

public interface IZombieAttackEffect
{
    void Execute(ZombieController zombie, int variant, float attackDuration);
}

[RequireComponent(typeof(NavMeshAgent))]
public class ZombieController : MonoBehaviour
{
    public enum ZombieState
    {
        Idle,
        Walking,
        Jumping,
        Attacking,
        Dead
    }

    [Header("State")]
    public ZombieState currentState = ZombieState.Idle;

    

    [Header("Animation Sync")]
    [Tooltip("Welt-Geschwindigkeit (m/s), die die Walk-Animation bei Animator.speed = 1 visuell darstellt.")]
    public float walkAnimWorldSpeed = 1.7f;

    private NavMeshAgent agent;
    private float nextAttackTime = 0f;
    private Animator animator;
    private ZombieStatsBase stats;
    public ZombieStatsBase Stats => stats;

    [HideInInspector]
    public Transform target;

    // Animator-Parameter-Hashes
    private static readonly int AnimSpeed  = Animator.StringToHash("Speed");
    private static readonly int AnimState  = Animator.StringToHash("State");
    private static readonly int AnimPunch1 = Animator.StringToHash("Punch1");
    private static readonly int AnimPunch2 = Animator.StringToHash("Punch2");

    private bool hasPunch1;
    private bool hasPunch2;

    void Awake()
    {
        agent    = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        stats    = GetComponent<ZombieStatsBase>();

        CacheAnimatorParameters();
    }

    void Start()
    {
        if (stats != null)
        {
            agent.speed = stats.walkSpeed;
            agent.stoppingDistance = stats.attackRange;
        }

        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }

        CacheAnimatorParameters();
    }

    void Update()
    {
        if (currentState == ZombieState.Dead || target == null)
            return;

        UpdateDestination();
        UpdateStateFromMovement();
        HandleJumpState();
        HandleAttackState();
        UpdateAnimator();
    }

    void UpdateDestination()
    {
        if (agent.enabled)
            agent.SetDestination(target.position);
    }

    void UpdateStateFromMovement()
    {
        if (currentState == ZombieState.Attacking || currentState == ZombieState.Jumping)
            return;

        float velocityMagnitude = agent.velocity.magnitude;

        currentState = velocityMagnitude > 0.1f
            ? ZombieState.Walking
            : ZombieState.Idle;
    }

    void HandleJumpState()
    {
        if (!agent.enabled)
            return;

        if (agent.isOnOffMeshLink)
        {
            currentState = ZombieState.Jumping;
        }
        else if (currentState == ZombieState.Jumping)
        {
            currentState = ZombieState.Walking;
        }
    }

    void HandleAttackState()
    {
        if (target == null || Stats == null || !agent.enabled)
            return;

        if (agent.pathPending)
            return;

        float dist = agent.remainingDistance;
        float stop = agent.stoppingDistance;

        if (dist <= stop + 0.05f)
        {
            currentState    = ZombieState.Attacking;
            agent.isStopped = true;

            if (Time.time >= nextAttackTime)
            {
                int variant = (Random.value < 0.5f) ? 1 : 2;
                PlayAttack(variant);
                nextAttackTime = Time.time + Stats.attackCooldown;
            }
        }
        else
        {
            if (currentState == ZombieState.Attacking)
                currentState = ZombieState.Walking;

            agent.isStopped = false;
        }
    }

    void UpdateAnimator()
    {
        if (animator == null)
            return;

        float moveSpeed = agent.enabled ? agent.velocity.magnitude : 0f;

        animator.SetFloat(AnimSpeed, moveSpeed);
        animator.SetInteger(AnimState, (int)currentState);

        if (currentState == ZombieState.Walking && walkAnimWorldSpeed > 0.01f)
        {
            float animatorSpeed = moveSpeed / walkAnimWorldSpeed;
            animator.speed = Mathf.Clamp(animatorSpeed, 0.1f, 3f);
        }
        else
        {
            animator.speed = 1f;
        }
    }

    void CacheAnimatorParameters()
    {
        if (animator == null)
            return;

        hasPunch1 = false;
        hasPunch2 = false;

        foreach (var p in animator.parameters)
        {
            if (p.nameHash == AnimPunch1 && p.type == AnimatorControllerParameterType.Trigger)
                hasPunch1 = true;

            if (p.nameHash == AnimPunch2 && p.type == AnimatorControllerParameterType.Trigger)
                hasPunch2 = true;
        }
    }

    public void PlayAttack(int variant = 1)
    {
        if (animator == null)
            return;

        if (variant == 2 && hasPunch2)
        {
            animator.SetTrigger(AnimPunch2);
        }
        else if (hasPunch1)
        {
            animator.SetTrigger(AnimPunch1);
        }

        float attackDuration = GetCurrentAttackDuration();
        TriggerAttackEffects(variant, attackDuration);
    }

    public void ApplyDamage(int amount)
    {
        if (stats == null || currentState == ZombieState.Dead)
            return;

        stats.TakeDamage(amount);

        if (stats.IsDead())
        {
            Kill();
            // Adjust float to increase/decrease score gained by this particular zombie kill
            UIManager instance = UIManager.Instance;
            if (instance != null)
                instance.AddZombieKill();

        }
    }

    public void Kill()
    {
        if (currentState == ZombieState.Dead)
            return;

        currentState = ZombieState.Dead;

        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.enabled = false;
        }

        if (animator != null)
        {
            animator.speed = 1f;
            animator.SetInteger(AnimState, (int)ZombieState.Dead);
        }
    }

    public float GetCurrentAttackDuration()
    {
        if (animator == null) return 0.5f;

        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        return info.length;
    }

    void TriggerAttackEffects(int variant, float attackDuration)
    {
        var effects = GetComponentsInChildren<IZombieAttackEffect>(true);

        foreach (var effect in effects)
            effect.Execute(this, variant, attackDuration);
    }
}
