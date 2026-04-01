using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BEYBLADE : MonoBehaviour
{
    [Header("References")]
    public Transform laserFirePoint;
    public LineRenderer lineRenderer;

    [Header("Detection & Movement")]
    public float detectionRange = 8f;
    public float chaseSpeed = 3f;

    [Header("Laser")]
    public float laserDistance = 12f;
    public float rotationSpeed = 360f; // degrees per second
    public int damagePerTick = 5;
    public float damageCooldown = 0.5f;
    public float knockbackForce = 5f;

    Transform playerTransform;
    Rigidbody2D rb;
    float damageTimer = 0f;
    public Money money;
    SpriteRenderer sr;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
          //  rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        var p = GameObject.FindWithTag("Player");
        if (p != null) playerTransform = p.transform;

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.enabled = false;
        }

        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        damageTimer -= Time.deltaTime;

        // check player distance
        float dist = playerTransform != null ? Vector2.Distance(transform.position, playerTransform.position) : Mathf.Infinity;
        bool active = dist <= detectionRange;

        // show/hide sprite depending on detection
        if (sr != null) sr.enabled = active;

        // if not active, ensure laser is off and do nothing else
        if (!active)
        {
            if (lineRenderer != null) lineRenderer.enabled = false;
            return;
        }

        // active: spin and fire laser
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

        if (lineRenderer != null) lineRenderer.enabled = true;

        Vector3 origin = laserFirePoint != null ? laserFirePoint.position : transform.position;
        Vector2 dir = transform.up;
        Vector3 end = origin + (Vector3)(dir * laserDistance);

        RaycastHit2D hit = Physics2D.Raycast(origin, dir, laserDistance);
        if (hit.collider != null)
        {
            end = hit.point;

            // if we hit the player, apply damage/knockback on cooldown
            if (hit.collider.CompareTag("Player") && damageTimer <= 0f)
            {
                var prb = hit.rigidbody != null ? hit.rigidbody : hit.collider.GetComponentInParent<Rigidbody2D>();
                if (prb != null)
                {
                    Vector2 kbDir = ((Vector2)hit.point - (Vector2)origin).normalized;
                    if (kbDir.sqrMagnitude < 0.0001f)
                        kbDir = (hit.collider.transform.position - origin).normalized;

                    float mass = Mathf.Max(0.0001f, prb.mass);
                    Vector2 vel = kbDir * (knockbackForce / mass);
                    prb.linearVelocity = vel; // simple knockback
                }

                // apply money/damage or other effects
                if (money != null) money.money -= damagePerTick;

                damageTimer = damageCooldown;
            }
        }

        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, origin);
            lineRenderer.SetPosition(1, end);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        if (laserFirePoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(laserFirePoint.position, laserFirePoint.position + transform.up * laserDistance);
        }
    }
}
