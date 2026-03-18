using UnityEngine;

public class FireballProjectile : MonoBehaviour
{
    public float damage = 10f;
    public float speed = 8f;
    public float lifetime = 4f;
    public string enemyTag = "Enemy";

    private Vector2 velocity;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += (Vector3)velocity * Time.deltaTime;
    }

    public void Init(Vector2 dir, float spd, float dmg)
    {
        velocity = dir.normalized * spd;
        speed = spd;
        damage = dmg;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(enemyTag))
        {
            var eh = other.GetComponent<EnemyHealth>();
            if (eh != null) eh.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
