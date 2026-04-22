using Unity.Cinemachine;
using UnityEngine;

public class MainGun : MonoBehaviour
{
    public GlobalPlayerInfo gS;
    private float cooldownInstance = 0f;

    private CinemachineImpulseSource impulseSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gS = FindAnyObjectByType<GlobalPlayerInfo>();

        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    // Update is called once per frame
    void Update()
    {
        cooldownInstance -= Time.deltaTime;
    }

    public void Attack() { 
        if (cooldownInstance > 0f) return;

        GameObject[] attackPoints = GameObject.FindGameObjectsWithTag("AttackPoint");
        foreach (GameObject point in attackPoints)
        {
            Instantiate(gS.bulletPrefab, point.transform.position, point.transform.rotation);
        }
        cooldownInstance = gS.attackCooldown;

        // recoil camera shake
        float radians = gS.aimDir * Mathf.Deg2Rad;
        impulseSource.DefaultVelocity = new Vector3(-Mathf.Cos(radians), -Mathf.Sin(radians), 0);
        impulseSource.GenerateImpulse(0.2f);
    }
}
