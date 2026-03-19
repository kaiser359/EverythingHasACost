using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PowerSystem : MonoBehaviour
{
    public static PowerSystem Instance;

    // 22 powers
    [Header("Powers")]
    public List<AbilityData> powers = new List<AbilityData>(22);

    // current levels for each power
    public List<int> powerLevels = new List<int>();

    [Header("Game hooks (optional)")]
    public PlayerStats playerStats;
    [Header("Power Prefabs")]
    public GameObject decoyPrefab;
    public GameObject aoePrefab;
    public GameObject fireballPrefab;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        EnsureSize();
    }

    private void EnsureSize()
    {
        if (powerLevels == null) powerLevels = new List<int>();
        while (powerLevels.Count < powers.Count) powerLevels.Add(0);
        if (powerLevels.Count > powers.Count)
            powerLevels.RemoveRange(powers.Count, powerLevels.Count - powers.Count);
    }

    // Apply a power by index; rarityMultiplier allows external rarity to boost effect
    public void ApplyPower(int powerIndex, int addedLevels, float rarityMultiplier = 1f)
    {
        if (powerIndex < 0 || powerIndex >= powers.Count)
        {
            Debug.LogWarning($"PowerSystem: invalid power index {powerIndex}");
            return;
        }

        EnsureSize();
        int current = powerLevels[powerIndex];
        int newLevel = current + addedLevels;
        int max = powers[powerIndex].maxLevel;
        if (max > 0 && newLevel > max) newLevel = max;
        powerLevels[powerIndex] = newLevel;

        // compute value with rarity
        var data = powers[powerIndex];
        float value = data.GetValueWithRarity(newLevel, rarityMultiplier);

        // apply effect to playerStats if available
        ApplyEffect(powerIndex, data.abilityType, value, newLevel, addedLevels);
    }

    // Get computed power value (for UI or other scripts) with optional rarity
    public float GetPowerValue(int powerIndex, float rarityMultiplier = 1f)
    {
        if (powerIndex < 0 || powerIndex >= powers.Count) return 0f;
        int level = powerLevels.Count > powerIndex ? powerLevels[powerIndex] : 0;
        return powers[powerIndex].GetValueWithRarity(level, rarityMultiplier);
    }

    private void ApplyEffect(int index, AbilityType type, float computedValue, int newLevel, int addedLevels)
    {
        // common mapping: indices correspond to powers list; switch on AbilityType for behaviour
        switch (type)
        {
            case AbilityType.TheFool:
                // spawn a decoy/clone at player position
                if (decoyPrefab != null)
                {
                    var player = GameObject.FindGameObjectWithTag("Player");
                    if (player != null)
                    {
                        var go = GameObject.Instantiate(decoyPrefab, player.transform.position + (Vector3)Vector2.right * 0.5f, Quaternion.identity);
                        var dec = go.GetComponent<Decoy>();
                        if (dec != null) dec.lifetime = computedValue;
                    }
                }
                else
                {
                    Debug.Log($"Power: TheFool applied (no prefab) value {computedValue}");
                }
                break;
            case AbilityType.TheMagician:
                // teleport the player forward by computedValue units
                var ply = GameObject.FindGameObjectWithTag("Player");
                if (ply != null)
                {
                    Vector3 dashDir = ply.transform.right; // assuming player faces right; better to use player facing if available
                    ply.transform.position = ply.transform.position + dashDir * computedValue;
                }
                Debug.Log($"Power: TheMagician teleported player by {computedValue}");
                break;
            case AbilityType.TheHighPriestess:
                if (aoePrefab != null)
                {
                    var player = GameObject.FindGameObjectWithTag("Player");
                    if (player != null)
                    {
                        var go = GameObject.Instantiate(aoePrefab, player.transform.position, Quaternion.identity);
                        var aoe = go.GetComponent<AoEEffect>();
                        if (aoe != null)
                        {
                            aoe.duration = computedValue;
                            aoe.damagePerTick = computedValue * 0.5f;
                            aoe.healPerTick = computedValue * 0.5f;
                            aoe.radius = 2f + newLevel * 0.5f;
                        }
                    }
                }
                break;
            case AbilityType.TheEmpress:
                // spawn an AoE that damages and could apply root; reuse AoEEffect for damage, root not implemented
                if (aoePrefab != null)
                {
                    var player = GameObject.FindGameObjectWithTag("Player");
                    if (player != null)
                    {
                        var go = GameObject.Instantiate(aoePrefab, player.transform.position, Quaternion.identity);
                        var aoe = go.GetComponent<AoEEffect>();
                        if (aoe != null)
                        {
                            aoe.duration = computedValue;
                            aoe.damagePerTick = computedValue;
                            aoe.healPerTick = 0f;
                            aoe.radius = 2f + newLevel * 0.5f;
                        }
                    }
                }
                break;
            case AbilityType.TheEmperor:
                // create shield that absorbs damage
                if (playerStats != null) playerStats.AddShield(computedValue * addedLevels);
                break;
            case AbilityType.TheHierophant:
                // simple line strike: raycast forward from player and damage enemies
                var pl = GameObject.FindGameObjectWithTag("Player");
                if (pl != null)
                {
                    Vector2 origin = pl.transform.position;
                    Vector2 dir = pl.transform.right;
                    RaycastHit2D[] hits = Physics2D.RaycastAll(origin, dir, computedValue * 2f);
                    foreach (var h in hits)
                    {
                        if (h.collider != null && h.collider.CompareTag("Enemy"))
                        {
                            var eh = h.collider.GetComponent<EnemyHealth>();
                            if (eh != null) eh.TakeDamage(computedValue);
                        }
                    }
                }
                break;
            case AbilityType.TheLovers:
                // steal enemy ability - placeholder: heal player by value and reduce nearest enemy damage
                var pp = GameObject.FindGameObjectWithTag("Player");
                if (pp != null)
                {
                    var ps = pp.GetComponent<PlayerStats>();
                    if (ps != null) ps.Heal(computedValue);
                }
                // reduce nearest enemy damage stat if available
                var nearest = FindNearestEnemy();
                if (nearest != null)
                {
                    var est = nearest.GetComponent<EnemyStats>();
                    if (est != null) est.atkDamage = Mathf.Max(0, est.atkDamage - (int)computedValue);
                }
                break;
            case AbilityType.TheChariot:
                // spawn multiple fireballs in a cone from player
                var playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null && fireballPrefab != null)
                {
                    // use Vector3 origin so additions with Vector3 are unambiguous
                    Vector3 origin = playerObj.transform.position;
                    int count = 6 + newLevel;
                    float coneAngle = 45f;
                    Vector2 forward = playerObj.transform.right;
                    for (int i = 0; i < count; i++)
                    {
                        float t = (float)i / (count - 1);
                        float angle = Mathf.Lerp(-coneAngle * 0.5f, coneAngle * 0.5f, t) * Mathf.Deg2Rad;
                        Vector2 dir = new Vector2(
                            forward.x * Mathf.Cos(angle) - forward.y * Mathf.Sin(angle),
                            forward.x * Mathf.Sin(angle) + forward.y * Mathf.Cos(angle)
                        ).normalized;
                        var go = GameObject.Instantiate(fireballPrefab, origin + (Vector3)dir * 0.5f, Quaternion.identity);
                        var proj = go.GetComponent<FireballProjectile>();
                        if (proj != null) proj.Init(dir, 6f + newLevel, computedValue);
                    }
                }
                break;
            default:
                Debug.Log($"[PowerSystem] Unhandled ability type {type}");
                break;
        }
    }

    // Moved helper method out of ApplyEffect so it's available to all cases
    private GameObject FindNearestEnemy()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject nearest = null;
        float best = float.MaxValue;
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return null;
        foreach (var e in enemies)
        {
            float d = Vector2.Distance(player.transform.position, e.transform.position);
            if (d < best)
            {
                best = d; nearest = e;
            }
        }
        return nearest;
    }

}

// Usage: PowerSystem.Instance.ApplyPower(index, levelsToAdd, rarityMultiplier);
