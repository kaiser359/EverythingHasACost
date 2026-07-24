using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainGun : MonoBehaviour
{
    public GlobalPlayerInfo gS;
    public AudioClip shoot;
    [SerializeField] private float pitchVariance;
    private float cooldownInstance = 0f;
    private CinemachineImpulseSource impulseSource;

    // new: whether the attack button is currently held
    private bool isFiring = false;

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

        // While the attack button is held, attempt to fire whenever cooldown allows.
        if (isFiring && cooldownInstance <= 0f)
        {
            Attack(); // calls parameterless overload that performs a single shot
        }
    }

    private void AttackSFX()
    {
        AudioSource audioSource = gameObject.GetComponent<AudioSource>();

        audioSource.pitch = Random.Range(1f - pitchVariance, 1f + pitchVariance);
        audioSource.PlayOneShot(shoot);
    }

    // Keep the original parameterless Attack method (fires a single shot if cooldown permits)
    public void Attack()
    {
        if (cooldownInstance > 0f) return;

        AttackSFX();

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

    // New overload for Unity Input System: call this from your Input Action (it receives the callback context)
    // - when the button is pressed, we start firing (isFiring = true)
    // - when the button is released, we stop firing (isFiring = false)
    public void Attack(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            isFiring = true;
        }
        else if (ctx.canceled)
        {
            isFiring = false;
        }
        // we don't call Attack() directly here — Update handles repeated firing while held
    }
}
