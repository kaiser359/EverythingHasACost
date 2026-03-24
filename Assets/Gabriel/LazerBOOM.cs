using UnityEngine;
using System.Collections;
using UnityEngine.VFX;

public class LazerBOOM : MonoBehaviour
{
    [Header("Cooldown")]
    public float abilityCooldown = 10f;
    public float timer = 0f;

    [Header("Beam")]
    public float activateDelay = 0.3f; // wait before animation starts
    public float connectTime = 0.5f; // time for halves to connect
    public float stayDuration = 3f; // how long beam stays connected
    public float damagePerSecond = 20f;
    public float beamWidth = 0.6f;
    public LayerMask enemyLayer;

    [Header("References (set in prefab)")]
    public Transform halfLeft;
    public Transform halfRight;
    public VisualEffect beamVfx;
    public Color beamColor = Color.cyan;
    public LineRenderer line;

    // internal positions captured at start
    private Vector3 leftOpenPos;
    private Vector3 rightOpenPos;
    private Vector3 leftClosedPos = new Vector3(-0.25f, 0f, 0f);
    private Vector3 rightClosedPos = new Vector3(0.25f, 0f, 0f);

    private Coroutine activeRoutine;

    // track VFX play state because VisualEffect has no 'isPlaying' property
    private bool vfxIsPlaying = false;

    private void Start()
    {
        // capture open positions if not explicitly set
        if (halfLeft != null) leftOpenPos = halfLeft.localPosition;
        if (halfRight != null) rightOpenPos = halfRight.localPosition;
    }

    private void Update()
    {
        ActivateAbility();
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

        // ensure halves exist
        if (halfLeft == null || halfRight == null)
        {
            Debug.LogWarning("LazerBOOM: halves not assigned (halfLeft/halfRight)");
            yield break;
        }

        
        float t = 0f;
        Vector3 sL = halfLeft.localPosition;
        Vector3 sR = halfRight.localPosition;
        while (t < connectTime)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / connectTime);
            halfLeft.localPosition = Vector3.Lerp(sL, leftClosedPos, p);
            halfRight.localPosition = Vector3.Lerp(sR, rightClosedPos, p);
            // update VFX to follow halves during connect (2D: keep Z at 0)
            if (beamVfx != null)
            {
                UpdateVFX(halfLeft.position, halfRight.position);
            }
            yield return null;
        }

        if (line != null) { line.enabled = true; line.positionCount = 2; }
       
        float activeElapsed = 0f;
        while (activeElapsed < stayDuration)
        {
            activeElapsed += Time.deltaTime;


            Vector3 worldLeft = halfLeft.position;
            Vector3 worldRight = halfRight.position;
            Vector2 center = ((Vector2)worldLeft + (Vector2)worldRight) * 0.5f;
            float length = Vector2.Distance(worldLeft, worldRight);
            Vector2 size = new Vector2(Mathf.Max(0.05f, length), beamWidth);

            // area damage (keeps original behavior)
            Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f, enemyLayer);
            foreach (var c in hits)
            {
                if (c == null) continue;
                var eh = c.GetComponent<EnemyHealth>();
                if (eh != null) eh.TakeDamage(damagePerSecond * Time.deltaTime);
            }

            // precise hit detection along beam to report a hit point to VFX (so you can show impact)
            Vector3 dir = (worldRight - worldLeft);
            float dist = dir.magnitude;
            Vector3 hitPoint = worldRight;
            if (dist > 0.001f)
            {
                dir /= dist; // normalize
                RaycastHit2D rhit = Physics2D.Raycast(worldLeft, dir, dist, enemyLayer);
                if (rhit.collider != null)
                {
                    hitPoint = rhit.point;
                    // optional: also apply direct hit damage to the first collider (in addition to area damage)
                    var eh = rhit.collider.GetComponent<EnemyHealth>();
                    if (eh != null) eh.TakeDamage(damagePerSecond * Time.deltaTime);
                }
            }

            // update VFX while beam is active (use helper to centralize changes)

            if (beamVfx != null)
            {
                UpdateVFX(worldLeft, worldRight);
                // provide hit point to the VFX graph; expose a Vector3 property named 'HitPosition' in your VFX
                hitPoint.z = 0f; // ensure 2D
                beamVfx.SetVector3("HitPosition", hitPoint);
            }

            if (line != null)
            {
                // ensure line positions are on Z=0 for 2D
                line.SetPosition(0, new Vector3(worldLeft.x, worldLeft.y, 0f));
                line.SetPosition(1, new Vector3(worldRight.x, worldRight.y, 0f));
                // optional: animate width or color:
                // line.widthMultiplier = Mathf.Lerp(0.1f, 1f, Mathf.PingPong(Time.time,1f));
            }

            yield return null;
        }

        // animate halves opening back to original positions
        t = 0f;
        Vector3 endL = halfLeft.localPosition;
        Vector3 endR = halfRight.localPosition;
        while (t < connectTime)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / connectTime);
            halfLeft.localPosition = Vector3.Lerp(endL, leftOpenPos, p);
            halfRight.localPosition = Vector3.Lerp(endR, rightOpenPos, p);
            if (beamVfx != null)
            {
                UpdateVFX(halfLeft.position, halfRight.position);
            }
            yield return null;
        }

        // stop VFX when fully closed/opened
        StopVFX();
        if (line != null) line.enabled = false;

        activeRoutine = null;
    }

    // visualize the beam area in editor when selected
    private void OnDrawGizmosSelected()
    {
        if (halfLeft == null || halfRight == null) return;
        Gizmos.color = Color.cyan;
        Vector3 a = halfLeft.position;
        Vector3 b = halfRight.position;
        Vector3 center = (a + b) * 0.5f;
        float len = Vector3.Distance(a, b);
        Gizmos.DrawWireCube(center, new Vector3(len, beamWidth, 0.1f));
    }

    // VFX helpers
    private void UpdateVFX(Vector3 start, Vector3 end)
    {
        if (beamVfx == null) return;

        start.z = 0f;
        end.z = 0f;

        if (beamVfx.HasVector3("StartPosition")) beamVfx.SetVector3("StartPosition", start);
        if (beamVfx.HasVector3("EndPosition"))   beamVfx.SetVector3("EndPosition", end);

        if (beamVfx.HasVector3("PositionA")) beamVfx.SetVector3("PositionA", start);
        if (beamVfx.HasVector3("PositionB")) beamVfx.SetVector3("PositionB", end);
        if (beamVfx.HasVector3("SourcePosition")) beamVfx.SetVector3("SourcePosition", start);
        if (beamVfx.HasVector3("TargetPosition")) beamVfx.SetVector3("TargetPosition", end);

        if (beamVfx.HasFloat("BeamWidth")) beamVfx.SetFloat("BeamWidth", beamWidth);
        if (beamVfx.HasFloat("Width"))     beamVfx.SetFloat("Width", beamWidth);

        if (beamVfx.HasVector4("BeamColor")) beamVfx.SetVector4("BeamColor", (Vector4)beamColor);
        if (beamVfx.HasVector4("Color"))     beamVfx.SetVector4("Color", (Vector4)beamColor);

        if (!vfxIsPlaying) { beamVfx.Play(); vfxIsPlaying = true; }
    }

    private void StopVFX()
    {
        if (beamVfx == null) return;
        beamVfx.Stop();
        vfxIsPlaying = false;
    }
}
