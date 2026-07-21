using System.Collections;
using UnityEngine;

/// <summary>
/// Complex 2D top-down enemy:
/// - Auto-assigns player on spawn (GameObject tagged "Player").
/// - Patrols when player is far.
/// - Chases + strafes + shoots when in shooting range (with cooldown).
/// - Detects incoming bullets using 2D overlap/casts (Collider2D aware).
/// - Attempts to dodge or use nearby walls (obstacles) as cover.
/// - Dodge has its own cooldown to avoid constant dodging.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class AiStudyTest1 : MonoBehaviour
{
    [Header("References")]
    public Transform firePoint;
    public GameObject bulletPrefab; // Prefab must have Rigidbody2D
    public LayerMask bulletLayer;
    public LayerMask obstacleLayer;
    public string playerTag = "Player";

    [Header("Ranges & Movement")]
    public float detectionRange = 12f;
    public float shootRange = 9f;
    public float chaseRange = 14f;
    public float patrolRadius = 6f;
    public float patrolSpeed = 1.0f;
    public float chaseSpeed = 2.2f;
    public float dodgeSpeed = 6f;
    public float dodgeDistance = 2.0f;
    public float strafeAmplitude = 0.8f;
    public float strafeSpeed = 3f;

    [Header("Shooting")]
    public float shootCooldown = 1.2f;
    public float bulletSpeed = 12f;

    [Header("Bullet Threat Detection")]
    public float bulletDetectRadius = 6f; // search radius for nearby projectiles
    public float threatTimeThreshold = 0.8f; // seconds until predicted hit to be considered threat
    public float dodgeCooldown = 1.5f; // after a dodge, wait before next dodge

    // Internals
    private Transform playerTransform;
    private Rigidbody2D rb;
    private Vector3 origin;
    private Vector3 patrolTarget;
    private float patrolIdleTimer;
    private bool isIdling;

    private float shootTimer;
    private float dodgeTimer;
    private float dodgeCooldownTimer;
    private bool isDodging;
    private Vector2 dodgeTarget;
    private float currentStateStrafeOffset; // used for smooth strafing

    void Start()
    {
        origin = transform.position;
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Auto assign player
        var p = GameObject.FindGameObjectWithTag(playerTag);
        if (p != null) playerTransform = p.transform;

        PickNewPatrolTarget();
    }

    void Update()
    {
        // Timers
        shootTimer -= Time.deltaTime;
        dodgeTimer -= Time.deltaTime;
        dodgeCooldownTimer -= Time.deltaTime;

        // If player got destroyed or not set, try to find again
        if (playerTransform == null)
        {
            var p = GameObject.FindGameObjectWithTag(playerTag);
            if (p != null) playerTransform = p.transform;
        }

        // Always check for incoming bullets first (high priority)
        bool foundThreat = false;
        if (dodgeCooldownTimer <= 0f)
        {
            foundThreat = DetectAndReactToIncomingBullets();
        }

        if (isDodging)
        {
            PerformDodgeMovement();
            return; // while dodging, skip other behaviors
        }

        float playerDist = playerTransform != null ? Vector2.Distance(transform.position, playerTransform.position) : Mathf.Infinity;

        // If player is within detection range, engage
        if (playerTransform != null && playerDist <= detectionRange)
        {
            // If within shooting range, strafe and shoot
            if (playerDist <= shootRange)
            {
                StrafeAndShoot();
            }
            // If within chase range but outside shoot range, move toward player
            else if (playerDist <= chaseRange)
            {
                MoveTowards(playerTransform.position, chaseSpeed);
            }
        }
        else
        {
            // Patrol behavior
            Patrol();
        }
    }

    // Patrol: pick random point in patrolRadius around origin and move there, with idle
    void Patrol()
    {
        if (isIdling)
        {
            patrolIdleTimer -= Time.deltaTime;
            if (patrolIdleTimer <= 0f)
            {
                isIdling = false;
                PickNewPatrolTarget();
            }
            return;
        }

        MoveTowards(patrolTarget, patrolSpeed);

        if (Vector2.Distance(transform.position, patrolTarget) < 0.2f)
        {
            isIdling = true;
            patrolIdleTimer = Random.Range(0.6f, 2.0f);
        }
    }

    void PickNewPatrolTarget()
    {
        Vector2 offset = Random.insideUnitCircle * patrolRadius;
        patrolTarget = origin + (Vector3)offset;
    }

    // Move toward a target position using Rigidbody2D.MovePosition (physics-friendly)
    void MoveTowards(Vector2 targetPos, float speed)
    {
        Vector2 newPos = Vector2.MoveTowards(rb.position, targetPos, speed * Time.deltaTime);
        rb.MovePosition(newPos);
    }

    // Strafes (side-to-side) while shooting periodic bullets
    void StrafeAndShoot()
    {
        if (playerTransform == null) return;

        // Face toward player (for external visuals/aiming)
        Vector2 toPlayer = (playerTransform.position - transform.position).normalized;

        // Strafe offset perpendicular to aim
        Vector2 perp = new Vector2(-toPlayer.y, toPlayer.x);
        currentStateStrafeOffset = Mathf.Sin(Time.time * strafeSpeed) * strafeAmplitude;

        Vector2 strafeTarget = (Vector2)transform.position + perp * currentStateStrafeOffset;
        // Also slowly slide toward/away to maintain engagement distance
        Vector2 approach = (Vector2)transform.position + toPlayer * 0.1f;

        Vector2 combined = Vector2.Lerp(strafeTarget, approach, 0.2f);

        MoveTowards(combined, chaseSpeed);

        // Shooting
        if (shootTimer <= 0f && bulletPrefab != null && firePoint != null)
        {
            ShootAtPlayer();
            shootTimer = shootCooldown;
        }
    }

    void ShootAtPlayer()
    {
        if (playerTransform == null) return;
        var dir = (playerTransform.position - firePoint.position).normalized;
        GameObject b = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        var rbBullet = b.GetComponent<Rigidbody2D>();
        if (rbBullet != null) rbBullet.linearVelocity = dir * bulletSpeed;
    }

    // Detect nearby projectiles (using OverlapCircleAll) and attempt to dodge or use cover.
    // Returns true when a threat was found and reaction initiated.
    bool DetectAndReactToIncomingBullets()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, bulletDetectRadius, bulletLayer);
        Vector2 selfPos = transform.position;

        foreach (var col in hits)
        {
            if (col == null) continue;

            // get projectile rigidbody to estimate velocity
            var projRb = col.attachedRigidbody;
            if (projRb == null) continue;

            Vector2 bPos = projRb.position;
            Vector2 v = projRb.linearVelocity;
            if (v.sqrMagnitude < 0.01f) continue;

            // project time to nearest approach: t = dot(e - b, v) / |v|^2
            Vector2 eMinusB = (Vector2)selfPos - bPos;
            float t = Vector2.Dot(eMinusB, v) / v.sqrMagnitude;
            if (t <= 0f || t > threatTimeThreshold) continue; // not an imminent forward threat

            // predicted closest point
            Vector2 predicted = bPos + v * t;
            float distAtClosest = Vector2.Distance(predicted, selfPos);

            // if predicted closest approach is small enough consider it a threat
            if (distAtClosest <= 0.6f)
            {
                // try to find cover behind an obstacle
                if (TryFindCoverAgainstProjectile(bPos, v, out Vector2 coverTarget))
                {
                    StartDodgeTo(coverTarget);
                    return true;
                }
                else
                {
                    // no cover found -> dodge perpendicular to projectile travel
                    Vector2 perp = new Vector2(-v.y, v.x).normalized;
                    // choose the side with more free space
                    Vector2 candidateA = selfPos + perp * dodgeDistance;
                    Vector2 candidateB = selfPos - perp * dodgeDistance;
                    bool aFree = !Physics2D.Raycast(selfPos, (candidateA - selfPos).normalized, dodgeDistance, obstacleLayer);
                    bool bFree = !Physics2D.Raycast(selfPos, (candidateB - selfPos).normalized, dodgeDistance, obstacleLayer);

                    Vector2 chosen = aFree ? candidateA : (bFree ? candidateB : candidateA);
                    StartDodgeTo(chosen);
                    return true;
                }
            }
        }

        return false;
    }

    // Try to find a nearby obstacle (obstacleLayer) that can act as cover from bullet origin.
    // If found, returns a point on the safe side of the obstacle to move toward.
    bool TryFindCoverAgainstProjectile(Vector2 bulletPos, Vector2 bulletVelocity, out Vector2 outCoverPoint)
    {
        outCoverPoint = Vector2.zero;
        float searchRadius = 6f;
        Collider2D[] obstacles = Physics2D.OverlapCircleAll(transform.position, searchRadius, obstacleLayer);
        if (obstacles == null || obstacles.Length == 0) return false;

        Vector2 selfPos = transform.position;
        float bestScore = float.MaxValue;
        bool found = false;
        foreach (var obs in obstacles)
        {
            if (obs == null) continue;

            // get closest point on obstacle to this enemy
            Vector2 coverPoint = obs.ClosestPoint(selfPos);

            // check whether obstacle blocks the line from bullet to coverPoint
            Vector2 dirFromBullet = (coverPoint - bulletPos);
            float dist = dirFromBullet.magnitude;
            if (dist <= 0.05f) continue;

            RaycastHit2D hit = Physics2D.Raycast(bulletPos, dirFromBullet.normalized, dist + 0.01f, obstacleLayer | bulletLayer);
            if (hit.collider == null) continue;

            // ensure the hit collider is the obstacle (i.e., the bullet would hit obstacle before reaching coverPoint)
            if (hit.collider != obs) continue;

            // now compute how safe the coverPoint is by how far it is from predicted bullet path
            // pick the side of obstacle that places obstacle between bullet and enemy
            Vector2 dirBulletToEnemy = (selfPos - bulletPos).normalized;
            Vector2 normal = (coverPoint - (Vector2)obs.bounds.center).normalized;
            Vector2 safePoint = coverPoint + normal * 0.3f; // step a bit behind the obstacle

            float score = Vector2.Distance(selfPos, safePoint); // prefer closer safe spots
            if (score < bestScore)
            {
                bestScore = score;
                outCoverPoint = safePoint;
                found = true;
            }
        }

        return found;
    }

    // Begin dodge movement toward a world position
    void StartDodgeTo(Vector2 target)
    {
        isDodging = true;
        dodgeTarget = target;
        dodgeTimer = 0.35f; // maintain dodge for a short window
        dodgeCooldownTimer = dodgeCooldown;
    }

    void PerformDodgeMovement()
    {
        // Move quickly toward dodgeTarget while timer active
        Vector2 newPos = Vector2.MoveTowards(rb.position, dodgeTarget, dodgeSpeed * Time.deltaTime);
        rb.MovePosition(newPos);

        dodgeTimer -= Time.deltaTime;
        if (dodgeTimer <= 0f)
        {
            isDodging = false;
        }
    }

    // Debug drawing for editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, bulletDetectRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootRange);
    }
}