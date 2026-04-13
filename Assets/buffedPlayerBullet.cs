using UnityEngine;

public class buffedPlayerBullet : MonoBehaviour
{
    public int damage = 60;
    public StarRatings star;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        damage += star.StartRating * 10;
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
