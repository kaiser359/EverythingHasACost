using UnityEngine;

public class OutOfBounds : MonoBehaviour
{
    public GameObject player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, Mathf.Infinity);

        if (hit.collider != null && hit.collider.gameObject == player)
        {
            // Player is out of bounds, handle accordingly
            var playerHealth = player.GetComponent<HealthBar>();
            playerHealth.TakeDamage(2);
        }
    }
}
