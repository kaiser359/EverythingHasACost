using UnityEngine;

public class PlayerBulletScript : MonoBehaviour
{
    public int damage = 15;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        collision.gameObject.GetComponent<EnemyHealth>()?.TakeDamage(damage);
    }
}
