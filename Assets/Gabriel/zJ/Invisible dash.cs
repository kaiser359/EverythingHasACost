using System.Collections;
using UnityEngine;

public class Invisibledash : MonoBehaviour
{
    public float dashDistance = 7f;
    public float dashDuration = 0.18f;
    public float cooldown = 5f;
    public StarRatings star;
    float _cooldownTimer = 0f;

    void Update()
    {
        //if (_cooldownTimer > 0f) _cooldownTimer -= Time.deltaTime;
        //if (Input.GetKey(KeyCode.K)){
        //    ActivateAbility();
        //}
    }
    private void Start()
    {
        cooldown -= (star.StartRating/100f);
    }
    // Public activation entrypoint
    public void ActivateAbility()
    {
        if (_cooldownTimer > 0f) return;
        _cooldownTimer = cooldown;

        // determine dash direction from player movement if possible
        var player = GameObject.FindWithTag("Player");
        if (player == null) return;

        var prb = player.GetComponent<Rigidbody2D>();
        Vector2 dashDir;
        if (prb != null && prb.linearVelocity.sqrMagnitude > 0.0001f)
            dashDir = prb.linearVelocity.normalized;
        else
            dashDir = (Vector2)player.transform.right;

        StartCoroutine(PerformDash(dashDir, dashDistance, dashDuration));
    }

    IEnumerator PerformDash(Vector2 direction, float distance, float duration)
    {
        var p = GameObject.FindWithTag("Player");
        if (p == null) yield break;

        var rb = p.GetComponent<Rigidbody2D>();
        var col = p.GetComponent<Collider2D>();

        if (col != null) col.enabled = false;

        Vector2 target = (Vector2)p.transform.position + direction.normalized * distance;

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            if (rb != null)
            {
                var toTarget = (target - (Vector2)p.transform.position);
                float remaining = toTarget.magnitude;
                if (remaining > 0.001f)
                    rb.AddForce(toTarget.normalized * remaining * 100f);
            }
            else
            {
                // fallback: move transform directly
                p.transform.position = Vector2.MoveTowards(p.transform.position, target, (distance / duration) * Time.deltaTime);
            }

            yield return null;
        }

        if (col != null) col.enabled = true;
    }
}
