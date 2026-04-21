using UnityEngine;

public class BulletScript : MonoBehaviour
{
    [SerializeField]private float timeAlive = 5;
    public EnemyStats stats;
    public Money money;
    public Levels level;
   // public PlayerHealthTEST playerHealth;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
      
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
            var playerHealth = collision.GetComponent<HealthBar>();
            playerHealth.TakeDamage(stats.atkDamage + (money.money / 100) + (level.levelNumber * 10));
            //money.money -= stats.atkDamage + (money.money/100) + (level.levelNumber*10);
            Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);

            }
    }


}
