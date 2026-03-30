using UnityEngine;

public class foreThebluebulet : MonoBehaviour
{
    public float time = 10f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        time -= Time.deltaTime;
        if (time < 0)
        {
            Destroy(gameObject);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            collision.gameObject.GetComponent<EnemyHealth>()?.TakeDamage(20);
            Destroy(gameObject);
        }
        if (!collision.gameObject.CompareTag("Enemy")) { 
            Destroy(gameObject);
        }
    }
}
