using UnityEngine;

public class MainGun : MonoBehaviour
{
    public GlobalPlayerInfo gS;
    private float cooldownInstance = 0f;
    public GameObject firePos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gS = FindAnyObjectByType<GlobalPlayerInfo>();
    }

    // Update is called once per frame
    void Update()
    {
        cooldownInstance -= Time.deltaTime;
    }

    public void Attack() { 
        if (cooldownInstance > 0f) return;
        Instantiate(gS.bulletPrefab, firePos.transform.position, firePos.transform.rotation);
        cooldownInstance = gS.attackCooldown;
    }
}
