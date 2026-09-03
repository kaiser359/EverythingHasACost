using System.Collections;
using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(Rigidbody2D))]
public class AiStudyTest1 : MonoBehaviour
{
    [Header("References")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    public LayerMask bulletLayer;
    public LayerMask obstacleLayer;
    public string playerTag = "Player";
  

    [Header("Ranges & Movement")]
    public float detectionRange = 12f;
    public float shootRange = 9f;
    public float closeRangeDistance = 3f;
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
    public float bulletDetectRadius = 6f;
    public float threatTimeThreshold = 0.8f;
    public float dodgeCooldown = 1.5f;

    private Transform playerTransform;
    private Rigidbody2D rb;
    private Vector3 origin;
    private Vector3 patrolTarget;
    private float patrolIdleTimer;
    private bool isIdling;
    private float shootTimer;
    private float dodgeTimer;
    private float dodgeCooldownTimer;
    public bool isDodging;
    private Vector2 dodgeTarget;
    private IAstarAI ai;
    private EnemyHealth enemyHealth;

    void Start()
    {
        origin = transform.position;
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        var p = GameObject.FindGameObjectWithTag(playerTag);
        if (p != null) playerTransform = p.transform;

        ai = GetComponent<IAstarAI>();
        if (ai != null)
        {
            ai.canMove = true;
            ai.canSearch = true;
            ai.isStopped = false;
            ai.maxSpeed = chaseSpeed;
        }

        enemyHealth = GetComponent<EnemyHealth>();
        PickNewPatrolTarget();
    }

    void Update()
    {
        shootTimer -= Time.deltaTime;
        dodgeTimer -= Time.deltaTime;
        dodgeCooldownTimer -= Time.deltaTime;

        if (playerTransform == null)
        {
            var p = GameObject.FindGameObjectWithTag(playerTag);
            if (p != null) playerTransform = p.transform;
        }

        if (dodgeCooldownTimer <= 0f)
            DetectAndReactToIncomingBullets();

        if (isDodging)
        {
            if (ai != null) ai.isStopped = true;
            PerformDodgeMovement();
            return;
        }
        else
        {
            if (ai != null) ai.isStopped = false;
        }

        float playerDist = playerTransform != null ? Vector2.Distance(transform.position, playerTransform.position) : Mathf.Infinity;

        if (playerTransform != null && playerDist <= detectionRange)
        {
            if (playerDist <= shootRange)
            {
                if (HasLineOfSightToPlayer())
                {
                    if (playerDist <= closeRangeDistance)
                        ShootSpreadPattern();
                    else
                        ShootAtPlayer();
                    StrafeTowardPlayer();
                }
                else
                {
                    SetDestination(playerTransform.position, chaseSpeed);
                }
            }
            else if (playerDist <= chaseRange)
                SetDestination(playerTransform.position, chaseSpeed);
        }
        else
        {
            Patrol();
        }
    }

    bool HasLineOfSightToPlayer()
    {
        if (playerTransform == null) return false;
        
        Vector2 dirToPlayer = (playerTransform.position - firePoint.position).normalized;
        float distToPlayer = Vector2.Distance(firePoint.position, playerTransform.position);
        
        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, dirToPlayer, distToPlayer);
        
        if (hit.collider == null)
            return true;
        
        if (hit.collider.CompareTag(playerTag))
            return true;
        
        return false;
    }

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

        SetDestination(patrolTarget, patrolSpeed);

        if (ai != null && ai.reachedDestination)
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

    void SetDestination(Vector3 worldTarget, float speed)
    {
        if (ai != null)
        {
            ai.destination = worldTarget;
            ai.canMove = true;
            ai.canSearch = true;
            ai.isStopped = false;
            ai.maxSpeed = speed;
        }
    }

    void StrafeTowardPlayer()
    {
        if (playerTransform == null) return;

        Vector2 toPlayer = (playerTransform.position - transform.position).normalized;
        Vector2 perp = new Vector2(-toPlayer.y, toPlayer.x);
        float strafeOffset = Mathf.Sin(Time.time * strafeSpeed) * strafeAmplitude;

        Vector2 strafeTarget = (Vector2)transform.position + perp * strafeOffset;
        Vector2 approach = (Vector2)transform.position + toPlayer * 0.1f;
        Vector2 combined = Vector2.Lerp(strafeTarget, approach, 0.2f);

        SetDestination(new Vector3(combined.x, combined.y, transform.position.z), chaseSpeed);
    }

    void ShootAtPlayer()
    {
        if (playerTransform == null || shootTimer > 0f) return;
        if (bulletPrefab == null || firePoint == null) return;

        var dir = (playerTransform.position - firePoint.position).normalized;
        GameObject b = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        var rbBullet = b.GetComponent<Rigidbody2D>();
        if (rbBullet != null) rbBullet.linearVelocity = dir * bulletSpeed;
        
        shootTimer = shootCooldown;
    }

