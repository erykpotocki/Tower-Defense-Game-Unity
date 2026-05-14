using TMPro;
using UnityEngine;

public class ShopStatsSummaryUI : MonoBehaviour
{
    private const string DamageLevelKey = "SlingshotDamageLevel";
    private const string AttackSpeedLevelKey = "SlingshotAttackSpeedLevel";
    private const string CastleHpLevelKey = "CastleHpLevel";
    private const string CurrentStageKey = "CurrentStage";
    private const string ProjectileSizeLevelKey = "ProjectileSizeLevel";
    private const string ProjectileSpeedLevelKey = "ProjectileSpeedLevel";
    private const string CriticalChanceLevelKey = "CriticalChanceLevel";
    private const string KnockbackLevelKey = "KnockbackLevel";
    private const string ProjectileCountLevelKey = "ProjectileCountLevel";

    [Header("UI")]
    [SerializeField] private TMP_Text statPlayerText;
    [SerializeField] private TMP_Text statMobText;

    [Header("Weapon Data")]
    [SerializeField] private WeaponData weaponData;

    [Header("Enemy Data")]
    [SerializeField] private EnemyData enemyData;

    [Header("Base Castle")]
    [SerializeField] private float baseCastleHealth = 100f;
    [SerializeField] private float castleHealthPerLevel = 10f;

    [Header("Projectile Size Upgrade")]
    [SerializeField] private int maxProjectileSizeLevel = 6;

    [Header("Projectile Speed Upgrade")]
    [SerializeField] private int maxProjectileSpeedLevel = 50;
    [SerializeField] private float baseProjectileSpeed = 3f;
    [SerializeField] private float projectileSpeedIncreasePerLevel = 1f;

    [Header("Projectile Count Upgrade")]
    [SerializeField] private int maxProjectileCountLevel = 4;
    [SerializeField] private int projectilesPerLevel = 1;

    [Header("Damage Upgrade")]
    [SerializeField] private float damagePerLevel = 3f;

    [Header("Attack Speed Upgrade")]
    [SerializeField] private int maxAttackSpeedLevel = 50;
    [SerializeField] private int firstPhaseEndLevel = 5;
    [SerializeField] private int secondPhaseEndLevel = 20;
    [SerializeField] private float firstPhaseDecrease = 0.10f;
    [SerializeField] private float secondPhaseDecrease = 0.06f;
    [SerializeField] private float minAttackInterval = 0.10f;

    [Header("Critical Chance Upgrade")]
    [SerializeField] private int maxCriticalChanceLevel = 50;
    [SerializeField] private float maxCriticalChancePercent = 25f;

    [Header("Knockback Upgrade")]
    [SerializeField] private int maxKnockbackLevel = 50;
    [SerializeField] private float maxKnockbackDistance = 0.6f;

    [Header("Enemy Stage Scaling")]
    [SerializeField] private float enemyHpIncreasePerStage = 3f;
    [SerializeField] private int enemyMaxSpeedStage = 40;
    [SerializeField] private float enemyMaxMoveSpeed = 4.5f;

    private void Start()
    {
        RefreshStats();
    }

    private void OnEnable()
    {
        RefreshStats();
    }

    public void RefreshStats()
    {
        RefreshPlayerStats();
        RefreshEnemyStats();
    }

