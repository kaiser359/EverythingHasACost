using System.Collections;
using UnityEngine;
using static UnityEngine.UI.Image;

public class EnemyCombat1 : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public EnemyStats stats;
    public Money money;
    public Transform playerWashere; 
    public Levels level;

    [Header("Detection & Movement")]
    public float detectionRange = 10f;
    public float chaseRange = 15f;
    public float chaseSpeed = 2.5f;
    public float wanderRadius = 2f;
    public float wanderSpeed = 0.8f;
    public float idleMin = 0.5f;
    public float idleMax = 2f;

    [Header("Dash Attack")]
    public float dashWarmup = 0.55f; 
    public float dashDuration = 0.25f; 
    public int touchDamageToMoney = 1;
    public float knockbackForce = 5f; 
    public float dashOvershoot = 1f; 

    public AudioClip dashSound;

    private Transform playerTransform;
    private Vector3 originPosition;
    private Vector3 wanderTarget;
    private float wanderIdleTimer = 0f;
    private bool isIdling = false;
    private Rigidbody2D rb;

    private float activationTimer = 0f;
    private bool isWarmingUp = false;
    private bool isDashing = false;
    private Vector3 markedPosition;
    

    void Start()
    {
        originPosition = transform.position;
        PickNewWanderTarget();
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) playerTransform = p.transform;
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Ensure dynamic body so collisions with walls are resolved by physics
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        // ensure the transform has no rotation so sprite doesn't appear rotated
        transform.rotation = Quaternion.identity;
    }

    void Update()
    {
        if (isDashing)
            return;

        float dist = playerTransform != null ? Vector2.Distance(transform.position, playerTransform.position) : Mathf.Infinity;

        // level-based adjustments (convert int to float)
        float lvl = level != null ? (float)level.levelNumber : 0f;
        // decrease warmup by 0.01 per level (level 4 => -0.04)
        float warmupAdj = Mathf.Max(0.05f, dashWarmup - lvl * 0.01f);
        // slightly decrease dash duration per level, smaller impact than warmup
        float dashDurationAdj = Mathf.Max(0.05f, dashDuration - lvl * 0.005f);

        if (playerTransform != null && dist <= detectionRange && HasLineOfSight())
        {
            isWarmingUp = true;
            activationTimer += Time.deltaTime;

            Vector3 toPlayer = (playerTransform.position - transform.position).normalized;
            // intentionally do not rotate the transform to keep sprite orientation stable.

            if (activationTimer >= warmupAdj)
            {
                Vector3 dashDir = (playerTransform.position - transform.position).normalized;
                markedPosition = playerTransform.position + dashDir * dashOvershoot;
                StartCoroutine(DashToMarked(markedPosition, dashDurationAdj));
                activationTimer = 0f;
                isWarmingUp = false;
            }
        // enforce zero rotation every frame to avoid any accidental sprite rotation
        transform.rotation = Quaternion.identity;
        }
        else if (playerTransform != null && dist <= chaseRange)
        {
          
            activationTimer = 0f;
            isWarmingUp = false;
            Vector3 dir = (playerTransform.position - transform.position).normalized;
            Vector3 targetPos = transform.position + dir;
            Vector3 newPos = Vector3.MoveTowards(transform.position, targetPos, chaseSpeed * Time.deltaTime);
            if (rb != null) rb.MovePosition(newPos);
            else transform.position = newPos;
        }
        else
        {
         
            activationTimer = 0f;
            isWarmingUp = false;
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

    bool HasLineOfSight()
    {
        if (playerTransform == null) return false;
        Vector2 dir = (playerTransform.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, detectionRange);
        return hit.collider != null && hit.collider.CompareTag("Player");
    }

    IEnumerator DashToMarked(Vector3 target, float duration)
    {
        GetComponent<AudioSource>().PlayOneShot(dashSound);
        isDashing = true;
       
        Vector3 start = transform.position;
        float elapsed = 0f;
        // prepare to temporarily ignore collisions with the player so the enemy can pass through
        Collider2D[] enemyColls = GetComponents<Collider2D>();
        Collider2D[] playerColls = new Collider2D[0];
        Rigidbody2D playerRb = null;
        if (playerTransform != null)
        {
            playerColls = playerTransform.GetComponentsInChildren<Collider2D>();
            playerRb = playerTransform.GetComponent<Rigidbody2D>();
        }
        // disable physics collisions between this enemy and the player while dashing
        foreach (var ec in enemyColls)
            foreach (var pc in playerColls)
                if (ec != null && pc != null) Physics2D.IgnoreCollision(ec, pc, true);

        // use provided duration (already adjusted by level)
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            Vector3 nextPos = Vector3.Lerp(start, target, t);

            // check for obstacles between current rigidbody position and nextPos
            if (rb != null)
            {
                Vector2 curPos = rb.position;
                Vector2 moveDir = (nextPos - (Vector3)curPos);
                float moveDist = moveDir.magnitude;
                if (moveDist > 0.0001f)
                {
                    
                    RaycastHit2D hit = Physics2D.Raycast(curPos, moveDir.normalized, moveDist);
                    if (hit.collider != null)
                    {
                        // if we hit the player, apply damage/knockback manually but keep moving
                        if (hit.collider.CompareTag("Player"))
                        {
                            if (money != null)
                            {
                                float lvl = level != null ? (float)level.levelNumber : 0f;
                                int baseDmg = stats != null ? stats.atkDamage : touchDamageToMoney;
                                int dmg = baseDmg + Mathf.RoundToInt(lvl * 10f);
                                money.money = Mathf.Max(0, money.money - dmg);
                                var playerHealth = hit.collider.GetComponent<HealthBar>();
                                if (playerHealth != null)
                                {
                                    playerHealth.TakeDamage(5); // just so the effect happens plus extra base damage.
                                }
                            }

        
                        }
                        else if (!hit.collider.isTrigger)
                        {
                            // stop at the collision point (small offset so we don't overlap)
                            Vector3 stopPos = (Vector3)hit.point - (Vector3)moveDir.normalized * 0.01f;
                            rb.MovePosition(stopPos);
                            break;
                        }
                    }
                }

                rb.MovePosition(nextPos);
                // neutralize external forces so player collisions don't push the enemy
                rb.linearVelocity = Vector2.zero;
                animator.SetBool("Dashing", true);
            }
            else
            {
                transform.position = nextPos;
            }

            yield return null;
        }

        // final safety: ensure position is set to target if there was no obstacle
        if (rb != null && !isDashing)
        {
            rb.MovePosition(target);
            rb.linearVelocity = Vector2.zero;
        }

        // re-enable collisions between enemy and player
        foreach (var ec in enemyColls)
            foreach (var pc in playerColls)
                if (ec != null && pc != null) Physics2D.IgnoreCollision(ec, pc, false);

        isDashing = false;
    }

    void OnCollisionStay2D(Collision2D other)
    {
        // prevent player from pushing this enemy while keeping collision for damage
        if (other.collider.CompareTag("Player") && rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    void PickNewWanderTarget()
    {
        Vector2 offset = Random.insideUnitCircle * wanderRadius;
        wanderTarget = originPosition + (Vector3)offset;
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Player") && money != null)
        {
            // scale damage by level (convert int level to float multiplier)
            float lvl = level != null ? (float)level.levelNumber : 0f;
            int baseDmg = stats != null ? stats.atkDamage : touchDamageToMoney;
            int dmg = baseDmg + Mathf.RoundToInt(lvl * 10f); // example: level 4 adds ~40 damage
            money.money = Mathf.Max(0, money.money - dmg);
            var playerHealth = other.collider.GetComponent<HealthBar>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(5); // just so the effect happens plus extra base damage.
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && money != null)
        {
            // apply same level-based damage scaling for triggers
            float lvl = level != null ? (float)level.levelNumber : 0f;
            int baseDmg = stats != null ? stats.atkDamage : touchDamageToMoney;
            int dmg = baseDmg + Mathf.RoundToInt(lvl * 10f);
            money.money = Mathf.Max(0, money.money - dmg);
            var PlayerHeath = other.GetComponent<HealthBar>();
            if (PlayerHeath != null) {
                PlayerHeath.TakeDamage(5);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw the detection ray used by HasLineOfSight
        Gizmos.color = Color.yellow;
        Vector3 origin = transform.position;
        if (playerTransform != null)
        {
            Vector3 dir = (playerTransform.position - transform.position).normalized;
            // draw full ray
            Vector3 end = origin + dir * detectionRange;
            // try to perform the same Physics2D.Raycast so the gizmo color can indicate hits
            RaycastHit2D hit = Physics2D.Raycast(origin, dir, detectionRange);
            if (hit.collider != null)
            {
                if (hit.collider.CompareTag("Player")) Gizmos.color = Color.green;
                else Gizmos.color = Color.red;
                // draw line to the hit point
                Gizmos.DrawLine(origin, hit.point);
                Gizmos.DrawSphere(hit.point, 0.08f);
                // draw remaining portion as faded
                Gizmos.color = new Color(1f, 1f, 1f, 0.15f);
                Gizmos.DrawLine(hit.point, end);
            }
            else
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(origin, end);
            }
        }

        // If a dash target was marked, draw the planned dash path
        if (markedPosition != Vector3.zero)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(origin, markedPosition);
            Gizmos.DrawWireSphere(markedPosition, 0.12f);
        }
    }
}
