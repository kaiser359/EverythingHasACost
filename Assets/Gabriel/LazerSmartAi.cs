using UnityEngine;
using Pathfinding;
using Gabriel;
using System.Collections.Generic;

public class LazerSmartAi : MonoBehaviour
{
    [Header("References")]
    public LineRenderer laserLineRenderer;
    public LineRenderer laserLineRendererWarning;
    public Transform firePoint;
    public LayerMask obstacleLayer;
    public string playerTag = "Player";

    [Header("Ranges & Movement")]
    public float detectionRange = 12f;
    public float shootRange = 9f;
    public float chaseRange = 14f;
    public float keepAwayDistance = 8f;
    public float patrolRadius = 6f;
    public float patrolSpeed = 1.0f;
    public float chaseSpeed = 2.2f;

    [Header("Laser Shooting")]
    public float shootCooldown = 1.5f;
    public float laserSpeed = 8f;
    public float laserAccuracy = 0.3f;
    public float maxLaserDistance = 50f;
    public int maxLaserBounces = 3;
    public float laserActiveDuration = 5f;
    public float laserWarningDuration = 3f;

    private Transform playerTransform;
    private Rigidbody2D rb;
    private Vector3 origin;
    private Vector3 patrolTarget;
    private float patrolIdleTimer;
    private bool isIdling;
    private float shootTimer;
    private IAstarAI ai;
    private bool isLaserActive;
    private float laserActiveTimer;
    private float currentLaserExtension;
    private Vector2 laserDirection;
    private List<Vector2> laserPathPoints;
    private HashSet<Collider2D> laserHitObjects;
    private bool isWarningActive;
    private float warningTimer;
    private const int laserDamage = 1;

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

        if (laserLineRenderer == null)
        {
            laserLineRenderer = gameObject.AddComponent<LineRenderer>();
            laserLineRenderer.startWidth = 0.1f;
            laserLineRenderer.endWidth = 0.1f;
        }

        if (laserLineRendererWarning == null)
        {
            laserLineRendererWarning = gameObject.AddComponent<LineRenderer>();
            laserLineRendererWarning.startWidth = 0.1f;
            laserLineRendererWarning.endWidth = 0.1f;
        }