    void ShootSpreadPattern()
    {
        if (playerTransform == null || shootTimer > 0f) return;
        if (bulletPrefab == null || firePoint == null) return;

        Vector2 dirToPlayer = (playerTransform.position - firePoint.position).normalized;
        Vector2 perpDir = new Vector2(-dirToPlayer.y, dirToPlayer.x).normalized;

        Vector2[] directions = new Vector2[4]
        {
            dirToPlayer,
            perpDir,
            -perpDir,
            (dirToPlayer + perpDir).normalized
        };

        foreach (var dir in directions)
        {
            GameObject b = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            var rbBullet = b.GetComponent<Rigidbody2D>();
            if (rbBullet != null) rbBullet.linearVelocity = dir * bulletSpeed;
        }

        shootTimer = shootCooldown;
    }

    bool DetectAndReactToIncomingBullets()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, bulletDetectRadius, bulletLayer);
        Vector2 selfPos = transform.position;

        foreach (var col in hits)
        {
            if (col == null) continue;

            var projRb = col.attachedRigidbody;
            if (projRb == null) continue;

            Vector2 bPos = projRb.position;
            Vector2 v = projRb.linearVelocity;
            if (v.sqrMagnitude < 0.01f) continue;

            Vector2 eMinusB = (Vector2)selfPos - bPos;
            float t = Vector2.Dot(eMinusB, v) / v.sqrMagnitude;
            if (t <= 0f || t > threatTimeThreshold) continue;

            Vector2 predicted = bPos + v * t;
            float distAtClosest = Vector2.Distance(predicted, selfPos);

            if (distAtClosest <= 0.6f)
            {
                if (TryFindCoverAgainstProjectile(bPos, v, out Vector2 coverTarget))
                {
                    StartDodgeTo(coverTarget);
                    return true;
                }
                else
                {
                    Vector2 perp = new Vector2(-v.y, v.x).normalized;
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

    bool TryFindCoverAgainstProjectile(Vector2 bulletPos, Vector2 bulletVelocity, out Vector2 outCoverPoint)
    {
        outCoverPoint = Vector2.zero;
        Collider2D[] obstacles = Physics2D.OverlapCircleAll(transform.position, 6f, obstacleLayer);
        if (obstacles == null || obstacles.Length == 0) return false;

        Vector2 selfPos = transform.position;
        float bestScore = float.MaxValue;
        bool found = false;

        foreach (var obs in obstacles)
        {
            if (obs == null) continue;

            Vector2 coverPoint = obs.ClosestPoint(selfPos);
            Vector2 dirFromBullet = (coverPoint - bulletPos);
            float dist = dirFromBullet.magnitude;
            if (dist <= 0.05f) continue;

            RaycastHit2D hit = Physics2D.Raycast(bulletPos, dirFromBullet.normalized, dist + 0.01f, obstacleLayer | bulletLayer);
            if (hit.collider == null || hit.collider != obs) continue;

            Vector2 normal = (coverPoint - (Vector2)obs.bounds.center).normalized;
            Vector2 safePoint = coverPoint + normal * 0.3f;

            float score = Vector2.Distance(selfPos, safePoint);
            if (score < bestScore)
            {
                bestScore = score;
                outCoverPoint = safePoint;
                found = true;
            }
        }

        return found;
    }

    void StartDodgeTo(Vector2 target)
    {
        isDodging = true;
        dodgeTarget = target;
        dodgeTimer = 0.35f;
        dodgeCooldownTimer = dodgeCooldown;
        if (ai != null) ai.isStopped = true;
    }

    void PerformDodgeMovement()
    {
        Vector2 newPos = Vector2.MoveTowards(rb.position, dodgeTarget, dodgeSpeed * Time.deltaTime);
        rb.MovePosition(newPos);

        dodgeTimer -= Time.deltaTime;
        if (dodgeTimer <= 0f)
        {
            isDodging = false;
            if (ai != null) ai.isStopped = false;
        }
    }

    Transform FindNearestWithTag(string tag)
    {
        if (string.IsNullOrEmpty(tag)) return null;
        var objs = GameObject.FindGameObjectsWithTag(tag);
        if (objs == null || objs.Length == 0) return null;

        Transform best = null;
        float bestDist = float.PositiveInfinity;
        Vector2 self = transform.position;

        foreach (var go in objs)
        {
            if (go == null) continue;
            float d = Vector2.SqrMagnitude((Vector2)go.transform.position - self);
            if (d < bestDist)
            {
                bestDist = d;
                best = go.transform;
            }
        }
        return best;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, bulletDetectRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, closeRangeDistance);
    }
}