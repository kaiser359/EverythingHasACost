using UnityEngine;

public class PlayerStats : MonoBehaviour
{
   
    public float baseDamage = 1f;
    public float baseMaxHealth = 100f;
    public float baseMoveSpeed = 5f;
    public float baseFireRate = 1f;
    public float baseCritChance = 0f;
    public float baseLootChance = 0.05f;
    public float dashCooldown = 1f;
    private float shield = 0f;
    public float baseCritDamage = 2f;
    public float baseMikuBean;
    public float baselifesteal = 0.1f;
    public float necromancyAmount = 5f;
    public Money money;


    [Header("Meteor")]
    public float meteorDamage = 50f;
    public float meteorCooldown = 20f;
    public float meteorRadius = 2.5f;

    
    public void SetDamage(float value) => baseDamage = value;
    public void SetMaxHealth(float value) => baseMaxHealth = value;
    public void SetMoveSpeed(float value) => baseMoveSpeed = value;
    public void SetFireRate(float value) => baseFireRate = value;
    public void SetCritChance(float value) => baseCritChance = value;
    public void SetLootChance(float value) => baseLootChance = value;
    public void SetDashCooldown(float value) => dashCooldown = value;
    public void AddShield(float amount) { shield += amount; }
    public void SetCritDamage(float value) => baseCritDamage = value;

    public void SetMikuBean(float value) => baseMikuBean = value;

    public void setlifesteal(float value) => baselifesteal = value;

    public void SetNecromancyAmount(float value) => necromancyAmount = value;



    
    public void SetMeteorDamage(float damage)
    {
        meteorDamage = damage;
       
    }

    public void SetMeteorCooldown(float cd)
    {
        meteorCooldown = cd;
       
    }

    public void SetMeteorRadius(float r)
    {
        meteorRadius = r;
       
    }

    // Apply a power by AbilityType - convenience helper so external systems can delegate effect application
    public void ApplyPower(AbilityType type, float value, int level = 1, int addedLevels = 1)
    {
        switch (type)
        {
            case AbilityType.TheFool:
                // no direct player stat change; handled by PowerSystem (decoy spawn)
                break;
            case AbilityType.TheMagician:
                // teleport player forward by `value`
                TeleportForward(value);
                break;
            case AbilityType.TheHighPriestess:
                // immediate heal by `value` (AoE handled by PowerSystem)
                Heal(value);
                break;
            case AbilityType.TheEmpress:
                // temporary damage boost based on value for a short duration
                ApplyTemporaryDamageMultiplier(1f + value, 5f);
                break;
            case AbilityType.TheEmperor:
                // add shield
                AddShield(value * addedLevels);
                break;
            case AbilityType.TheHierophant:
                // small temporary damage buff or visual effect
                ApplyTemporaryDamageMultiplier(1f + value * 0.5f, 2f + level);
                break;
            case AbilityType.TheLovers:
                // heal player
                Heal(value);
                break;
            case AbilityType.TheChariot:
                // temporary speed burst
                ApplyTemporaryMoveSpeedMultiplier(1f + value, 3f + level);
                break;
            default:
                // other powers are handled elsewhere
                break;
        }
    }

    // Heal the player (assumes this component is on the player GameObject)
    public void Heal(float amount)
    {
        money.money += (int)amount;
    }

    // Apply a temporary damage multiplier for `duration` seconds
    public void ApplyTemporaryDamageMultiplier(float multiplier, float duration)
    {
        if (_tempDamageCoroutine != null) StopCoroutine(_tempDamageCoroutine);
        _tempDamageCoroutine = StartCoroutine(TempDamageCoroutine(multiplier, duration));
    }

    private System.Collections.IEnumerator TempDamageCoroutine(float multiplier, float duration)
    {
        float original = baseDamage;
        baseDamage = original * multiplier;
        yield return new WaitForSeconds(duration);
        baseDamage = original;
    }

    // Temporary move speed multiplier
    public void ApplyTemporaryMoveSpeedMultiplier(float multiplier, float duration)
    {
        if (_tempMoveCoroutine != null) StopCoroutine(_tempMoveCoroutine);
        _tempMoveCoroutine = StartCoroutine(TempMoveCoroutine(multiplier, duration));
    }

    private System.Collections.IEnumerator TempMoveCoroutine(float multiplier, float duration)
    {
        float original = baseMoveSpeed;
        baseMoveSpeed = original * multiplier;
        yield return new WaitForSeconds(duration);
        baseMoveSpeed = original;
        _tempMoveCoroutine = null;
    }

    // Teleport the player to a specific world position
    public void TeleportTo(Vector3 position)
    {
        transform.position = position;
    }

    // Teleport the player forward by distance along facing direction
    public void TeleportForward(float distance)
    {
        Vector3 dir = transform.right; // assumes right is facing; adapt if you use a different facing system
        transform.position += dir * distance;
    }

    // Returns player's facing direction (world space)
    public Vector2 GetFacingDirection()
    {
        return transform.right;
    }

    private Coroutine _tempDamageCoroutine;
    private Coroutine _tempMoveCoroutine;
}