        isLaserActive = false;
        isWarningActive = false;
        laserPathPoints = new List<Vector2>();
        laserHitObjects = new HashSet<Collider2D>();
        PickNewPatrolTarget();
    }

    void Update()
    {
        shootTimer -= Time.deltaTime;

        if (playerTransform == null)
        {
            var p = GameObject.FindGameObjectWithTag(playerTag);
            if (p != null) playerTransform = p.transform;
        }

        float playerDist = playerTransform != null ? Vector2.Distance(transform.position, playerTransform.position) : Mathf.Infinity;

        if (isLaserActive)
        {
            StopMovement();
            FreezeRigidbody();
        }
        else if (isWarningActive)
        {
            StopMovement();
            FreezeRigidbody();
        }
        else if (playerTransform != null && playerDist <= detectionRange)
        {
            UnfreezeRigidbody();
            if (playerDist <= shootRange)
            {
                ShootLaser();
            }
            else if (playerDist <= chaseRange)
            {
                MoveAroundPlayer();
            }
        }
        else
        {
            UnfreezeRigidbody();
            Patrol();
        }

        UpdateWarningLaser();
        UpdateLaser();
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

    void MoveAroundPlayer()
    {
        Vector2 directionToPlayer = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
        Vector2 targetPos = (Vector2)playerTransform.position - directionToPlayer * keepAwayDistance;
        SetDestination(targetPos, chaseSpeed);
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

    void StopMovement()
    {
        if (ai != null)
        {
            ai.canMove = false;
            ai.canSearch = false;
            ai.isStopped = true;
        }
    }

    void FreezeRigidbody()
    {
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;
        }
    }

    void UnfreezeRigidbody()
    {
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    void ShootLaser()
    {
        if (playerTransform == null || shootTimer > 0f) return;
        if (firePoint == null) return;

        Vector2 targetPos = playerTransform.position;
        targetPos += Random.insideUnitCircle * laserAccuracy;

        laserDirection = (targetPos - (Vector2)firePoint.position).normalized;
        CalculateLaserPath();

        isWarningActive = true;
        warningTimer = laserWarningDuration;
        shootTimer = laserWarningDuration + shootCooldown;
    }

    private void CalculateLaserPath()
    {
        laserPathPoints.Clear();
        Vector2 currentPos = (Vector2)firePoint.position;
        Vector2 currentDir = laserDirection;
        float distanceRemaining = maxLaserDistance;
        int bouncesLeft = maxLaserBounces;

        laserPathPoints.Add(currentPos);

        while (distanceRemaining > 0f && bouncesLeft > 0)
        {
            RaycastHit2D hit = Physics2D.Raycast(currentPos, currentDir, distanceRemaining, obstacleLayer);

            if (hit.collider != null)
            {
                laserPathPoints.Add(hit.point);
                distanceRemaining -= hit.distance;
                bouncesLeft--;

                currentDir = Vector2.Reflect(currentDir, hit.normal);
                currentPos = hit.point + currentDir * 0.01f;
            }
            else
            {
                laserPathPoints.Add(currentPos + currentDir * distanceRemaining);
                break;
            }
        }
    }

    private void UpdateWarningLaser()
    {
        if (!isWarningActive)
        {
            laserLineRendererWarning.positionCount = 0;
            return;
        }

        warningTimer -= Time.deltaTime;

        if (warningTimer <= 0f)
        {
            isWarningActive = false;
            laserLineRendererWarning.positionCount = 0;

            isLaserActive = true;
            currentLaserExtension = 0f;
            laserActiveTimer = laserActiveDuration;
            laserHitObjects.Clear();
            return;
        }

        DisplayWarningLaser();
    }

    private void DisplayWarningLaser()
    {
        if (laserPathPoints.Count < 2)
            return;
     
        System.Collections.Generic.List<Vector3> displayPoints = new System.Collections.Generic.List<Vector3>();

        for (int i = 0; i < laserPathPoints.Count; i++)
        {
            displayPoints.Add(laserPathPoints[i]);
        }

        laserLineRendererWarning.positionCount = displayPoints.Count;
        for (int i = 0; i < displayPoints.Count; i++)
        {
            laserLineRendererWarning.SetPosition(i, displayPoints[i]);
        }
    }

    private void UpdateLaser()
    {
        if (!isLaserActive)
        {
            laserLineRenderer.positionCount = 0;
            return;
        }

        laserActiveTimer -= Time.deltaTime;

        if (laserActiveTimer <= 0f)
        {
            isLaserActive = false;
            laserLineRenderer.positionCount = 0;
            laserHitObjects.Clear();
            return;
        }

        currentLaserExtension += laserSpeed * Time.deltaTime;
        UpdateLineRenderer();
    }

    private void UpdateLineRenderer()
    {
        if (laserPathPoints.Count < 2)
            return;

        System.Collections.Generic.List<Vector3> displayPoints = new System.Collections.Generic.List<Vector3>();
        float distanceCovered = 0f;

        for (int i = 0; i < laserPathPoints.Count - 1; i++)
        {
            Vector2 segmentStart = laserPathPoints[i];
            Vector2 segmentEnd = laserPathPoints[i + 1];
            float segmentLength = Vector2.Distance(segmentStart, segmentEnd);

            displayPoints.Add(segmentStart);

            if (distanceCovered + segmentLength <= currentLaserExtension)
            {
                distanceCovered += segmentLength;
                displayPoints.Add(segmentEnd);
                DealDamageOnSegment(segmentStart, segmentEnd);
            }
            else
            {
                float remainingDistance = currentLaserExtension - distanceCovered;
                if (segmentLength > 0)
                {
                    Vector2 partialEnd = Vector2.Lerp(segmentStart, segmentEnd, remainingDistance / segmentLength);
                    displayPoints.Add(partialEnd);
                    DealDamageOnSegment(segmentStart, partialEnd);
                }
                break;
            }
        }

        laserLineRenderer.positionCount = displayPoints.Count;
        for (int i = 0; i < displayPoints.Count; i++)
        {
            laserLineRenderer.SetPosition(i, displayPoints[i]);
        }
    }

    private void DealDamageOnSegment(Vector2 segmentStart, Vector2 segmentEnd)
    {
        // Check for obstacles
        RaycastHit2D[] hits = Physics2D.LinecastAll(segmentStart, segmentEnd, obstacleLayer);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && !laserHitObjects.Contains(hit.collider))
            {
                var healthComponent = hit.collider.GetComponent<HealthBar>();
                if (healthComponent != null)
                {
                    healthComponent.TakeDamage(laserDamage);
                    laserHitObjects.Add(hit.collider);
                }
            }
        }

        // Check for player - no hit tracking, continuous damage every frame
        if (playerTransform != null)
        {
            Collider2D playerCollider = playerTransform.GetComponent<Collider2D>();
            if (playerCollider != null)
            {
                if (IsPointOnLineSegment(playerTransform.position, segmentStart, segmentEnd, playerCollider.bounds.extents.magnitude))
                {
                    var healthComponent = playerTransform.GetComponent<HealthBar>();
                    if (healthComponent != null)
                    {
                        healthComponent.TakeDamage(laserDamage);
                    }
                }
            }
        }
    }

    private bool IsPointOnLineSegment(Vector2 point, Vector2 lineStart, Vector2 lineEnd, float tolerance)
    {
        float distance = DistancePointToLineSegment(point, lineStart, lineEnd);
        return distance <= tolerance;
    }

    private float DistancePointToLineSegment(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    {
        Vector2 lineDir = lineEnd - lineStart;
        float lineLengthSq = lineDir.sqrMagnitude;

        if (lineLengthSq == 0f)
            return Vector2.Distance(point, lineStart);

        float t = Mathf.Max(0f, Mathf.Min(1f, Vector2.Dot(point - lineStart, lineDir) / lineLengthSq));
        Vector2 closestPoint = lineStart + t * lineDir;
        return Vector2.Distance(point, closestPoint);
    }
}
