using System.Collections;
using UnityEngine;

public class firaball : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 6f;
    public float lifeTime = 8f;

    [Header("Explosion")]
    public int damage = 20;
    public float aoeRadius = 1.6f;
    public LayerMask enemyLayer; 

    [Header("Chain")]
    public GameObject smallFireballPrefab; 
    public int numSmallFireballs = 8;
    public float smallSpeed = 4f;
    public float spawnInterval = 0.03f; // spawn slowly then they KABOOM
    public int remainingChains = 2; 
    public bool spawnInAllDirections = true; 
    public float spreadVariance = 15f;
    public float spawnRadiusOffset = 0f; 

    [Header("Physics")]
    [Range(0f, 1f)] public float bounceDamping = 0.95f;

    Rigidbody2D rb;
    float timer;
    PhysicsMaterial2D physMat;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
        rb.freezeRotation = true;
        
        physMat = new PhysicsMaterial2D("firaball_mat") { friction = 0f, bounciness = 1f };


        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            var cc = gameObject.AddComponent<CircleCollider2D>();
            cc.isTrigger = false;
            cc.sharedMaterial = physMat;
        }
        else
        {
           
            if (col.sharedMaterial == null)
                col.sharedMaterial = physMat;
            else
            {
                col.sharedMaterial.friction = 0f;
                col.sharedMaterial.bounciness = 1f;
            }
        }
    }

    void Start()
    {
        timer = lifeTime;
        if (rb.linearVelocity.sqrMagnitude < 0.001f)
            rb.linearVelocity = transform.right * speed;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
            Destroy(gameObject);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider == null) return;

        if (col.collider.CompareTag("Enemy"))
        {
            TryExplode();
            return;
        }


        Destroy(gameObject);
    }

    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;

        if (other.CompareTag("Enemy"))
        {
            TryExplode();
            return;
        }

       
        Destroy(gameObject);
    }

    void TryExplode()
    {
      
        Collider2D[] hits;
        if (enemyLayer.value != 0)
            hits = Physics2D.OverlapCircleAll(transform.position, aoeRadius, enemyLayer);
        else
            hits = Physics2D.OverlapCircleAll(transform.position, aoeRadius);

        int enemyCount = 0;
        foreach (var c in hits)
        {
            if (c == null) continue;
            if (c.CompareTag("Enemy") || (enemyLayer.value != 0))
            {
                enemyCount++;
            }
        }

        if (enemyCount == 0)
        {
         
            return;
        }

       
        foreach (var c in hits)
        {
            if (c == null) continue;
            if (!c.CompareTag("Enemy")) continue;
            var eh = c.GetComponent<EnemyHealth>();
            if (eh != null) eh.TakeDamage(damage);
        }


        if (remainingChains > 0 && smallFireballPrefab != null)
        {

            StartCoroutine(ExplodeSequence(transform.position));
            return;
        }

      
        Destroy(gameObject);
    }

    IEnumerator ExplodeSequence(Vector3 explosionPos)
    {
      
        yield return StartCoroutine(SpawnChain(explosionPos));

      
        Destroy(gameObject);
    }

    IEnumerator SpawnChain(Vector3 explosionPos)
    {
        int toSpawn = Mathf.Max(1, numSmallFireballs);
        int childChains = Mathf.Max(0, remainingChains - 1);

        for (int i = 0; i < toSpawn; i++)
        {
            float baseAngle = spawnInAllDirections ? (360f / toSpawn) * i : Random.Range(0f, 360f);
            float angle = baseAngle + Random.Range(-spreadVariance, spreadVariance);
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            Vector3 spawnPos = explosionPos + (Vector3)(dir * spawnRadiusOffset);
            GameObject go = Instantiate(smallFireballPrefab, spawnPos, Quaternion.identity);
            var fb = go.GetComponent<firaball>();
            if (fb != null)
            {
                fb.remainingChains = childChains;
                
                fb.damage = Mathf.Max(1, Mathf.RoundToInt(damage * 0.5f));
                fb.aoeRadius = aoeRadius * 0.6f;
                fb.numSmallFireballs = Mathf.Max(3, Mathf.RoundToInt(numSmallFireballs * 0.5f));
            }

            var r = go.GetComponent<Rigidbody2D>();
            if (r == null)
            {
                r = go.AddComponent<Rigidbody2D>();
                r.bodyType = RigidbodyType2D.Dynamic;
            }
            r.gravityScale = 0f;

           
            Collider2D childCol = go.GetComponent<Collider2D>();
            if (childCol == null)
            {
                var cc = go.AddComponent<CircleCollider2D>();
                cc.isTrigger = false;
            }

            r.linearVelocity = dir * smallSpeed;

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aoeRadius);
    }
}