    private void RefreshPlayerStats()
    {
        int damageLevel = PlayerPrefs.GetInt(DamageLevelKey, 0);
        int attackSpeedLevel = PlayerPrefs.GetInt(AttackSpeedLevelKey, 0);
        int castleHpLevel = PlayerPrefs.GetInt(CastleHpLevelKey, 0);
        int projectileSizeLevel = PlayerPrefs.GetInt(ProjectileSizeLevelKey, 0);
        int projectileSpeedLevel = PlayerPrefs.GetInt(ProjectileSpeedLevelKey, 0);
        int criticalChanceLevel = PlayerPrefs.GetInt(CriticalChanceLevelKey, 0);
        int knockbackLevel = PlayerPrefs.GetInt(KnockbackLevelKey, 0);
        int projectileCountLevel = PlayerPrefs.GetInt(ProjectileCountLevelKey, 0);

        projectileSizeLevel = Mathf.Clamp(projectileSizeLevel, 0, maxProjectileSizeLevel);
        projectileSpeedLevel = Mathf.Clamp(projectileSpeedLevel, 0, maxProjectileSpeedLevel);
        criticalChanceLevel = Mathf.Clamp(criticalChanceLevel, 0, maxCriticalChanceLevel);
        knockbackLevel = Mathf.Clamp(knockbackLevel, 0, maxKnockbackLevel);
        projectileCountLevel = Mathf.Clamp(projectileCountLevel, 0, maxProjectileCountLevel);

        float projectileSizeProgress = maxProjectileSizeLevel > 0
            ? (float)projectileSizeLevel / maxProjectileSizeLevel
            : 0f;

        float finalProjectileSpeed = baseProjectileSpeed + projectileSpeedLevel * projectileSpeedIncreasePerLevel;
        float maxProjectileSpeed = baseProjectileSpeed + maxProjectileSpeedLevel * projectileSpeedIncreasePerLevel;

        float criticalChance = maxCriticalChanceLevel > 0
            ? maxCriticalChancePercent * ((float)criticalChanceLevel / maxCriticalChanceLevel)
            : 0f;

        float knockbackDistance = maxKnockbackLevel > 0
            ? maxKnockbackDistance * ((float)knockbackLevel / maxKnockbackLevel)
            : 0f;

        float baseDamage = weaponData != null ? weaponData.baseDamage : 0f;
        float baseAttackInterval = weaponData != null ? weaponData.attackInterval : 0f;
        int baseShotsPerAttack = weaponData != null ? Mathf.Max(weaponData.shotsPerAttack, 1) : 1;

        int finalShotsPerAttack = baseShotsPerAttack + projectileCountLevel * projectilesPerLevel;

        float finalDamage = baseDamage + damageLevel * damagePerLevel;
        float finalAttackInterval = CalculateAttackInterval(baseAttackInterval, attackSpeedLevel);
        float finalCastleHealth = baseCastleHealth + castleHpLevel * castleHealthPerLevel;

        if (statPlayerText != null)
        {
            statPlayerText.richText = true;

            statPlayerText.text =
                "<b>Statystyki:</b>\n\n" +

                "Obrażenia: " + finalDamage.ToString("0") + "\n" +
                "Prędkość ataku: " + finalAttackInterval.ToString("0.00") + " s\n\n" +

                "Wielkość pocisku: " + projectileSizeProgress.ToString("0.00") + " / 1.00\n" +
                "Prędkość pocisku: " + finalProjectileSpeed.ToString("0") + " / " + maxProjectileSpeed.ToString("0") + "\n\n" +

                "Siła odepchnięcia: " + knockbackDistance.ToString("0.00") + " / " + maxKnockbackDistance.ToString("0.00") + "\n" +
                "Szansa na trafienie krytyczne: " + criticalChance.ToString("0.0") + "%\n\n" +

                "Ilość pocisków: " + finalShotsPerAttack + "\n\n" +

                "HP: " + finalCastleHealth.ToString("0");
        }
    }

    private void RefreshEnemyStats()
    {
        if (statMobText == null)
            return;

        if (enemyData == null)
        {
            statMobText.text =
                "Przeciwnicy:\n" +
                "Brak podpiętych danych przeciwnika.";
            return;
        }

        int currentStage = PlayerPrefs.GetInt(CurrentStageKey, 1);
        currentStage = Mathf.Max(currentStage, 1);

        float finalEnemyHealth = CalculateEnemyHealth(currentStage);
        float finalEnemyMoveSpeed = CalculateEnemyMoveSpeed(currentStage);

        statMobText.text =
            "Przeciwnicy:\n\n" +
            "Podstawowy wróg\n" +
            "HP: " + finalEnemyHealth.ToString("0") + "\n" +
            "Obrażenia: " + enemyData.damage.ToString("0") + "\n" +
            "Prędkość ruchu: " + finalEnemyMoveSpeed.ToString("0.0") + "\n\n" +

            "Bossowie:\n" +
            "Ukryte";
    }

    private float CalculateEnemyHealth(int stage)
    {
        float baseHealth = enemyData != null ? enemyData.maxHealth : 0f;
        float bonusHealth = (stage - 1) * enemyHpIncreasePerStage;

        return baseHealth + bonusHealth;
    }

    private float CalculateEnemyMoveSpeed(int stage)
    {
        if (enemyData == null)
            return 0f;

        float baseMoveSpeed = enemyData.moveSpeed;

        if (stage >= enemyMaxSpeedStage)
            return enemyMaxMoveSpeed;

        float progress = (float)(stage - 1) / (enemyMaxSpeedStage - 1);

        return Mathf.Lerp(baseMoveSpeed, enemyMaxMoveSpeed, progress);
    }

    private float CalculateAttackInterval(float baseInterval, int level)
    {
        level = Mathf.Clamp(level, 0, maxAttackSpeedLevel);

        float interval = baseInterval;

        int firstPhaseLevels = Mathf.Min(level, firstPhaseEndLevel);
        interval -= firstPhaseLevels * firstPhaseDecrease;

        int secondPhaseLevels = Mathf.Clamp(level - firstPhaseEndLevel, 0, secondPhaseEndLevel - firstPhaseEndLevel);
        interval -= secondPhaseLevels * secondPhaseDecrease;

        if (level > secondPhaseEndLevel)
        {
            float intervalAtFinalPhaseStart =
                baseInterval -
                firstPhaseEndLevel * firstPhaseDecrease -
                (secondPhaseEndLevel - firstPhaseEndLevel) * secondPhaseDecrease;

            int finalPhaseLevels = level - secondPhaseEndLevel;
            int totalFinalPhaseLevels = maxAttackSpeedLevel - secondPhaseEndLevel;

            float finalPhaseDecreasePerLevel = (intervalAtFinalPhaseStart - minAttackInterval) / totalFinalPhaseLevels;

            interval = intervalAtFinalPhaseStart - finalPhaseLevels * finalPhaseDecreasePerLevel;
        }

        return Mathf.Max(interval, minAttackInterval);
    }
}