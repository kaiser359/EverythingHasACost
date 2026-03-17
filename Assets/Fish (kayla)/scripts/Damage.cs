using UnityEngine;

public class Damage : MonoBehaviour
{
    private HealthBar healthBar;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        healthBar = FindFirstObjectByType<HealthBar>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        healthBar.TakeDamage(10);
    }
}
