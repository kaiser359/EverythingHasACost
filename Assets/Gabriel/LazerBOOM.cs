using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class LazerBOOM : MonoBehaviour
{
    [Header("Cooldown")]
    public float abilityCooldown = 10f;
    public float timer = 0f;
    public ParticleSystem SpecialParticles;
    public float particleSpeed = 5f;

    [Header("Beam")]
    public float activateDelay = 0.3f;
    public float stayDuration = 3f;
    public int damagePerSecond = 20;
    public float beamWidth = 0.6f;
    public float maxRange = 10f;
    public float growDuration = 0.5f;
    public LayerMask enemyLayer;
    public float startWidthMultiplier = 1.5f;
    public float endWidthMultiplier = 0.5f;
    public AudioClip lazer;

    [Header("Line End Offset")]
    public Vector2 endPositionOffset = new Vector2(0f, -0.05f);

    [Header("References (set in prefab)")]
    public Transform beamOrigin;
    public Color beamColor = Color.cyan;
    public LineRenderer line;

    public StarRatings star;
    public ParticleSystem enemyHitEffectPrefab;

    private Coroutine activeRoutine;
   // private HashSet<int> effectedEnemies = new HashSet<int>();

    private void Awake()
    {
        stayDuration += star.StartRating;
        damagePerSecond += star.StartRating * 5;
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            beamOrigin = player.transform;
        }
        else if (beamOrigin == null)
        {
            beamOrigin = this.transform;
        }
    }

    private void Update()
    {
        if (timer > 0f) timer -= Time.deltaTime;
        //if (Input.GetKeyDown(KeyCode.K))
        //{
        //    ActivateAbility();
        //}
    }

    public void ActivateAbility()
    {
        if (timer > 0f) return;
        timer = abilityCooldown;
        SpecialParticles?.Play();
        if (activeRoutine != null) StopCoroutine(activeRoutine);
        activeRoutine = StartCoroutine(DoBeamSequence());
        FindAnyObjectByType<AudioSource>().PlayOneShot(lazer);
    }

    private IEnumerator DoBeamSequence()
    {
        yield return new WaitForSeconds(activateDelay);

        if (line != null) { line.enabled = true; line.positionCount = 2; line.startWidth = 0f; line.endWidth = 0f; }

        // Clear set of effected enemies at start of this activation so effects can be re-applied on subsequent uses
        //effectedEnemies.Clear();

        float activeElapsed = 0f;
        while (activeElapsed < stayDuration)
        {
            activeElapsed += Time.deltaTime;

            Vector3 start = (beamOrigin != null) ? beamOrigin.position : transform.position;

            Vector3 mouseWorld = start;
            Camera cam = Camera.main;
            if (cam != null)
            {
                Plane plane = new Plane(Vector3.forward, start);
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                if (plane.Raycast(ray, out float enter)) mouseWorld = ray.GetPoint(enter);
                else
                {
                    float zDist = start.z - cam.transform.position.z;
                    mouseWorld = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, zDist));
                    mouseWorld.z = start.z;
                }
            }
            else
            {
                mouseWorld = start + transform.right;
            }

            Vector3 dir = mouseWorld - start;
            float distToMouse = dir.magnitude;
            if (distToMouse > 0.0001f) dir /= distToMouse;
            else dir = transform.right;

            float growT = (growDuration > 0f) ? Mathf.Clamp01(activeElapsed / growDuration) : 1f;
            float shrinkT = (growDuration > 0f) ? Mathf.Clamp01((stayDuration - activeElapsed) / growDuration) : 1f;
            float beamScale = Mathf.Min(growT, shrinkT);
            float currentRange = Mathf.Lerp(0f, maxRange, beamScale);

            RaycastHit2D rhit;
            Vector3 end = GetHitPosition(start, dir, currentRange, out rhit);

            Debug.DrawLine(start, end, beamColor);

            Vector2 wStart = start;
            Vector2 wEnd = end;
            Vector2 center = (wStart + wEnd) * 0.5f;
            float length = Vector2.Distance(wStart, wEnd);
            Vector2 size = new Vector2(Mathf.Max(0.05f, length), beamWidth);

            Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg, enemyLayer);
            foreach (var c in hits)
            {
                if (c == null) continue;

                // ignore player or anything attached to the beam origin (so the beam passes through the player)
                if (c.CompareTag("Player")) continue;
                if (beamOrigin != null && c.transform.IsChildOf(beamOrigin)) continue;

                var eh = c.GetComponent<EnemyHealth>();
                if (eh != null)
                {
                    eh.TakeDamage(damagePerSecond * Time.deltaTime);
                    if (enemyHitEffectPrefab != null)
                    {
                        int id = eh.GetInstanceID();
                    //    if (effectedEnemies.Add(id))
                     //   {
                            var target = eh.gameObject;
                            var ps = Instantiate(enemyHitEffectPrefab, target.transform.position, Quaternion.identity, target.transform);
                            ps.transform.localPosition = Vector3.zero;
                            var main = ps.main;
                            main.simulationSpace = ParticleSystemSimulationSpace.Local;
                            ps.Play();
                     //   }
                    }
                }
            }

            if (rhit.collider != null && rhit.collider.CompareTag("Enemy"))
            {
                var eh = rhit.collider.GetComponent<EnemyHealth>();
                if (eh != null) eh.TakeDamage(damagePerSecond * Time.deltaTime);
            }

            float currentWidth = Mathf.Lerp(0.01f, beamWidth, beamScale);

            if (line != null)
            {
                line.alignment = LineAlignment.View;
                line.startWidth = currentWidth * startWidthMultiplier;
                line.endWidth = currentWidth * endWidthMultiplier;
                line.SetPosition(0, new Vector3(start.x, start.y, start.z));
                Vector3 perp = Vector3.Cross(dir, Vector3.forward).normalized;
                Vector3 endVis = end + dir * endPositionOffset.x + perp * endPositionOffset.y;
                endVis.z = start.z;
                line.SetPosition(1, endVis);
            }

            if (SpecialParticles != null)
            {
                // Place the particle system at the beam start and rotate it to face the beam direction.
                SpecialParticles.transform.position = start;

                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                SpecialParticles.transform.rotation = Quaternion.Euler(0f, 0f, angle);

                // Configure the shape so emission box sits in front of the start (half the range forward)
                var shape = SpecialParticles.shape;
                shape.shapeType = ParticleSystemShapeType.Box;
                shape.position = new Vector3(currentRange * 0.5f, 0f, 0f); // local offset forward
                shape.rotation = Vector3.zero;
                shape.scale = new Vector3(currentRange, Mathf.Max(0.01f, currentWidth), 1f);

                // Run particles in local space so they spawn relative to the start and respect rotation
                var main = SpecialParticles.main;
                main.gravityModifier = 0f;
                main.simulationSpace = ParticleSystemSimulationSpace.Local;

                // Velocity in local space along +X (particle system rotated to beam direction)
                var vel = SpecialParticles.velocityOverLifetime;
                vel.enabled = true;
                vel.space = ParticleSystemSimulationSpace.Local;
                vel.x = new ParticleSystem.MinMaxCurve(particleSpeed);
                vel.y = new ParticleSystem.MinMaxCurve(0f);
                vel.z = new ParticleSystem.MinMaxCurve(0f);

                var emission = SpecialParticles.emission;
                emission.enabled = true;
            }

            yield return null;
        }

        SpecialParticles?.Stop();
        if (line != null) line.enabled = false;
        activeRoutine = null;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 start = (beamOrigin != null) ? beamOrigin.position : transform.position;
        Vector3 end = start + transform.right * maxRange;
        Gizmos.color = Color.cyan;
        Vector3 center = (start + end) * 0.5f;
        float len = Vector3.Distance(start, end);
        Gizmos.DrawWireCube(center, new Vector3(len, beamWidth, 0.1f));
    }

    private Vector3 GetHitPosition(Vector3 start, Vector3 dir, float range, out RaycastHit2D hit)
    {
        var hits = Physics2D.RaycastAll(start, dir, range);
        RaycastHit2D firstEnemy = new RaycastHit2D();
        float bestDist = float.MaxValue;
        foreach (var h in hits)
        {
            if (h.collider == null) continue;

            // ignore player's collider and anything attached to the beam origin so the beam won't stop on the player
            if (h.collider.CompareTag("Player")) continue;
            if (beamOrigin != null && (h.collider.transform.IsChildOf(beamOrigin) || h.collider.gameObject == beamOrigin.gameObject)) continue;

            float d = h.distance;
            if (d < bestDist)
            {
                if (h.collider.CompareTag("Enemy"))
                {
                    firstEnemy = h;
                    bestDist = d;
                    continue;
                }

                hit = h;
                return h.point;
            }
        }

        if (firstEnemy.collider != null)
        {
            hit = firstEnemy;
            return firstEnemy.point;
        }

        hit = default;
        return start + dir * range;
    }
}
