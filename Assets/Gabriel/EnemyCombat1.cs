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

    private Transform playerTransform;
    private Vector3 originPosition;
    private Vector3 wanderTarget;
    private float wanderIdleTimer = 0f;
    private bool isIdling = false;

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
            transform.position = Vector3.MoveTowards(transform.position, transform.position + dir, chaseSpeed * Time.deltaTime);
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
                transform.position = Vector3.MoveTowards(transform.position, wanderTarget, wanderSpeed * Time.deltaTime);
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
        isDashing = true;
        Vector3 start = transform.position;
        float elapsed = 0f;

        // use provided duration (already adjusted by level)
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.position = Vector3.Lerp(start, target, t);
            yield return null;
        }

        transform.position = target;
        isDashing = false;
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
        }
    }
}
