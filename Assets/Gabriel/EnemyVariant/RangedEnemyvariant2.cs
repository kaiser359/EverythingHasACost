using UnityEngine;

public class RangedEnemyvariant2 : MonoBehaviour
{
    [Header("References")]
    public EnemyStats stats;
    public GameObject bulletPrefab;


    [Header("Projectile")]
    public int bulletsCount = 12;
    public float projectileSpeed = 5f;
    public float spawnRadius = 0.5f;
    public float spreadAngle = 10f; // degrees between side bullets and center

    [Header("Pattern")]
    public float angleIncrementPerShot = 15f;

    [Header("Detection")]
    public float detectionRange = 10f;

    private Transform playerTransform;

    [Header("Movement")]
    public float chaseRange = 15f;
    public float chaseSpeed = 2.5f;
    public float wanderRadius = 2f;
    public float wanderSpeed = 0.8f;
    public float idleMin = 0.5f;
    public float idleMax = 2f;

    private Vector3 originPosition;
    private Vector3 wanderTarget;
    private float wanderIdleTimer = 0f;
    private bool isIdling = false;
    private Rigidbody2D rb;

    float shootTimer = 0f;
    float currentAngleOffset = 0f;
    public Levels level;
    void Start()
    {
        if (stats == null)
            Debug.LogWarning("EnemyStats not assigned on " + name);

        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) playerTransform = p.transform;
        else Debug.LogWarning("Player object with tag 'Player' not found for " + name);

        originPosition = transform.position;
        PickNewWanderTarget();
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Ensure dynamic body so collisions with walls are resolved by physics
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            // make enemy heavy so player bangs don't readily move it
            rb.mass = Mathf.Max(rb.mass, 100f);
        }
    }
    private void Awake()
    {
        bulletsCount = bulletsCount + (level.levelNumber * 2);
    }

    void Update()
    {

        float atkSpeed = stats != null ? stats.atkSpeed : 1f;
        float interval = 1f / Mathf.Max(0.1f, atkSpeed);

        shootTimer -= Time.deltaTime;

        float dist = playerTransform != null ? Vector2.Distance(transform.position, playerTransform.position) : Mathf.Infinity;

        // If player is within detection range -> stop moving and shoot
        if (dist <= detectionRange)
        {
            // shooting behavior
            if (shootTimer <= 0f)
            {
                ShootRing();
                shootTimer = interval;
                currentAngleOffset += angleIncrementPerShot;
            }
        }
        else if (playerTransform != null && dist <= chaseRange)
        {
            // chassing
            chaseRange = 10;
            Vector3 dir = (playerTransform.position - transform.position).normalized;
            Vector3 targetPos = transform.position + dir;
            Vector3 newPos = Vector3.MoveTowards(transform.position, targetPos, chaseSpeed * Time.deltaTime);
            if (rb != null) rb.MovePosition(newPos);
            else transform.position = newPos;
        }
        else
        {
            // Wander around origin with occasional stops, minecragt styles
            if (isIdling)
            {
                wanderIdleTimer -= Time.deltaTime;
                if (wanderIdleTimer <= 0f)
                {
                    isIdling = false;
                    PickNewWanderTarget();
                }
            }
            else
            {
                Vector3 newPos = Vector3.MoveTowards(transform.position, wanderTarget, wanderSpeed * Time.deltaTime);
                if (rb != null) rb.MovePosition(newPos);
                else transform.position = newPos;
                if (Vector2.Distance(transform.position, wanderTarget) < 0.1f)
                {
                    isIdling = true;
                    wanderIdleTimer = Random.Range(idleMin, idleMax);
                }
            }
        }
    }

    void PickNewWanderTarget()
    {
        Vector2 offset = Random.insideUnitCircle * wanderRadius;
        wanderTarget = originPosition + (Vector3)offset;
    }

    void OnCollisionStay2D(Collision2D other)
    {
        if (other.collider.CompareTag("Player") && rb != null)
        {
            // prevent player from pushing this enemy while keeping collision for damage
            rb.linearVelocity = Vector2.zero;
        }
    }

    void ShootRing()
    {
        if (bulletPrefab == null)
            return;

        // Shoot 3 bullets aimed at the player (center + two side bullets)
        Vector2 baseDir = Vector2.right;
        if (playerTransform != null)
        {
            baseDir = (playerTransform.position - transform.position);
            if (baseDir.sqrMagnitude < 0.0001f) baseDir = Vector2.right;
            baseDir = baseDir.normalized;
        }

        float[] angles = new float[] { -spreadAngle, 0f, spreadAngle };
        foreach (var a in angles)
        {
            Vector2 dir = Quaternion.Euler(0f, 0f, a) * baseDir;
            Vector3 spawnPos = transform.position + (Vector3)(dir * spawnRadius);
            GameObject b = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
            Rigidbody2D brb = b.GetComponent<Rigidbody2D>();
            if (brb != null)
            {
                brb.linearVelocity = dir * projectileSpeed;
            }
        }
    }
}
