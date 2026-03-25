using UnityEngine;
using System.Collections;
using JetBrains.Annotations;


public class LazerBOOM : MonoBehaviour
{
    [Header("Cooldown")]
    public float abilityCooldown = 10f;
    public float timer = 0f;

    [Header("Beam")]
    public float activateDelay = 0.3f; // wait before animation starts
    public float stayDuration = 3f; // how long beam stays connected
    public int damagePerSecond = 20;
    public float beamWidth = 0.6f;
    public float maxRange = 10f; // max beam distance
    public float growDuration = 0.5f; // time it takes the beam to reach maxRange
    public LayerMask enemyLayer;
    public float startWidthMultiplier = 1.5f; // how much bigger the start is relative to currentWidth
    public float endWidthMultiplier = 0.5f;   // how much smaller the end is relative to currentWidth

    [Header("Line End Offset")]
    public Vector2 endPositionOffset = new Vector2(0f, -0.05f); // visual offset applied to line index 1 (x,y)

    [Header("References (set in prefab)")]
    public Transform beamOrigin; // optional: if null uses this.transform.position
    public Color beamColor = Color.cyan;
    public LineRenderer line;
   // public ParticleSystem beamParticles; // optional particle system to emit along the beam
    //public float particleSpeed = 5f; // speed at which particles move along the beam direction

    private Coroutine activeRoutine;

    // (VFX removed) track state not needed when using only LineRenderer
    private void Awake()
    {
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
    }

    public void ActivateAbility()
    {
        
        if (timer > 0f) return; // still cooling down
        timer = abilityCooldown;
        if (activeRoutine != null) StopCoroutine(activeRoutine);
        activeRoutine = StartCoroutine(DoBeamSequence());
    }

    private IEnumerator DoBeamSequence()
    {
        yield return new WaitForSeconds(activateDelay);

        // make sure there's a line renderer to show something
        if (line != null) { line.enabled = true; line.positionCount = 2; line.startWidth = 0f; line.endWidth = 0f; }
       // if (beamParticles != null) beamParticles.Play();

        float activeElapsed = 0f;
        while (activeElapsed < stayDuration)
        {
            activeElapsed += Time.deltaTime;

            // determine beam start point
            Vector3 start = (beamOrigin != null) ? beamOrigin.position : transform.position;

            // determine mouse world position (2D) so the beam follows the player's aim
            Vector3 mouseWorld = start;
            Camera cam = Camera.main;
            if (cam != null)
            {
                // ScreenToWorldPoint expects a z distance from the camera. Calculate
                // the distance from the camera to the beam start's z plane so the
                // projected mouse position lies on the same z as the beam origin.
                float zDist = start.z - cam.transform.position.z;
                Vector3 mp = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, zDist));
                mouseWorld = mp;
            }
            else
            {
                // fallback direction if no camera available
                mouseWorld = start + transform.right;
            }

            Vector3 dir = mouseWorld - start;
            float distToMouse = dir.magnitude;
            if (distToMouse > 0.0001f) dir /= distToMouse;
            else dir = transform.right; // arbitrary default

            // gradually grow the beam over `growDuration` seconds, hold, then shrink
            // in the final `growDuration` seconds. This gives a smooth in/out behavior.
            float growT = (growDuration > 0f) ? Mathf.Clamp01(activeElapsed / growDuration) : 1f;
            float shrinkT = (growDuration > 0f) ? Mathf.Clamp01((stayDuration - activeElapsed) / growDuration) : 1f;
            float beamScale = Mathf.Min(growT, shrinkT); // grows, then holds, then shrinks
            float currentRange = Mathf.Lerp(0f, maxRange, beamScale);
            Vector3 end = start + dir * currentRange;

            // debug draw
            Debug.DrawLine(start, end, beamColor);

