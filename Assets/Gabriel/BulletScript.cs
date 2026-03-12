using UnityEngine;

public class BulletScript : MonoBehaviour
{
    [SerializeField]private float timeAlive = 5;
    public EnemyStats stats;
    public PlayerHealthTEST playerHealth;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerHealth = GameObject.FindWithTag("Player").GetComponent<PlayerHealthTEST>();
    }

    // Update is called once per frame
    void Update()
    {
        timeAlive -= Time.deltaTime;
        if (timeAlive < 0)
        {
            Destroy(gameObject);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerHealth.health -= stats.atkDamage;
            Destroy(gameObject);
        }
    }


}
