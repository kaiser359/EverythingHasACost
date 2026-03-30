using UnityEngine;

public class DamageBuffer : MonoBehaviour
{
    public Money money;
    public GlobalPlayerInfo playerInfo;
    public float cooldown;
    public float duration;
    public GameObject normalBullet;
    public GameObject buffedBullet;
    public float originakmoveSpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        normalBullet = playerInfo.bulletPrefab;
        originakmoveSpeed = playerInfo.moveSpeed;
    }
    // Update is called once per frame
    void Update()
    {
        if (cooldown > 0)
        {
            cooldown -= Time.deltaTime;
        }
        if (duration > 0) { duration -= Time.deltaTime; }

        if (duration <= 0)
        {
            playerInfo.moveSpeed = originakmoveSpeed;
            playerInfo.bulletPrefab = normalBullet;
        }

    }
    public void ActivateAbility()
    {if (cooldown > 0) return;

        playerInfo.moveSpeed += 1f;
        money.money -= 100;
        cooldown = 10;
        duration += 5f;
        playerInfo.bulletPrefab = buffedBullet;
    }
}
