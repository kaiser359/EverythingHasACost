using UnityEngine;

public class MeeleeDamage : MonoBehaviour
{
    public int damageAmount = 10;
    public Money money;
    public float knockbackForce = 5f; // impulse force applied to player on hit

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (money != null)
                money.money -= damageAmount + (money.money / 100);

            var rb = collision.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 kbDir = (collision.transform.position - transform.position).normalized;
                rb.AddForce(kbDir * knockbackForce, ForceMode2D.Impulse);
            }
        }
    }
}
