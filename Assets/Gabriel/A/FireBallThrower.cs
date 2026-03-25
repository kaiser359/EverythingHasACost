using UnityEngine;

public class FireBallThrower : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // cooldown timer
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
    }

    public void ActivateAbility()
    {
        if (cooldownTimer > 0f) return; // still cooling down

        GameObject fireballPrefab = Resources.Load<GameObject>("FireBallBlast");
        if (fireballPrefab == null)
        {
            Debug.LogError("FireballPrefab not found in Resources folder!");
            return;
        }

        // determine aim direction from mouse position (2D)
        Vector3 spawnPos = transform.position;
        Vector3 aimDir = transform.right;
        Camera cam = Camera.main;
        if (cam != null)
        {
            float zDist = transform.position.z - cam.transform.position.z;
            Vector3 mp = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, zDist));
            aimDir = (mp - spawnPos);
            if (aimDir.sqrMagnitude > 0.0001f) aimDir = aimDir.normalized;
            else aimDir = transform.right;
        }

        // instantiate and launch
        var obj = Instantiate(fireballPrefab, spawnPos, Quaternion.Euler(0f, 0f, Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg));
        var rb = obj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = aimDir * projectileSpeed;
        }

        cooldownTimer = abilityCooldown;
    }

    [Header("Ability")]
    public float projectileSpeed = 8f;
    public float abilityCooldown = 1f;
    private float cooldownTimer = 0f;
}
