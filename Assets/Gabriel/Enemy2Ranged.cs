using UnityEngine;

public class Enemy2Ranged : MonoBehaviour
{
    //inspired by miku bean scirpt. i will do huge changes in the future but for now i just want to get a basic lazer enemy working.
    [Header("References")]
    public EnemyStats stats;
    public Transform laserFirePoint;
    public LineRenderer lineRenderer;
    public Money money;

    [Header("Ranges & Movement")]
    public float detectionRange = 8f; 
    public float chaseRange = 14f; 
    public float chaseSpeed = 2.5f;
    public float wanderRadius = 2f;
    public float wanderSpeed = 0.8f;
    public float idleMin = 0.5f;
    public float idleMax = 2f;

    [Header("Laser")]
    public float laserDistance = 12f;
    public float aimSpeed = 3f; // might remove
    public int damagePerTick = 5;
    public float damageCooldown = 0.5f; 
    public float knockbackForce = 5f;
    public float oscillationAmplitude = 0.6f; 
    public float oscillationSpeed = 3f; 
    public float rotationSpeed = 360f; 

    private Transform playerTransform;
    private Vector3 originPosition;
    private Vector3 wanderTarget;
    private float wanderIdleTimer = 0f;
    private bool isIdling = false;

    private Vector2 _currentAimDir = Vector2.right;
    private float damageTimer = 0f;

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
        damageTimer -= Time.deltaTime;

        if (lineRenderer != null)
            lineRenderer.enabled = false;

        float dist = playerTransform != null ? Vector2.Distance(transform.position, playerTransform.position) : Mathf.Infinity;

       
        if (playerTransform != null)
        {
            Vector2 targetDir = (playerTransform.position - transform.position);
            if (targetDir.sqrMagnitude > 0.0001f)
            {
                float speed = dist <= detectionRange ? aimSpeed : aimSpeed * 0.4f;
                _currentAimDir = Vector2.Lerp(_currentAimDir, targetDir.normalized, speed * Time.deltaTime);
            }
        }

      
        if (dist <= detectionRange)
        {
            // Laser active
            if (lineRenderer != null)
            {
                lineRenderer.enabled = true;
                Vector3 origin = laserFirePoint != null ? laserFirePoint.position : transform.position;

                // oscillation  up and down
                Vector2 aim = _currentAimDir.normalized;
                Vector2 perp = new Vector2(-aim.y, aim.x);
                float osc = Mathf.Sin(Time.time * oscillationSpeed) * oscillationAmplitude;
                Vector2 oscillatedAim = (aim + perp * osc).normalized;

                Vector3 end = origin + (Vector3)(oscillatedAim * laserDistance);

                // see if lazer hits
                RaycastHit2D hit = Physics2D.Raycast(origin, oscillatedAim, laserDistance);
                if (hit.collider != null)
                {
                    end = hit.point;

                    // KnockBAck

                    if (hit.collider.CompareTag("Player") && damageTimer <= 0f)
                    {
                        money.money -= damagePerTick+ (money.money /100);

                        var prb = hit.collider.GetComponent<Rigidbody2D>();
                        if (prb != null)
                        {
                            // knockback away from laser origin
                            Vector2 kbDir = (hit.collider.transform.position - origin).normalized;
                            prb.AddForce(kbDir * knockbackForce, ForceMode2D.Impulse);
                        }

                        damageTimer = damageCooldown;
                    }
                }

                // Rotate enemy smoothly
                float desiredAngle = Mathf.Atan2(oscillatedAim.y, oscillatedAim.x) * Mathf.Rad2Deg;
                Quaternion desiredRot = Quaternion.Euler(0f, 0f, desiredAngle);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRot, rotationSpeed * Time.deltaTime);
                lineRenderer.SetPosition(0, origin);
                lineRenderer.SetPosition(1, end);
            }
        }
        else if (playerTransform != null && dist <= chaseRange)
        {
            // Chase
            Vector3 dir = (playerTransform.position - transform.position).normalized;
            transform.position = Vector3.MoveTowards(transform.position, transform.position + dir, chaseSpeed * Time.deltaTime);
        }
        else
        {
            // Wandering
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
}
