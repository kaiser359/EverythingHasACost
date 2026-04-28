using System.Collections;
using UnityEngine;
using static UnityEngine.UI.Image;

public class EnemyCombat1 : MonoBehaviour
{
    [Header("References")]
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
            float desiredAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;
            Quaternion desiredRot = Quaternion.Euler(0f, 0f, desiredAngle);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRot, 720f * Time.deltaTime);

            if (activationTimer >= warmupAdj)
            {
                Vector3 dashDir = (playerTransform.position - transform.position).normalized;
                markedPosition = playerTransform.position + dashDir * dashOvershoot;
                StartCoroutine(DashToMarked(markedPosition, dashDurationAdj));
                activationTimer = 0f;
                isWarmingUp = false;
            }
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

                            // do not apply physical knockback so the player won't be pushed out of the level
                            // any damage/effects are already applied above via money change
                            // continue moving (do not treat player as an obstacle)
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
}
