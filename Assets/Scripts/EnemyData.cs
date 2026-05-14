using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Ageward/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Basic")]
    public string enemyName;
    public Sprite enemySprite;

    [Header("Base Stats")]
    [Min(1)] public float maxHealth = 10f;
    [Min(0)] public float damage = 5f;
    [Min(0.1f)] public float moveSpeed = 1.5f;
}