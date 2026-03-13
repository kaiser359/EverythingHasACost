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

    public void Attack()
    {
        if (cooldownInstance > 0f) return;

        // Instantiate a bullet at every child transform that has the tag "AttackPoint"
        foreach (Transform child in GetComponentsInChildren<Transform>(includeInactive: false))
        {
            if (child == transform) continue;
            if (child.CompareTag("AttackPoint"))
            {
                Instantiate(gS.bulletPrefab, child.position, child.rotation);
            }
        }

        cooldownInstance = gS.attackCooldown;
    }
}
