using UnityEditor;
using UnityEngine;

public class Meelee2 : MonoBehaviour
{

    public Money money;
    public GameObject atkLocation;
    public GameObject atkplace;
    [Header("Attack")]
    public float knockbackForce = 5f;
    [Header("Detection & Movement")]
    public float detectionRange = 10f;
    public float chaseRange = 15f;
    public float chaseSpeed = 2.5f;
    public float wanderRadius = 2f;
    public float wanderSpeed = 0.8f;
    public float idleMin = 0.5f;
    public float idleMax = 2f;
    
    private Transform playerTransform;
    private Rigidbody2D rb;
    private float timerForNextAtk=0;
    private float timerForAtkDisapear=0;
    private Vector3 originPosition;
    private Vector3 wanderTarget;
    private float wanderIdleTimer = 0f;
    private bool isIdling = false;
    private void Start()
    {

       
        originPosition = transform.position;
        PickNewWanderTarget();
        atkplace.SetActive(false);
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) playerTransform = p.transform;
        rb = GetComponent<Rigidbody2D>();

    }
    private void Update()
    {
        float dist = playerTransform != null ? Vector2.Distance(transform.position, playerTransform.position) : Mathf.Infinity;
        if (playerTransform != null && dist <= detectionRange && HasLineOfSight())
        {
         
            const float attackRange = 3f;
            if (dist <= attackRange)
            {
                timerForNextAtk -= Time.deltaTime;
                if (timerForNextAtk <= 0f)
                {
                    Attack();
                    timerForNextAtk = 0.8f;
                }
            }
            else
            {
                Vector3 direction = (playerTransform.position - transform.position).normalized;
                Vector3 targetPos = transform.position + direction;
                Vector3 newPos = Vector3.MoveTowards(transform.position, targetPos, chaseSpeed * Time.deltaTime);
                if (rb != null) rb.MovePosition(newPos);
                else transform.position = newPos;
            }
        }
        else
        {
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
       
        if (playerTransform != null && atkLocation != null)
        {
            Vector2 dir = (playerTransform.position - atkLocation.transform.position);
            if (dir.sqrMagnitude > 0.0001f)
            {
                float desiredAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                atkLocation.transform.rotation = Quaternion.Euler(0f, 0f, desiredAngle);
            }
        }

      
        if (playerTransform != null)
        {
            var rb = playerTransform.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 kbDir = (playerTransform.position - transform.position).normalized;
                rb.AddForce(kbDir * knockbackForce, ForceMode2D.Impulse);
            }
        }
        timerForAtkDisapear = 0.1f;

    }
    private bool HasLineOfSight()
    {
        if (playerTransform == null) return false;
        Vector2 direction = playerTransform.position - transform.position;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction.normalized, detectionRange);
        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            return true; 
        }
        return false;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        DrawGizmo();
    }


    private void DrawGizmo()
    {
       
        Gizmos.color = new Color(1f, 0f, 0f, 0.15f);
        Gizmos.DrawSphere(transform.position, 0.02f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        if (playerTransform == null)
        {
            
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }

        if (playerTransform != null)
        {
            Vector2 dir = (playerTransform.position - transform.position);
            if (dir.sqrMagnitude > 0.0001f)
            {
                Vector3 origin = transform.position;
                Vector3 end = origin + (Vector3)dir.normalized * detectionRange;

              
                RaycastHit2D hit = Physics2D.Raycast(origin, dir.normalized, detectionRange);
                if (hit.collider != null)
                {
                 
                    Gizmos.color = hit.collider.CompareTag("Player") ? Color.green : Color.yellow;
                    end = hit.point;
                   
                    Gizmos.DrawSphere(end, 0.05f);
                }
                else
                {
                    Gizmos.color = Color.gray;
                }

               
                Gizmos.DrawLine(origin, end);
            }
        }
    }
    void PickNewWanderTarget()
    {
        Vector2 offset = Random.insideUnitCircle * wanderRadius;
        wanderTarget = originPosition + (Vector3)offset;
    }
}