            // area damage (keeps original behavior but now along beam from start->end)
            Vector2 wStart = start;
            Vector2 wEnd = end;
            Vector2 center = (wStart + wEnd) * 0.5f;
            float length = Vector2.Distance(wStart, wEnd);
            Vector2 size = new Vector2(Mathf.Max(0.05f, length), beamWidth);

            Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg, enemyLayer);
            foreach (var c in hits)
            {
                if (c == null) continue;
                var eh = c.GetComponent<EnemyHealth>();
                if (eh != null) eh.TakeDamage(damagePerSecond * Time.deltaTime);
            }

            // precise hit detection along beam to report a hit point to VFX (so you can show impact)
            RaycastHit2D rhit;
            Vector3 hitPoint = GetHitPosition(start, dir, currentRange, out rhit);
            if (rhit.collider != null)
            {
                var eh = rhit.collider.GetComponent<EnemyHealth>();
                if (eh != null) eh.TakeDamage(damagePerSecond * Time.deltaTime);
            }

            // no VFX: only update line renderer

            // compute current width once so both renderer and particle shape can use it
            float currentWidth = Mathf.Lerp(0.01f, beamWidth, beamScale);

            if (line != null)
            {
                // ensure consistent visual centering relative to the camera
                line.alignment = LineAlignment.View;

                // taper the beam: start can be larger than the end (end narrower)
                line.startWidth = currentWidth * startWidthMultiplier;
                line.endWidth = currentWidth * endWidthMultiplier;
                line.SetPosition(0, new Vector3(start.x, start.y, 0f));
                // apply a small visual offset to the end point (index 1)
                Vector3 endVis = end + new Vector3(endPositionOffset.x, endPositionOffset.y, 0f);
                line.SetPosition(1, endVis);
            }

            // update optional particle system to follow the beam
            //if (beamParticles != null)
            //{
            //    // use the visual end (with offset) when positioning particles so they align with the rendered line
            //    Vector3 endVis = end + new Vector3(endPositionOffset.x, endPositionOffset.y, 0f);
            //    Vector3 mid = (start + endVis) * 0.5f;
            //    beamParticles.transform.position = mid;
            //    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            //    beamParticles.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            //    var shape = beamParticles.shape;
            //    // stretch the particle system shape along the beam so particles emit along it
            //    shape.scale = new Vector3(currentRange, Mathf.Max(0.01f, currentWidth * 0.5f), 1f);

            //    // ensure particles move along the beam direction (not towards the player)
            //    var main = beamParticles.main;
            //    main.gravityModifier = 0f;
            //    main.simulationSpace = ParticleSystemSimulationSpace.World;

            //    var vel = beamParticles.velocityOverLifetime;
            //    vel.enabled = true;
            //    vel.space = ParticleSystemSimulationSpace.World;
            //    // set a constant velocity along the beam direction
            //    vel.x = new ParticleSystem.MinMaxCurve(dir.x * particleSpeed);
            //    vel.y = new ParticleSystem.MinMaxCurve(dir.y * particleSpeed);
            //    vel.z = new ParticleSystem.MinMaxCurve(0f);
            //}

            yield return null;
        }

        // stop particle system and disable line
    //    if (beamParticles != null) beamParticles.Stop();
        if (line != null) line.enabled = false;
        activeRoutine = null;
    }

    // visualize the beam area in editor when selected
    private void OnDrawGizmosSelected()
    {
        Vector3 start = (beamOrigin != null) ? beamOrigin.position : transform.position;
        Vector3 end = start + transform.right * maxRange;
        Gizmos.color = Color.cyan;
        Vector3 center = (start + end) * 0.5f;
        float len = Vector3.Distance(start, end);
        Gizmos.DrawWireCube(center, new Vector3(len, beamWidth, 0.1f));
    }

    // (VFX removed) all visual-effect helpers deleted; renderer-only implementation

    private Vector3 GetHitPosition(Vector3 start, Vector3 dir, float range, out RaycastHit2D hit)
    {
        hit = Physics2D.Raycast(start, dir, range, enemyLayer);
        return (hit.collider != null) ? hit.point : start + dir * range;
    }
}
