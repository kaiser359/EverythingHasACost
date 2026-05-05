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

        var rb = p.GetComponent<Rigidbody2D>();
        var col = p.GetComponent<Collider2D>();

        if (col != null)
        {
            secondcol.enabled = true;
            col.enabled = false;
            
        }

        Vector2 target = (Vector2)p.transform.position + direction.normalized * distance;

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;

            if (rb != null)
            {
              
                float dashSpeed = distance / duration;
                rb.linearVelocity = direction.normalized * dashSpeed ;
            }
            yield return null;
        }
        if (col != null)
        {
            col.enabled = true;
            secondcol.enabled = false;
        }
            yield return null;
        }

        
    }

