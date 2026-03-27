using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class NuclearBomb : MonoBehaviour
{
    [Header("AOE")]
    public float radius = 5f;
    public float damage = 10000f;
    

    [Header("Timing")]
    public float duration = 3f; // how long lasers orbit
    public float cooldown = 10f; // cooldown between uses

    [Header("Lasers")]
    public GameObject laserPrefab; // optional prefab for lasers; if null, simple spheres will be created
    public int laserCount = 5;
    public float orbitRadius = 2.5f;

    bool isActive = false;
    float lastUsedTime = -999f;
    public ParticleSystem part;
    public bool debugLog = true;

    List<GameObject> orbiting = new List<GameObject>();

    public void ActivateAbility()
    {
        if (Time.time < lastUsedTime + cooldown)
        {
            if (debugLog) Debug.Log($"NuclearBomb: still on cooldown ({(lastUsedTime + cooldown) - Time.time:F2}s left)");
            return; // still on cooldown
        }

        lastUsedTime = Time.time;
        if (part != null) part.Play();
        StartCoroutine(ActivateRoutine());
    }

    IEnumerator ActivateRoutine()
    {
        // Apply instant AOE damage to enemies inside radius (2D + 3D)
        var damaged = new HashSet<EnemyHealth>();

        // helper to find the nearest parent with tag "Enemy"
        Transform FindTaggedParent(Transform start)
        {
            Transform t = start;
            while (t != null)
            {
                if (t.CompareTag("Enemy")) return t;
                t = t.parent;
            }
            return null;
        }

        // 2D colliders - only consider objects whose collider or parent is tagged "Enemy"
        Collider2D[] cols2d = Physics2D.OverlapCircleAll((Vector2)transform.position, radius);
        if (debugLog)
        {
            Debug.Log($"NuclearBomb: Found {cols2d.Length} 2D colliders in radius");
            foreach (var c in cols2d)
            {
                if (c == null) continue;
                Debug.Log($"  2D Collider: name={c.gameObject.name} tag={c.gameObject.tag} parent={ (c.transform.parent? c.transform.parent.name : "null") }");
            }
        }
        foreach (var c in cols2d)
        {
            if (c == null) continue;

            // Prefer to find EnemyHealth directly on the collider or its parents/children
            EnemyHealth eh = c.GetComponent<EnemyHealth>() ?? c.GetComponentInParent<EnemyHealth>() ?? c.GetComponentInChildren<EnemyHealth>();
            if (eh != null)
            {
                damaged.Add(eh);
                if (debugLog) Debug.Log($"NuclearBomb: Found EnemyHealth on collider {c.gameObject.name} -> {eh.gameObject.name}");
                continue;
            }

            // Fallback: use tag to find the enemy root then get EnemyHealth there
            Transform tagged = null;
            if (c.gameObject.CompareTag("Enemy")) tagged = c.transform;
            else tagged = FindTaggedParent(c.transform);

            if (tagged != null)
            {
                EnemyHealth eh2 = tagged.GetComponent<EnemyHealth>() ?? tagged.GetComponentInChildren<EnemyHealth>();
                if (eh2 != null)
                {
                    damaged.Add(eh2);
                    if (debugLog) Debug.Log($"NuclearBomb: Found EnemyHealth via tag on {tagged.gameObject.name} -> {eh2.gameObject.name}");
                }
                else if (debugLog)
                {
                    Debug.Log($"NuclearBomb: Tagged object {tagged.gameObject.name} had no EnemyHealth");
                }
            }
        }

        // Fallback: if no enemies found by collider checks, also check all GameObjects tagged "Enemy" by distance
        GameObject[] enemiesByTag = GameObject.FindGameObjectsWithTag("Enemy");
        if (debugLog) Debug.Log($"NuclearBomb: Found {enemiesByTag.Length} GameObjects with tag 'Enemy' in scene (fallback check)");
        foreach (var go in enemiesByTag)
        {
            if (go == null) continue;
            float dist = Vector2.Distance(transform.position, go.transform.position);
            if (dist <= radius)
            {
                EnemyHealth eh = go.GetComponent<EnemyHealth>() ?? go.GetComponentInChildren<EnemyHealth>() ?? go.GetComponentInParent<EnemyHealth>();
                if (eh != null)
                {
                    if (!damaged.Contains(eh))
                    {
                        damaged.Add(eh);
                        if (debugLog) Debug.Log($"NuclearBomb: Fallback added enemy {go.name} at distance {dist}");
                    }
                }
                else if (debugLog)
                {
                    Debug.Log($"NuclearBomb: Fallback enemy {go.name} had no EnemyHealth component");
                }
            }
            else if (debugLog)
            {
                // optional: log enemies out of range
                // Debug.Log($"NuclearBomb: Fallback enemy {go.name} is out of range (dist={dist})");
            }
        }

        // 3D colliders fallback - same tag requirement
        Collider[] cols3d = Physics.OverlapSphere(transform.position, radius);
        if (debugLog)
        {
            Debug.Log($"NuclearBomb: Found {cols3d.Length} 3D colliders in radius");
            foreach (var c in cols3d)
            {
                if (c == null) continue;
                Debug.Log($"  3D Collider: name={c.gameObject.name} tag={c.gameObject.tag} parent={ (c.transform.parent? c.transform.parent.name : "null") }");
            }
        }
        foreach (var c in cols3d)
        {
            if (c == null) continue;

            EnemyHealth eh = c.GetComponent<EnemyHealth>() ?? c.GetComponentInParent<EnemyHealth>() ?? c.GetComponentInChildren<EnemyHealth>();
            if (eh != null)
            {
                damaged.Add(eh);
                if (debugLog) Debug.Log($"NuclearBomb: Found EnemyHealth on collider {c.gameObject.name} -> {eh.gameObject.name}");
                continue;
            }

            Transform tagged = null;
            if (c.gameObject.CompareTag("Enemy")) tagged = c.transform;
            else tagged = FindTaggedParent(c.transform);

            if (tagged != null)
            {
                EnemyHealth eh2 = tagged.GetComponent<EnemyHealth>() ?? tagged.GetComponentInChildren<EnemyHealth>();
                if (eh2 != null)
                {
                    damaged.Add(eh2);
                    if (debugLog) Debug.Log($"NuclearBomb: Found EnemyHealth via tag on {tagged.gameObject.name} -> {eh2.gameObject.name}");
                }
                else if (debugLog)
                {
                    Debug.Log($"NuclearBomb: Tagged object {tagged.gameObject.name} had no EnemyHealth");
                }
            }
        }

        int hits = 0;
        float appliedDamage = 10000f; // force 10000 damage regardless of inspector value
        foreach (var eh in damaged)
        {
            if (eh == null) continue;
            if (debugLog) Debug.Log($"NuclearBomb: Damaging {eh.gameObject.name} for {appliedDamage}");
            eh.TakeDamage(appliedDamage);
            hits++;
        }
        if (debugLog) Debug.Log($"NuclearBomb: AOE unique hit count = {hits}");

        // Spawn orbiting lasers
        orbiting.Clear();
        for (int i = 0; i < laserCount; i++)
        {
            GameObject obj;
            if (laserPrefab != null)
            {
                obj = Instantiate(laserPrefab, transform.position, Quaternion.identity, transform);
                // make sure it's a child and on Z=0 for 2D
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;
                // remove 3D colliders that might interfere
                var col3 = obj.GetComponent<Collider>(); if (col3 != null) Destroy(col3);
                var col2 = obj.GetComponent<Collider2D>(); if (col2 != null) Destroy(col2);
                // if it has a LineRenderer, set to local-space so we can rotate the parent
                var lrPrefab = obj.GetComponent<LineRenderer>();
                if (lrPrefab != null)
                {
                    lrPrefab.useWorldSpace = false;
                    lrPrefab.positionCount = 2;
                    lrPrefab.startWidth = 0.05f;
                    lrPrefab.endWidth = 0.02f;
                    lrPrefab.numCapVertices = 2;
                    if (lrPrefab.material == null) lrPrefab.material = new Material(Shader.Find("Sprites/Default"));
                }
            }
            else
            {
                // create a simple 2D laser visual using LineRenderer so it looks correct in 2D
                obj = new GameObject("Laser");
                obj.transform.SetParent(transform);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;
                var lr = obj.AddComponent<LineRenderer>();
                lr.positionCount = 2;
                lr.useWorldSpace = false; // local space makes 2D rotation easier
                lr.startWidth = 0.05f;
                lr.endWidth = 0.02f;
                lr.numCapVertices = 2;
                lr.material = new Material(Shader.Find("Sprites/Default"));
                lr.startColor = Color.cyan;
                lr.endColor = Color.blue;
                // make a short line pointing right in local space
                Vector3 a = new Vector3(0.05f, 0f, 0f);
                Vector3 b = new Vector3(0.25f, 0f, 0f);
                lr.SetPosition(0, a);
                lr.SetPosition(1, b);
            }

            orbiting.Add(obj);
        }

        isActive = true;
        float elapsed = 0f;
        // Each laser will complete 360 degrees over the duration (orbit in XY plane for 2D)
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float angleOffset = 360f * t;

            for (int i = 0; i < orbiting.Count; i++)
            {
                if (orbiting[i] == null) continue;
                float baseAngle = (360f / orbiting.Count) * i;
                float angle = baseAngle + angleOffset;
                float rad = angle * Mathf.Deg2Rad;
                // XY plane (z = 0) - use localPosition so children orbit correctly
                Vector3 localPos = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * orbitRadius;
                orbiting[i].transform.localPosition = localPos;

                // orient visuals to face outward from center (local rotation)
                Vector3 dir = localPos.normalized;
                float angleZ = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                orbiting[i].transform.localRotation = Quaternion.Euler(0f, 0f, angleZ);

                var lr = orbiting[i].GetComponent<LineRenderer>();
                if (lr != null)
                {
                    // if LineRenderer uses world space, update endpoints in world coords
                    if (lr.useWorldSpace)
                    {
                        Vector3 worldPos = orbiting[i].transform.position;
                        lr.SetPosition(0, worldPos + (Vector3)dir * 0.05f);
                        lr.SetPosition(1, worldPos + (Vector3)dir * 0.25f);
                    }
                    // if lr is local space we leave its local endpoints as created
                }
                else
                {
                    // if it's a sprite or mesh, align its up to the outward direction for better visuals
                    var sr = orbiting[i].GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        orbiting[i].transform.up = dir;
                    }
                }
            }

            yield return null;
        }

        // cleanup
        foreach (var o in orbiting)
        {
            if (o != null) Destroy(o);
        }
        orbiting.Clear();
        isActive = false;
        if (part != null) part.Stop();
    }
    // removed automatic activation from Update so ability must be triggered explicitly
    public void Update()
    {
        // quick test trigger: press 'K' to activate ability
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (debugLog) Debug.Log("NuclearBomb: Test key pressed -> ActivateAbility");
            ActivateAbility();
        }
        //if (cooldown > 0f)
        //{
        //    float timeSinceUse = Time.time - lastUsedTime;
        //    if (timeSinceUse < cooldown)
        //    {
        //        // Optionally, you could add visual feedback for cooldown here
        //    }
        //}
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
        Gizmos.DrawSphere(transform.position, radius);
    }
}
