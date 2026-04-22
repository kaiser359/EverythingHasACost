using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NuclearBomb : MonoBehaviour
{
    [Header("Spite AOE")]
    public float radius = 5f;             // AOE radius around the spite
    public float damage = 10000f;         // damage applied when enemy reaches center

    [Header("Timing")]
    public float duration = 3f;           // how long the spite remains active
    public float cooldown = 10f;          // cooldown between uses

    [Header("Spite")]
    public GameObject spite;              // prefab to spawn as spite (or a scene object)
    //public ParticleSystem part;           // optional particle effect

    [Header("Suction")]
    public float suctionSpeed = 5f;       // units per second enemies are pulled
    public float killDistance = 0.1f;     // distance to spite at which enemies are killed
    public bool debugLog = true;

    float lastUsedTime = -999f;

    public void ActivateAbility()
    {
        if (Time.time < lastUsedTime + cooldown)
        {
            if (debugLog) Debug.Log($"NuclearBomb: still on cooldown ({(lastUsedTime + cooldown) - Time.time:F2}s left)");
            return;
        }

        lastUsedTime = Time.time;
        StartCoroutine(ActivateRoutine());
    }

    IEnumerator ActivateRoutine()
    {
        // spawn the spite prefab at this object's position; if spite is null, use this object as center
        GameObject activeSpite = null;
        if (spite != null)
        {
            activeSpite = Instantiate(spite, transform.position, spite.transform.rotation);
            activeSpite.SetActive(true);
        }
        else
        {
            activeSpite = this.gameObject;
        }

        // play particle if assigned (prefer particle on spawned spite if available)


        float elapsed = 0f;
        var damagedIds = new HashSet<int>();

        while (elapsed < duration)
        {
            if (activeSpite == null) break;
            Vector3 center = activeSpite.transform.position;

            // pull all tagged enemies within radius toward the spite
            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            for (int i = 0; i < enemies.Length; i++)
            {
                var go = enemies[i];
                if (go == null) continue;

                float dist = Vector3.Distance(center, go.transform.position);
                if (dist > radius) continue;

                // move enemy toward center
                float step = suctionSpeed * Time.deltaTime;
                go.transform.position = Vector3.MoveTowards(go.transform.position, center, step);

                // kill/apply damage when within killDistance
                if (Vector3.Distance(center, go.transform.position) <= killDistance)
                {
                    var eh = go.GetComponent<EnemyHealth>() ?? go.GetComponentInChildren<EnemyHealth>() ?? go.GetComponentInParent<EnemyHealth>();
                    if (eh != null)
                    {
                        if (damagedIds.Add(eh.GetInstanceID()))
                        {
                            if (debugLog) Debug.Log($"NuclearBomb: Applying {damage} damage to {go.name}");
                            eh.TakeDamage(damage);
                        }
                    }
                    else
                    {
                        if (damagedIds.Add(go.GetInstanceID()))
                        {
                            if (debugLog) Debug.Log($"NuclearBomb: Destroying {go.name} (no EnemyHealth)");
                            Destroy(go);
                        }
                    }
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // final sweep: kill anything still inside radius
        if (activeSpite != null)
        {
            Vector3 center = activeSpite.transform.position;
            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var go in enemies)
            {
                if (go == null) continue;
                float d = Vector3.Distance(center, go.transform.position);
                if (d <= radius)
                {
                    var eh = go.GetComponent<EnemyHealth>() ?? go.GetComponentInChildren<EnemyHealth>() ?? go.GetComponentInParent<EnemyHealth>();
                    if (eh != null)
                    {
                        if (damagedIds.Add(eh.GetInstanceID())) eh.TakeDamage(damage);
                    }
                    else
                    {
                        if (damagedIds.Add(go.GetInstanceID())) Destroy(go);
                    }
                }
            }
        }

        // destroy spawned spite if we created one
        if (activeSpite != null && activeSpite != this.gameObject)
        {
            Destroy(activeSpite);
        }

        yield break;
    }

    void Update()
    {
        // quick test trigger: press 'K' to activate ability
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (debugLog) Debug.Log("NuclearBomb: Test key pressed -> ActivateAbility");
            ActivateAbility();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
        Gizmos.DrawSphere(transform.position, radius);
    }
}
