using System.Collections;
using UnityEngine;

public class Invisibledash : MonoBehaviour
{
    public float dashDistance = 7f;
    public float dashDuration = 0.18f;
    public float cooldown = 5f;
    public StarRatings star;
    public float _cooldownTimer = 0f;
    public Collider2D secondcol;
    public AudioClip dashSound;
    //public Rigidbody2D rb;
    public ParticleSystem particle;

    void Update()
    {
        if (_cooldownTimer > 0f) _cooldownTimer -= Time.deltaTime;
        //if (Input.GetKey(KeyCode.K))
        //{
        //    ActivateAbility();
        //}
    }
    private void Start()
    {
        cooldown -= (star.StartRating/100f);
    }
   
    public void ActivateAbility()
    {
        if (_cooldownTimer > 0f) return;
        _cooldownTimer = cooldown;

       
        var player = GameObject.FindWithTag("Player");
        if (player == null) return;

        var prb = player.GetComponent<Rigidbody2D>();
        Vector2 dashDir;

        FindAnyObjectByType<AudioSource>().PlayOneShot(dashSound);

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

        Rigidbody2D rb = p.GetComponent<Rigidbody2D>();

        // 1. DONT disable the collider here! 
        // If you need to ignore enemies, change the layer instead:
        // p.layer = LayerMask.NameToLayer("DashingPlayer"); 

        // 2. Calculate speed based on your distance/duration
        float dashSpeed = distance / duration;
        rb.linearVelocity = direction.normalized * dashSpeed;

        // 3. Wait for the duration using FixedUpdate for physics consistency
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // 4. Reset velocity and Layer
        rb.linearVelocity = Vector2.zero;
        yield return null;
        }

        
    }

