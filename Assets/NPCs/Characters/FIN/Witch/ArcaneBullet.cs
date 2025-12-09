using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class ArcaneBullet : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 8f;
    public float lifeTime = 1f;

    [Header("Damage")]
    public int damage = 10;

    private Vector3 moveDir = Vector3.zero;
    private const float yOffset = 1.5f;   // Ziel-Punkt über dem Player

    public void Init(Transform target, float lifeTimeOverride = -1f)
    {
        if (target != null)
        {
            Vector3 targetPos = target.position + Vector3.up * yOffset;
            moveDir = (targetPos - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(moveDir);
        }

        if (lifeTimeOverride > 0f)
            lifeTime = lifeTimeOverride;
    }

    private void Start()
    {
        if (moveDir == Vector3.zero)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Vector3 targetPos = player.transform.position + Vector3.up * yOffset;
                moveDir = (targetPos - transform.position).normalized;
                transform.rotation = Quaternion.LookRotation(moveDir);
            }
        }

        Destroy(gameObject, lifeTime);

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true;

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }

    private void Update()
    {
        if (moveDir != Vector3.zero)
            transform.position += moveDir * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerHealth player = other.GetComponent<PlayerHealth>()
                           ?? other.GetComponentInParent<PlayerHealth>();

        if (player != null)
        {
            player.LooseLife(damage);
            Destroy(gameObject);
        }
    }
}
