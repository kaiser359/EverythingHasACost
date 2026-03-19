using System.Collections;
using UnityEngine;

public class AoEEffect : MonoBehaviour
{
    public float duration = 5f;
    public float tickInterval = 1f;
    public float damagePerTick = 5f;
    public float healPerTick = 5f;
    public float radius = 3f;
    public string enemyTag = "Enemy";
    public string playerTag = "Player";

    private void Start()
    {
        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            // damage enemies in radius
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
            foreach (var c in hits)
            {
                if (c.CompareTag(enemyTag))
                {
                    var eh = c.GetComponent<EnemyHealth>();
                    if (eh != null && damagePerTick > 0f)
                        eh.TakeDamage(damagePerTick);
                }
                else if (c.CompareTag(playerTag))
                {
                    // heal player - try to find PlayerStats
                    var ps = c.GetComponent<PlayerStats>();
                    if (ps != null && healPerTick > 0f)
                        ps.Heal(healPerTick);
                }
            }

            yield return new WaitForSeconds(tickInterval);
            elapsed += tickInterval;
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0.5f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
