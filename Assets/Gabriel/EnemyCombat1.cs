using System.Collections;
using UnityEngine;
using static UnityEngine.UI.Image;

public class EnemyCombat1 : MonoBehaviour
{
    [Header("References")]
    public EnemyStats stats;
    public Money money;
    public Transform playerWashere; 

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

  
        if (playerTransform != null && dist <= detectionRange && HasLineOfSight())
        {
            isWarmingUp = true;
            activationTimer += Time.deltaTime;

           
            Vector3 toPlayer = (playerTransform.position - transform.position).normalized;
            float desiredAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;
            Quaternion desiredRot = Quaternion.Euler(0f, 0f, desiredAngle);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRot, 720f * Time.deltaTime);

            if (activationTimer >= dashWarmup)
            {
                
                Vector3 dashDir = (playerTransform.position - transform.position).normalized;
                markedPosition = playerTransform.position + dashDir * dashOvershoot;
                StartCoroutine(DashToMarked(markedPosition));
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

    IEnumerator DashToMarked(Vector3 target)
    {
        isDashing = true;
        Vector3 start = transform.position;
        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dashDuration);
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
            int dmg = stats != null ? stats.atkDamage : touchDamageToMoney;
            money.money = Mathf.Max(0, money.money - dmg);
            var prb = other.collider.GetComponent<Rigidbody2D>();
            if (prb != null)
            {
       
                Vector2 kbDir = (other.collider.transform.position - transform.position).normalized;
                prb.AddForce(kbDir * knockbackForce, ForceMode2D.Impulse);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && money != null)
        {
            int dmg = stats != null ? stats.atkDamage : touchDamageToMoney;
            money.money = Mathf.Max(0, money.money - dmg);
        }
    }
}
