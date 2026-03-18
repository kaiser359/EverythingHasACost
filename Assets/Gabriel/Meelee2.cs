using UnityEngine;

public class Meelee2 : MonoBehaviour
{
    public Money money;
    public GameObject atkLocation;
    public GameObject atkplace;
    [Header("Detection & Movement")]
    public float detectionRange = 10f;
    public float chaseRange = 15f;
    public float chaseSpeed = 2.5f;
    public float wanderRadius = 2f;
    public float wanderSpeed = 0.8f;
    public float idleMin = 0.5f;
    public float idleMax = 2f;
    
    private Transform playerTransform;
    private float timerForNextAtk=0;
    private float timerForAtkDisapear=0;
    private Vector3 originPosition;
    private Vector3 wanderTarget;
    private float wanderIdleTimer = 0f;
    private bool isIdling = false;
    private void Start()
    {

        // initialize wander origin and target
        originPosition = transform.position;
        PickNewWanderTarget();
        atkplace.SetActive(false);
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) playerTransform = p.transform;

    }
    private void Update()
    {
        float dist = playerTransform != null ? Vector2.Distance(transform.position, playerTransform.position) : Mathf.Infinity;
        if (playerTransform != null && dist <= detectionRange && HasLineOfSight())
        {
            // Move towards the player
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            transform.position += direction * chaseSpeed * Time.deltaTime;
            // Check if within attack range
            if (dist <= 1.5f && timerForNextAtk <= 0) // Adjust attack range as needed
            {
                Attack();
                timerForNextAtk = 0.25f;
            }
            
        }
        else
        {
            // Wandering behavior
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
        if (timerForNextAtk >= 0)
        {
            timerForNextAtk -= Time.deltaTime;
        }
        if(timerForAtkDisapear > 0f)
        {
            timerForAtkDisapear -= Time.deltaTime;
            if (timerForAtkDisapear <= 0f)
            {
                atkplace.SetActive(false) ;
            }
        }
    }
    private void Attack()
    {
        atkplace.SetActive(true) ;
        // rotate the attack location to face the player (top-down: rotate around Z)
        if (playerTransform != null && atkLocation != null)
        {
            Vector2 dir = (playerTransform.position - atkLocation.transform.position);
            if (dir.sqrMagnitude > 0.0001f)
            {
                float desiredAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                atkLocation.transform.rotation = Quaternion.Euler(0f, 0f, desiredAngle);
            }
        }
        timerForAtkDisapear = 0.1f;
        // Implement attack logic here (e.g., deal damage to the player)
        // For demonstration, we'll just print a message
        Debug.Log("Meelee2 attacks the player!");
        // You can also add logic to deal damage to the player or trigger animations
    }
    private bool HasLineOfSight()
    {
        if (playerTransform == null) return false;
        Vector2 direction = playerTransform.position - transform.position;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction.normalized, detectionRange);
        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            return true; // Player is in line of sight
        }
        return false; // Player is not in line of sight
    }
    void PickNewWanderTarget()
    {
        Vector2 offset = Random.insideUnitCircle * wanderRadius;
        wanderTarget = originPosition + (Vector3)offset;
    }
}
