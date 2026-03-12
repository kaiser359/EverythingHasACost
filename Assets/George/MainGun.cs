using UnityEngine;

public class MainGun : MonoBehaviour
{
    public GlobalPlayerInfo gS;
    private float cooldownInstance = 0f;
    public 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        cooldownInstance -= Time.deltaTime;
    }

    public void Attack() { 
        Instantiate(gS.bulletPrefab, firePos, transform.rotation);
        cooldownInstance = gS.attackCooldown;
    }
}
