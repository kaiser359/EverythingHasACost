using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueAbility : MonoBehaviour
{
    public float dashDistance = 6f;
    public float dashDuration = 0.18f;
    public float dashDamage = 25f;
    public float damageRadius = 0.6f;

    // projectile settings (will fall back to GlobalPlayerInfo if null)
    public GameObject projectilePrefab;
    public float projectileSpeed = 14f;

    Rigidbody2D rb;
    [SerializeField]
    Collider2D[] myColliders;

    // store original collider enabled states if needed
    bool[] _originalColliderStates;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // include child colliders as well in case colliders are on child objects
        myColliders = GetComponentsInChildren<Collider2D>(true);
        if (myColliders != null)
        {
            _originalColliderStates = new bool[myColliders.Length];
            for (int i = 0; i < myColliders.Length; i++) _originalColliderStates[i] = myColliders[i] != null && myColliders[i].enabled;
        }
    }

    void Start()
    {
        // Start no longer needs to assign rb/myColliders because Awake does it
        // keep Start for fallback assignments
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (myColliders == null || myColliders.Length == 0) myColliders = GetComponentsInChildren<Collider2D>(true);
        if (projectilePrefab == null)
        {
            var g = FindFirstObjectByType<GlobalPlayerInfo>();
            if (g != null) projectilePrefab = g.bulletPrefab;
        }
    }

    // Allow calling the ability from the component context menu in the Inspector
    [ContextMenu("Activate Ability (Inspector)")]
    public void ActivateAbilityContext()
    {
        ActivateAbility();
    }

    // Quick debug entry to perform dash from Inspector
    [ContextMenu("Perform Dash Now (Inspector)")]
    public void PerformDashNow()
    {
        Vector2 dir = transform.right;
        StartCoroutine(PerformDash(dir, dashDistance, dashDuration));
    }

    public void ActivateAbility()
    {
        // determine mouse world position
        Camera cam = Camera.main;
        if (cam == null)
            cam = FindFirstObjectByType<GlobalPlayerInfo>()?.mainCamera;

        Vector3 mouseWorld = Vector3.zero;
        if (cam != null)
        {
            Vector3 mp = Input.mousePosition;
            mp.z = Mathf.Abs(cam.transform.position.z - transform.position.z);
            mouseWorld = cam.ScreenToWorldPoint(mp);
        }

        // throw a burst of 3 projectiles toward mouse (center + two spread)
        if (projectilePrefab != null)
        {
            Vector2 baseDir = (mouseWorld - transform.position);
            if (baseDir.sqrMagnitude < 0.0001f) baseDir = transform.right;
            baseDir.Normalize();
            float spreadAngle = 10f; // degrees between center and side bullets
            float speed = projectileSpeed > 0f ? projectileSpeed : (FindFirstObjectByType<GlobalPlayerInfo>()?.bulletSpeed ?? 10f);

            for (int i = -1; i <= 1; i++)
            {
                float angle = i * spreadAngle;
                Vector2 dir = Quaternion.Euler(0f, 0f, angle) * baseDir;
                var proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                var prb = proj.GetComponent<Rigidbody2D>();
                if (prb != null)
                    prb.linearVelocity = dir * speed;
            }
        }

        // start dash coroutine toward mouse
        Vector2 dashDir = (mouseWorld - transform.position);
        if (dashDir.sqrMagnitude < 0.0001f) dashDir = transform.right;
        StartCoroutine(PerformDash(dashDir.normalized, dashDistance, dashDuration));
    }

    IEnumerator PerformDash(Vector2 direction, float distance, float duration)
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

       
        // var enemyHealths = FindObjectsOfType<EnemyHealth>();
        var enemyHealths = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);

        List<Collider2D> enemyColliders = new List<Collider2D>();
        foreach (var eh in enemyHealths)
        {
            var cols = eh.GetComponentsInChildren<Collider2D>();
            foreach (var c in cols) if (c != null) enemyColliders.Add(c);
        }

        
        foreach (var pc in myColliders)
            foreach (var ec in enemyColliders)
                Physics2D.IgnoreCollision(pc, ec, true);

        // disable player movement script while dashing (to avoid it overwriting velocity)
        var movementComp = GetComponent<PlayerMovement>();
        if (movementComp != null) movementComp.enabled = false;

        Vector2 start = rb != null ? rb.position : (Vector2)transform.position;
        Vector2 target = start + direction * distance;

        HashSet<EnemyHealth> damaged = new HashSet<EnemyHealth>();

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            Vector2 nextPos = Vector2.Lerp(start, target, t);

            // check walls between current pos and nextPos; if wall hit -> stop at wall
            Vector2 curPos = rb != null ? rb.position : (Vector2)transform.position;
            Vector2 moveDir = nextPos - curPos;
            float moveDist = moveDir.magnitude;
            if (moveDist > 0.0001f)
            {
                // raycast and ignore any of the player's own colliders so the ray doesn't hit self
                RaycastHit2D hit = default;
                var rayHits = Physics2D.RaycastAll(curPos, moveDir.normalized, moveDist);
                foreach (var h in rayHits)
                {
                    if (h.collider == null) continue;
                    bool isSelf = false;
                    if (myColliders != null)
                    {
                        foreach (var mc in myColliders)
                        {
                            if (mc == h.collider) { isSelf = true; break; }
                        }
                    }
                    if (isSelf) continue;
                    hit = h;
                    break;
                }

                if (hit.collider != null)
                {
                    // if we hit an enemy, register damage but do not stop
                    var eh = hit.collider.GetComponentInParent<EnemyHealth>();
                    if (eh != null)
                    {
                        if (!damaged.Contains(eh))
                        {
                            eh.TakeDamage(dashDamage);
                            damaged.Add(eh);
                        }
                    }
                    else if (!hit.collider.isTrigger)
                    {
                        // hit wall/obstacle -> stop before it
                        Vector2 stopPos = hit.point - moveDir.normalized * 0.02f;
                        if (rb != null) rb.MovePosition(stopPos);
                        else transform.position = stopPos;
                        break;
                    }
                }
            }

            // move
            if (rb != null)
            {
                rb.MovePosition(nextPos);
                rb.linearVelocity = Vector2.zero;
            }
            else
            {
                transform.position = nextPos;
            }

            // also check overlap circle for hitting enemies
            Collider2D[] hits = Physics2D.OverlapCircleAll(nextPos, damageRadius);
            foreach (var c in hits)
            {
                var eh = c.GetComponentInParent<EnemyHealth>();
                if (eh != null && !damaged.Contains(eh))
                {
                    eh.TakeDamage(dashDamage);
                    damaged.Add(eh);
                }
            }

            yield return null;
        }

        // re-enable collisions and player's colliders
        if (myColliders != null)
        {
            foreach (var pc in myColliders)
            {
                if (pc == null) continue;
                foreach (var ec in enemyColliders)
                    Physics2D.IgnoreCollision(pc, ec, false);
                pc.enabled = true;
            }
        }

        if (movementComp != null) movementComp.enabled = true;
    }

    // Draw debug gizmos in the Scene view so dash distance and damage radius are visible
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, damageRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + transform.right * dashDistance);
        Gizmos.DrawWireSphere(transform.position + transform.right * dashDistance, 0.05f);
    }
    private void Update()
    {
        // for testing: press F to activate ability
        if (Input.GetKeyDown(KeyCode.F))
        {
            ActivateAbility();
        }

    }
}
