using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    private const string CurrentStageKey = "CurrentStage";

    [Header("Enemy Data")]
    [SerializeField] private EnemyData enemyData;

    [Header("Stage Scaling")]
    [SerializeField] private float hpIncreasePerStage = 3f;

    [Header("Gold Reward")]
    [SerializeField] private bool isBossForReward = false;

    [Header("Damage Protection Before Screen")]
    [SerializeField] private bool blockDamageUntilNearScreen = true;
    [SerializeField] private float canTakeDamageAtX = 8.8f;

    private float currentHealth;
    private bool isDead;
    private GoldManager goldManager;

    private void Start()
    {
        int currentStage = PlayerPrefs.GetInt(CurrentStageKey, 1);
        currentStage = Mathf.Max(currentStage, 1);

        if (enemyData != null)
        {
            float bonusHealth = (currentStage - 1) * hpIncreasePerStage;
            currentHealth = enemyData.maxHealth + bonusHealth;
        }

        goldManager = FindAnyObjectByType<GoldManager>();
    }

    public bool TakeDamage(float damage)
    {
        if (isDead)
            return false;

        if (!CanTakeDamageNow())
            return false;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        if (currentHealth <= 0f)
        {
            Die();
        }

        return true;
    }

    private bool CanTakeDamageNow()
    {
        if (!blockDamageUntilNearScreen)
            return true;

        return transform.position.x <= canTakeDamageAtX;
    }

    private void Die()
    {
        isDead = true;

        if (goldManager != null)
        {
            if (isBossForReward)
                goldManager.AddBossKillGold();
            else
                goldManager.AddEnemyKillGold();
        }

        Destroy(gameObject);
    }
}