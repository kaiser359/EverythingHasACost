using UnityEngine;

public class PlayerBullets : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GlobalPlayerInfo gS;
    private float timer = 0f;
    public Collider2D col;
    public SpriteRenderer sr;
    void Start()
    {
        gS = FindAnyObjectByType<GlobalPlayerInfo>();
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        timer = gS.attackCooldown + 0.1f;
        
        if (rb != null)
        {
            rb.linearVelocity = -transform.right * gS.bulletSpeed; // Adjust the speed as needed
        }        
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f) // Destroy the bullet after 5 seconds to prevent memory leaks
        {
            Destroy(gameObject);
        }

    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Bullet collided with: " + collision.gameObject.name);
        collision.gameObject.GetComponent<EnemyHealth>()?.TakeDamage(15f); // Assuming the target has a Health component}
        col.enabled = false; // Deactivate the bullet on collision
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.3f); // Make the bullet invisible
    }
}
