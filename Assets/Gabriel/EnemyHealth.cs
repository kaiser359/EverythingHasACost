using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class EnemyHealth : MonoBehaviour
{
    
    [SerializeField]private float currentHealth;
    public float timer = 0f;
    public EnemyStats stats;
    public float maxHealth;
    public Money money;
    void Start()
    {
        maxHealth = stats.maxHealth;
        currentHealth = maxHealth;
    }

    void Awake()
    {
        // stats = GameObject.FindWithTag("Player").GetComponent<PlayerStats>();
    
    }


    


    private void Update()
    {
        if (timer > 0f)
        {
            timer -= Time.deltaTime;
        }
        if (timer <= 0f)
        {
           // animator.SetBool("IsHurt", false);
          
        }


    }
    public void TakeDamage(float damage)
    {
       // animator.SetBool("IsHurt", true);
        timer = 0.2f;
       // float randomValue = UnityEngine.Random.Range(0.00f,1f); for crit damage
        currentHealth -= damage;
        
        if (currentHealth <= 0)
        {
        Die();
        }
    }

   
    private void Die()
    {
       
        money.money += stats.dropAmount;
        Destroy(gameObject);
    }


}
