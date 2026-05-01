using UnityEngine;

public class FireBallThrower : MonoBehaviour
{
    public GameObject fireballPrefab;
    public StarRatings star;
    public AudioClip fireballSound;
    //= Resources.Load<GameObject>("FireBallBlast");
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    // Update is called once per frame
    void Update()
    {
        // cooldown timer
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
        if (Input.GetKey(KeyCode.K))
        {
            ActivateAbility();
        }
    }
   private void Start()
    {
        abilityCooldown -= star.StartRating;
        
    }
    public void ActivateAbility()
    {
      //  if (cooldownTimer > 0f) return;


        if (fireballPrefab == null)
        {
            Debug.LogError("FireballPrefab not found in Resources folder!");
            return;
        }

        FindAnyObjectByType<AudioSource>().PlayOneShot(fireballSound);

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

        // spawn a single fireball toward the aim direction
        float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
        Vector2 dir = aimDir.normalized;

        var obj = Instantiate(fireballPrefab, spawnPos, Quaternion.Euler(0f, 0f, angle));
        var rb = obj.GetComponent<Rigidbody2D>();
        if (rb == null) rb = obj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearVelocity = dir * projectileSpeed;

        cooldownTimer = abilityCooldown;
    }

    [Header("Ability")]
    public float projectileSpeed = 8f;
    public float abilityCooldown = 8f;
    private float cooldownTimer = 0f;
    [Header("Wave")]
    public int waveCount = 5; // number of fireballs in the wave
    public float coneAngle = 30f; // total cone angle in degrees
}
