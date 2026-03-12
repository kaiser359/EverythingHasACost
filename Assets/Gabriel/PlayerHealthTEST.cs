using UnityEngine;

public class PlayerHealthTEST : MonoBehaviour
{
    private int maxHealth = 100;
    public int health;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(int damage)
    {
       health -= damage;
        Debug.Log("Player took " + damage + " damage!");
    }
    public void dead()
    {
        //nothing
    }
}
