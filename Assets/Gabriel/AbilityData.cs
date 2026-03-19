using UnityEngine;

[System.Serializable]
public class AbilityData
{
    public string abilityName;
    [TextArea] public string description;
    public Sprite icon;

    public AbilityType abilityType;

    // Primary numeric tuning: base value and per-level increment
    public float baseValue = 1f;         // e.g., base damage for Meteor
    public float valuePerLevel = 1f;     // e.g., damage added per level (or multiplier when isMultiplier)
    public bool isMultiplier = false;    // if true, value scales multiplicatively: base * valuePerLevel^(level-1)

    // Cooldown tuning (secondary value used by abilities like Meteor)
    public float cooldownBase = 20f;              // starting cooldown in seconds
    public float cooldownReductionPerLevel = 1f;  // amount reduced per level (linear)
    public bool cooldownIsMultiplier = false;     // if true, cooldown multiplies by valuePerLevel^(level-1) instead

    // 0 = no max
    public int maxLevel = 0;

    // returns the primary ability value (damage, speed, etc.) at `level`
    public float GetValue(int level)
    {
        if (level <= 0) level = 1;
        if (isMultiplier)
            return baseValue * Mathf.Pow(valuePerLevel, level - 1); // exponential scaling
        else
            return baseValue + valuePerLevel * (level - 1); // linear scaling
    }

    // returns primary value modified by a rarity multiplier (rarity: 0..1 or 1..N depending on design)
    public float GetValueWithRarity(int level, float rarityMultiplier)
    {
        float val = GetValue(level);
        return val * rarityMultiplier;
    }

    // returns cooldown at `level`. Clamps to a small positive minimum.
    public float GetCooldown(int level)
    {
        if (level <= 0) level = 1;
        float cd;
        if (cooldownIsMultiplier)
        {
            cd = cooldownBase * Mathf.Pow(valuePerLevel, level - 1);
        }
        else
        {
            cd = cooldownBase - cooldownReductionPerLevel * (level - 1);
        }

        // safety clamp
        return Mathf.Max(0.1f, cd);
    }
}
