using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStats", menuName = "Scriptable Objects/EnemyStats")]
public class EnemyStats : ScriptableObject
{
    public int maxHealth;
    public int speed;
    public int atkDamage;
    public int resistance;
    public int atkSpeed;
    public int dropAmount;
}
