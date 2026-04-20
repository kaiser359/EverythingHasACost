using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BlueAbility : MonoBehaviour
{
    public StarRatings star;

    public float dashDistance = 6f;
    public float dashDuration = 0.18f;
    public float dashDamage = 25f;
    public float damageRadius = 0.6f;
    public Collider2D bluebagcol;
    public float cooldown;
    public GameObject Shield;
    public float shieldHealth = 50;
    public float shieldDuration = 0f;

    // projectile settings (will fall back to GlobalPlayerInfo if null)
    public GameObject projectilePrefab;
    public float projectileSpeed = 14f;

    Rigidbody2D rb;
    [SerializeField]
    Collider2D[] myColliders;

    // player references (the ability lives on a bloodbag; dash should move the player)
    GameObject playerObject;
    Rigidbody2D playerRb;
    Collider2D[] playerColliders;
    bool[] _originalPlayerColliderStates;
    PlayerMovement playerMovement;
    List<Behaviour> _disabledBehaviours;

    // store original collider enabled states if needed
    bool[] _originalColliderStates;

    void Awake()
    {
        Shield.SetActive(false);
        rb = GetComponent<Rigidbody2D>();
        // include child colliders as well in case colliders are on child objects
        myColliders = GetComponentsInChildren<Collider2D>(true);
        if (myColliders != null)
        {
            _originalColliderStates = new bool[myColliders.Length];
            for (int i = 0; i < myColliders.Length; i++) _originalColliderStates[i] = myColliders[i] != null && myColliders[i].enabled;
        }

        // try to locate the player automatically by tag at startup
        FindPlayerIfNeeded();
        // debug info to help diagnose small movement (run after player lookup)

    }

    void FindPlayerIfNeeded()
    {
        if (playerObject != null) return;
        try
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null)
            {
                playerObject = p;
                playerRb = playerObject.GetComponent<Rigidbody2D>();
                if (playerRb == null) playerRb = playerObject.GetComponentInChildren<Rigidbody2D>(true);
                playerColliders = playerObject.GetComponentsInChildren<Collider2D>(true);
                playerMovement = playerObject.GetComponentInChildren<PlayerMovement>(true);
            }
            else
            {
                // fallback: try to find any PlayerMovement instance and use its GameObject
                var pm = FindFirstObjectByType<PlayerMovement>();
                if (pm != null)
                {
                playerObject = pm.gameObject;
                playerRb = playerObject.GetComponent<Rigidbody2D>();
                if (playerRb == null) playerRb = playerObject.GetComponentInChildren<Rigidbody2D>(true);
                playerColliders = playerObject.GetComponentsInChildren<Collider2D>(true);
                playerMovement = pm;
                }
            }
        }
        catch { }
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
        if (cooldown > 0) return;
        Shield.SetActive(true);
        shieldDuration = 3f + star.StartRating;
        // ensure we have a reference to the player (ability lives on a bloodbag)
        FindPlayerIfNeeded();
        

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
        cooldown = 5;
        // throw a burst of 3 projectiles toward mouse (center + two spread)
        if (projectilePrefab != null)
        {
            Vector3 spawnPos = playerObject != null ? playerObject.transform.position : transform.position;
            Vector2 baseDir = (mouseWorld - spawnPos);
            if (baseDir.sqrMagnitude < 0.0001f) baseDir = transform.right;
            baseDir.Normalize();
            float spreadAngle = 10f; // degrees between center and side bullets
            float speed = projectileSpeed > 0f ? projectileSpeed : (FindFirstObjectByType<GlobalPlayerInfo>()?.bulletSpeed ?? 10f);

            for (int i = -1; i <= 1; i++)
            {
                float angle = i * spreadAngle;
                Vector2 dir = Quaternion.Euler(0f, 0f, angle) * baseDir;
                var proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
                var prb = proj.GetComponent<Rigidbody2D>();
                if (prb != null)
                    prb.linearVelocity = dir * speed;
            }
        }

        // start dash coroutine: dash in current movement direction (use Rigidbody2D velocity when available)
        Vector2 dashDir;
        if (playerObject != null && playerRb != null && playerRb.linearVelocity.sqrMagnitude > 0.0001f)
        {
            dashDir = playerRb.linearVelocity.normalized;
        }
        else if (playerObject != null)
        {
            // player not moving: dash forward relative to player
            dashDir = (Vector2)playerObject.transform.right;
        }
        else
        {
            dashDir = (Vector2)transform.right;
        }

        StartCoroutine(PerformDash(dashDir.normalized, dashDistance, dashDuration));
    }

    IEnumerator PerformDash(Vector2 direction, float distance, float duration)
    {
        var p = GameObject.FindWithTag("Player");
        Vector2 point = (Vector2)p.transform.position + direction.normalized * distance;

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float newDist = ((Vector2)p.transform.position - point).magnitude;

            Debug.Log(p.transform.position + " moving toward " + point + " dist=" + newDist);

            p.GetComponent<Rigidbody2D>().AddForce((point - (Vector2)p.transform.position).normalized * newDist * 100f);

            //p.transform.position = Vector2.MoveTowards(p.transform.position, point, newDist * 0.05f);

            yield return null;
        }
        p.GetComponent<Collider2D>().enabled = true;
        bluebagcol.enabled = false;
        yield return null;
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
        //    for testing: press F to activate ability
       
        
          //  ActivateAbility();
        
            if (cooldown > 0) cooldown -= Time.deltaTime;
            if (shieldDuration > 0 ) shieldDuration -= Time.deltaTime;

            if (shieldDuration <= 0)
            {
                Shield.SetActive(false);
        }
        //Vector2 dashDir;
        //Camera cam = Camera.main;
        //if (cam == null)
        //    cam = FindFirstObjectByType<GlobalPlayerInfo>()?.mainCamera;
        //Vector3 mp = Input.mousePosition;

        //var p = GameObject.FindWithTag("Player");
        //p.GetComponent<Collider2D>().enabled = false;
        //bluebagcol.enabled = true;
        //Vector3 mouseWorld = Vector3.zero;
        //mouseWorld = cam.ScreenToWorldPoint(mp);
        //dashDir = (mouseWorld - (Vector3)playerObject.transform.position);
        // StartCoroutine(PerformDash(dashDir,dashDistance,dashDuration));
        //   }

    }
}
