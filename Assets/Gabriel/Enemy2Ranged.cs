using UnityEngine;

public class Enemy2Ranged : MonoBehaviour
{
    [Header("References")]
    public EnemyStats stats;
    public Transform laserFirePoint;
    public LineRenderer lineRenderer;
    public GameObject beanPrefab;

    [Header("Ranges & Movement")]
    public float detectionRange = 8f; // when to stop and fire laser/throw beans
    public float chaseRange = 14f; // when to start chasing
    public float chaseSpeed = 2.5f;
    public float wanderRadius = 2f;
    public float wanderSpeed = 0.8f;
    public float idleMin = 0.5f;
    public float idleMax = 2f;

    [Header("Shooting")]
    public float throwForce = 6f;
    public float throwInterval = 2f;
    public float throwSpreadDegrees = 12f;
    public float laserDistance = 12f;
    public float aimSpeed = 3f; // how fast laser aims toward player

    private Transform playerTransform;
    private Vector3 originPosition;
    private Vector3 wanderTarget;
    private float wanderIdleTimer = 0f;
    private bool isIdling = false;

    private Vector2 _currentAimDir = Vector2.right;
    private float shootTimer = 0f;

    void Start()
    {
        originPosition = transform.position;
        PickNewWanderTarget();

        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) playerTransform = p.transform;

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.enabled = false;
        }
    }

    void Update()
    {
        shootTimer -= Time.deltaTime;

        float dist = playerTransform != null ? Vector2.Distance(transform.position, playerTransform.position) : Mathf.Infinity;

        // Update aim direction smoothly toward player when available
        if (playerTransform != null)
        {
            Vector2 targetDir = (playerTransform.position - transform.position);
            if (targetDir.sqrMagnitude > 0.0001f)
            {
                float speed = dist <= detectionRange ? aimSpeed : aimSpeed * 0.4f;
                _currentAimDir = Vector2.Lerp(_currentAimDir, targetDir.normalized, speed * Time.deltaTime);
            }
        }

        // Laser visualization
        if (lineRenderer != null)
        {
            if (dist <= detectionRange)
            {
                lineRenderer.enabled = true;
                Vector3 origin = laserFirePoint != null ? laserFirePoint.position : transform.position;
                Vector3 end = origin + (Vector3)(_currentAimDir.normalized * laserDistance);

                // Raycast to hit obstacles and/or player
                RaycastHit2D hit = Physics2D.Raycast(origin, _currentAimDir, laserDistance);
                if (hit.collider != null)
                    end = hit.point;

                lineRenderer.SetPosition(0, origin);
                lineRenderer.SetPosition(1, end);
            }
            else
            {
                lineRenderer.enabled = false;
            }
        }

        // Behavior: stop and shoot if in detectionRange, else chase if within chaseRange, else wander
        if (dist <= detectionRange)
        {
            // stationary: fire beans at intervals
            if (shootTimer <= 0f)
            {
                ThrowBean();
                shootTimer = throwInterval;
            }
        }
        else if (playerTransform != null && dist <= chaseRange)
        {
            // Chase player
            Vector3 dir = (playerTransform.position - transform.position).normalized;
            transform.position = Vector3.MoveTowards(transform.position, transform.position + dir, chaseSpeed * Time.deltaTime);
        }
        else
        {
            // Wander around origin
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
                transform.position = Vector3.MoveTowards(transform.position, wanderTarget, wanderSpeed * Time.deltaTime);
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

    void ThrowBean()
    {
        if (beanPrefab == null) return;

        Vector3 spawnPos = laserFirePoint != null ? laserFirePoint.position : transform.position;

        Vector2 aim = _currentAimDir.normalized;
        float halfSpread = throwSpreadDegrees * 0.5f;
        float randomAngle = Random.Range(-halfSpread, halfSpread);
        float rad = randomAngle * Mathf.Deg2Rad;
        Vector2 spreadDir = new Vector2(
            aim.x * Mathf.Cos(rad) - aim.y * Mathf.Sin(rad),
            aim.x * Mathf.Sin(rad) + aim.y * Mathf.Cos(rad)
        ).normalized;

        GameObject b = Instantiate(beanPrefab, spawnPos, Quaternion.identity);
        Rigidbody2D rb = b.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = spreadDir * throwForce;
    }
}
