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
    public float stayDuration = 3f; // how long beam stays connected
    public float damagePerSecond = 20f;
    public float beamWidth = 0.6f;
    public float maxRange = 10f; // max beam distance
    public LayerMask enemyLayer;

    [Header("References (set in prefab)")]
    public Transform beamOrigin; // optional: if null uses this.transform.position
    public VisualEffect beamVfx;
    public Color beamColor = Color.cyan;
    public LineRenderer line;

    private Coroutine activeRoutine;

    // track VFX play state because VisualEffect has no 'isPlaying' property
    private bool vfxIsPlaying = false;

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

        // make sure there's a line renderer or VFX to show something, but still allow behavior without them
        if (line != null) { line.enabled = true; line.positionCount = 2; }

        float activeElapsed = 0f;
        while (activeElapsed < stayDuration)
        {
            activeElapsed += Time.deltaTime;

            // determine beam start point
            Vector3 start = (beamOrigin != null) ? beamOrigin.position : transform.position;

            // determine mouse world position (2D)
            Vector3 mouseWorld = start;
            Camera cam = Camera.main;
            if (cam != null)
            {
                Vector3 mp = cam.ScreenToWorldPoint(Input.mousePosition);
                mp.z = start.z; // keep same z-plane as start
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

            Vector3 end = start + dir * maxRange;

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
            Vector3 hitPoint = GetHitPosition(start, dir, maxRange, out rhit);
            if (rhit.collider != null)
            {
                var eh = rhit.collider.GetComponent<EnemyHealth>();
                if (eh != null) eh.TakeDamage(damagePerSecond * Time.deltaTime);
            }

            // update VFX while beam is active
            if (beamVfx != null)
            {
                UpdateVFX(start, end);
                hitPoint.z = 0f; // ensure 2D
                beamVfx.SetVector3("HitPosition", hitPoint);
            }

            if (line != null)
            {
                line.SetPosition(0, new Vector3(start.x, start.y, 0f));
                line.SetPosition(1, new Vector3(end.x, end.y, 0f));
            }

            yield return null;
        }

        StopVFX();
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

    private Vector3 GetHitPosition(Vector3 start, Vector3 dir, float range, out RaycastHit2D hit)
    {
        hit = Physics2D.Raycast(start, dir, range, enemyLayer);
        return (hit.collider != null) ? hit.point : start + dir * range;
    }
}
