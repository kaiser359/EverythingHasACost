using UnityEngine;

public class Enemy2Ranged : MonoBehaviour
{
    //inspired by miku bean scirpt. i will do huge changes in the future but for now i just want to get a basic lazer enemy working.
    [Header("References")]
    public EnemyStats stats;
    public Transform laserFirePoint;
    public LineRenderer lineRenderer;
    public Money money;
    public Levels level;
  public Animator animator;

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
    // Warmup before the laser actually becomes active (seconds)
    public float laserActivationCooldown = 0.55f;
    // Movement while warming up (before laser becomes active)
    public float preFireMoveSpeed = 1.2f;
    public float preFireMoveAmount = 0.25f;

    public AudioClip laserSound;

    private Transform playerTransform;
    private Vector3 originPosition;
    private Vector3 wanderTarget;
    private float wanderIdleTimer = 0f;
    private bool isIdling = false;
    [SerializeField]private float timerTohit;
    private Rigidbody2D rb;

    private Vector2 _currentAimDir = Vector2.right;
    private float damageTimer = 0f;
    private float activationTimer = 0f;
    private bool laserActive = false;
    public float charger = 0;
    public float timer = 5;

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
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.mass = Mathf.Max(rb.mass, 5f);
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

        // compute oscillated aim continuously so the enemy can move/face while warming up
        Vector2 oscillatedAim = _currentAimDir.normalized;
        {
            Vector2 aim = _currentAimDir.normalized;
            Vector2 perp = new Vector2(-aim.y, aim.x);
            float osc = Mathf.Sin(Time.time * (oscillationSpeed + ((float)level.levelNumber/2f))) * oscillationAmplitude;
            oscillatedAim = (aim + perp * osc).normalized;
        }


        if (dist <= detectionRange + level.levelNumber)
        {
            activationTimer += Time.deltaTime;

            // Laser becomes active after warmup
            if (activationTimer >= laserActivationCooldown && timer >0)
            {
                laserActive = true;
                GetComponent<AudioSource>().PlayOneShot(laserSound);
            }
            else if (timer < 0) { 
                laserActive = false;
                charger += Time.deltaTime;
            }
            if (laserActive == true)
            {
                timer -= Time.deltaTime;

            }
            if (charger > 5 && timer < 0)
            {
                timer = 5;
                charger = 0;
            }
      

            // While warming up (not yet active), move slightly along the oscillated aim and face it
            if (!laserActive)
            {
                // move a small amount in the oscillated aim direction to telegraph the attack
                    Vector3 moveTarget = transform.position + (Vector3)(oscillatedAim * preFireMoveAmount);
                    Vector3 newPos = Vector3.MoveTowards(transform.position, moveTarget, preFireMoveSpeed * Time.deltaTime);
                    if (rb != null) rb.MovePosition(newPos);
                    else transform.position = newPos;

                // rotate to face the aim direction
                float desiredAngle = Mathf.Atan2(oscillatedAim.y, oscillatedAim.x) * Mathf.Rad2Deg;
                Quaternion desiredRot = Quaternion.Euler(0f, 0f, desiredAngle);
               // transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRot, rotationSpeed * Time.deltaTime);

                if (lineRenderer != null)
                    lineRenderer.enabled = false;
            }
            else // laser active: perform raycast and render
            {
                if (lineRenderer != null)
                {
                    lineRenderer.enabled = true;
                    Vector3 origin = laserFirePoint != null ? laserFirePoint.position : transform.position;

                    Vector3 end = origin + (Vector3)(oscillatedAim * laserDistance);
                    timerTohit += Time.deltaTime;

                    // see if laser hits
                    RaycastHit2D hit = Physics2D.Raycast(origin, oscillatedAim, laserDistance + level.levelNumber);
                    if (hit.collider != null)
                    {
                        end = hit.point;

                        // Knockback
                        if (hit.collider.CompareTag("Player") && damageTimer <= 0f)
                        {
                            //money.money -= damagePerTick + (money.money / 100) + (level.levelNumber*10);
                            var playerHealth = hit.collider.GetComponentInChildren<HealthBar>();
                            if (playerHealth != null) playerHealth.TakeDamage(damagePerTick + (money.money / 100) + (level.levelNumber*10));

                            // prefer the rigidbody reported by the raycast, fall back to parent lookup
                            Rigidbody2D prb = hit.rigidbody != null ? hit.rigidbody : hit.collider.GetComponentInParent<Rigidbody2D>();

                            // compute knockback direction from impact point for better accuracy
                            Vector2 kbDir = ((Vector2)hit.point - (Vector2)origin).normalized;
                            if (kbDir.sqrMagnitude < 0.0001f)
                                kbDir = ((Vector2)hit.collider.transform.position - (Vector2)origin).normalized;

                            if (prb != null)
                            {
                                // Try to use a KnockbackOverride component so we can bypass the player's movement script.
                                var kbOverride = prb.GetComponent<KnockbackOverride>();
                                if (kbOverride == null)
                                    kbOverride = prb.gameObject.AddComponent<KnockbackOverride>();

                                // convert impulse to velocity approximation: v = impulse / mass
                                float mass = Mathf.Max(0.0001f, prb.mass);
                                Vector2 vel = kbDir * (knockbackForce / mass);
                                kbOverride.Apply(vel, 0.15f);
                            }
                            else
                            {
                                // last resort: nudge the transform if there's no Rigidbody2D
                                if (kbDir.sqrMagnitude > 0.0001f)
                                    hit.collider.transform.position += (Vector3)(kbDir * (knockbackForce * 0.02f));
                            }

                            damageTimer = damageCooldown;
                        }
                    }

                    // Rotate enemy smoothly while firing as well
                   // float desiredAngle = Mathf.Atan2(oscillatedAim.y, oscillatedAim.x) * Mathf.Rad2Deg;
                    //Quaternion desiredRot = Quaternion.Euler(0f, 0f, desiredAngle);
                  //  transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRot, rotationSpeed * Time.deltaTime);
                    lineRenderer.SetPosition(0, origin);
                    lineRenderer.SetPosition(1, end);
                }
            }
        }
        else if (playerTransform != null && dist <= chaseRange)
        {
            timerTohit = 0f;
            // reset laser warmup/state when leaving detection range
            activationTimer = 0f;
            laserActive = false;
            // Chase
            Vector3 dir = (playerTransform.position - transform.position).normalized;
            Vector3 targetPos = transform.position + dir;
            Vector3 newPos = Vector3.MoveTowards(transform.position, targetPos, chaseSpeed * Time.deltaTime);
            if (rb != null) rb.MovePosition(newPos);
            else transform.position = newPos;
        }
        else
        {
            timerTohit = 0;
            activationTimer = 0f;
            laserActive = false;
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

    void PickNewWanderTarget()
    {
        Vector2 offset = Random.insideUnitCircle * wanderRadius;
        wanderTarget = originPosition + (Vector3)offset;
    }

    void OnCollisionStay2D(Collision2D other)
    {
        if (other.collider.CompareTag("Player") && rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}
