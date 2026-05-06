using UnityEngine;

public class OutOfBounds : MonoBehaviour
{
    public GameObject player;
   
    public bool requireClearLineOfSight = true; // if true, player must be the first hit
    public int damage = 10;

    // New: how long the player must remain "out of bounds" before damage starts (seconds)
    public float timeBeforeDamage = 2.0f;
    // New: time between consecutive damage ticks once damage is active (seconds)
    public float damageInterval = 0.1f;

    // Internal timers
    float outOfBoundsTimer = 0f;
    float damageCooldownTimer = 0f;

    void Start()
    {
        if (player == null)
            player = GameObject.FindWithTag("Player");
    }

    void Update()
    {
        if (player == null) return;

        Vector2 origin = transform.position;
        Vector2 toPlayer = (player.transform.position - transform.position);
        float distanceToPlayer = toPlayer.magnitude;
        if (distanceToPlayer <= 0f)
        {
            // Nothing to do if zero distance
            ResetTimers();
            return;
        }
        Vector2 direction = toPlayer / distanceToPlayer;

        bool playerDetected = false;

        if (requireClearLineOfSight)
        {
            // Cast toward the player and require the player to be the first hit.
            RaycastHit2D hit = Physics2D.Raycast(origin, direction, distanceToPlayer);
            if (hit.collider != null && hit.collider.gameObject == player)
            {
                playerDetected = true;
            }
        }
        else
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, distanceToPlayer);
            foreach (var h in hits)
            {
                if (h.collider != null && h.collider.gameObject == player)
                {
                    playerDetected = true;
                    break;
                }
            }
        }

        if (playerDetected)
        {
            // accumulate time the player has been detected
            outOfBoundsTimer += Time.deltaTime;

            // only start applying damage after the grace period
            if (outOfBoundsTimer >= timeBeforeDamage)
            {
                damageCooldownTimer += Time.deltaTime;
                if (damageCooldownTimer >= damageInterval)
                {
                    var playerHealth = player.GetComponent<HealthBar>();
                    if (playerHealth != null)
                        playerHealth.TakeDamage(damage);

                    damageCooldownTimer = 0f;
                }
            }
        }
        else
        {
            ResetTimers();
        }
    }

    void ResetTimers()
    {
        outOfBoundsTimer = 0f;
        damageCooldownTimer = 0f;
    }

    void OnDrawGizmosSelected()
    {
        if (player == null)
            player = GameObject.FindWithTag("Player");

        Vector3 origin = transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, 0.1f);

        if (player == null) return;

        Vector3 playerPos = player.transform.position;
        Vector3 toPlayer = playerPos - origin;
        float distanceToPlayer = toPlayer.magnitude;
        if (distanceToPlayer <= 0f) return;
        Vector2 direction = toPlayer / distanceToPlayer;

        bool playerVisible = false;

        if (requireClearLineOfSight)
        {
            RaycastHit2D hit = Physics2D.Raycast(origin, direction, distanceToPlayer);
            if (hit.collider != null && hit.collider.gameObject == player)
                playerVisible = true;
        }
        else
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, distanceToPlayer);
            foreach (var h in hits)
            {
                if (h.collider != null && h.collider.gameObject == player)
                {
                    playerVisible = true;
                    break;
                }
            }
        }

        Gizmos.color = playerVisible ? Color.green : Color.red;
        Gizmos.DrawLine(origin, playerPos);
        Gizmos.DrawWireSphere(playerPos, 0.12f);
    }
}
