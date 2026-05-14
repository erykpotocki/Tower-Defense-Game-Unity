using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Ageward/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Basic")]
    public string weaponName;
    public Sprite weaponSprite;

    [Header("Base Stats")]
    [Min(0)] public float baseDamage = 5f;
    [Min(0.01f)] public float attackInterval = 1.5f;

    [Header("Future Stats")]
    [Range(0f, 100f)] public float critChancePercent = 0f;
    [Min(1)] public int shotsPerAttack = 1;
    [Range(0f, 100f)] public float knockbackChancePercent = 0f;

    [Header("Optional")]
    public Sprite projectileSprite;
}