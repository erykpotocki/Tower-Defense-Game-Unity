using UnityEngine;
using UnityEngine.EventSystems;

public class WeaponShooter : MonoBehaviour
{
    private const string DamageLevelKey = "SlingshotDamageLevel";
    private const string AttackSpeedLevelKey = "SlingshotAttackSpeedLevel";
    private const string CriticalChanceLevelKey = "CriticalChanceLevel";
    private const string KnockbackLevelKey = "KnockbackLevel";
    private const string ProjectileCountLevelKey = "ProjectileCountLevel";

    [Header("Weapon Data")]
    [SerializeField] private WeaponData weaponData;

    [Header("Projectile")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform firePoint;

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
    [SerializeField] private float criticalDamageMultiplier = 2f;

    [Header("Knockback Upgrade")]
    [SerializeField] private int maxKnockbackLevel = 50;
    [SerializeField] private float maxKnockbackDistance = 0.6f;

    [Header("Projectile Count Upgrade")]
    [SerializeField] private int maxProjectileCountLevel = 4;
    [SerializeField] private int projectilesPerLevel = 1;
    [SerializeField] private float spreadAnglePerProjectile = 8f;

    [Header("Debug")]
    [SerializeField] private float debugFinalAttackInterval;
    [SerializeField] private int debugAttackSpeedLevel;
    [SerializeField] private float debugCriticalChancePercent;
    [SerializeField] private float debugKnockbackDistance;
    [SerializeField] private int debugProjectileCount;

    private float nextShootTime;

    private void Start()
    {
        nextShootTime = 0f;
    }

    private void Update()
    {
        if (weaponData == null || projectilePrefab == null)
            return;

        debugAttackSpeedLevel = PlayerPrefs.GetInt(AttackSpeedLevelKey, 0);
        debugFinalAttackInterval = GetFinalAttackInterval();
        debugCriticalChancePercent = GetFinalCriticalChancePercent();
        debugKnockbackDistance = GetFinalKnockbackDistance();
        debugProjectileCount = GetFinalProjectileCount();

        if (TryGetAimScreenPosition(out Vector2 screenPosition))
            TryShoot(screenPosition);
    }

    private bool TryGetAimScreenPosition(out Vector2 screenPosition)
    {
        screenPosition = Vector2.zero;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                return false;

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                return false;

            screenPosition = touch.position;
            return true;
        }

        if (Input.GetMouseButton(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return false;

            screenPosition = Input.mousePosition;
            return true;
        }

        return false;
    }

    private void TryShoot(Vector2 screenPosition)
    {
        if (Time.time < nextShootTime)
            return;

        Camera mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        Vector3 worldPoint = mainCamera.ScreenToWorldPoint(screenPosition);
        worldPoint.z = 0f;

        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;

        Vector2 direction = worldPoint - spawnPosition;

        if (direction.sqrMagnitude <= 0.01f)
            return;

        ShootProjectiles(spawnPosition, direction.normalized);

        nextShootTime = Time.time + GetFinalAttackInterval();
    }

    private void ShootProjectiles(Vector3 spawnPosition, Vector2 baseDirection)
    {
        int projectileCount = GetFinalProjectileCount();

        if (projectileCount <= 1)
        {
            SpawnSingleProjectile(spawnPosition, baseDirection);
            return;
        }

        float totalSpread = spreadAnglePerProjectile * (projectileCount - 1);
        float startAngle = -totalSpread * 0.5f;

        for (int i = 0; i < projectileCount; i++)
        {
            float angle = startAngle + spreadAnglePerProjectile * i;
            Vector2 spreadDirection = Quaternion.Euler(0f, 0f, angle) * baseDirection;

            SpawnSingleProjectile(spawnPosition, spreadDirection);
        }
    }

    private void SpawnSingleProjectile(Vector3 spawnPosition, Vector2 direction)
    {
        Projectile projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

        float finalDamage = GetFinalDamageWithCritical();
        float finalKnockbackDistance = GetFinalKnockbackDistance();

        projectile.Init(direction, finalDamage, finalKnockbackDistance);
    }

    private int GetFinalProjectileCount()
    {
        int projectileCountLevel = PlayerPrefs.GetInt(ProjectileCountLevelKey, 0);
        projectileCountLevel = Mathf.Clamp(projectileCountLevel, 0, maxProjectileCountLevel);

        int baseProjectileCount = weaponData != null ? Mathf.Max(weaponData.shotsPerAttack, 1) : 1;

        return baseProjectileCount + projectileCountLevel * projectilesPerLevel;
    }

    private float GetFinalDamageWithCritical()
    {
        float damage = GetBaseFinalDamage();
        float criticalChance = GetFinalCriticalChancePercent();

        float randomValue = Random.Range(0f, 100f);

        if (randomValue <= criticalChance)
            damage *= criticalDamageMultiplier;

        return damage;
    }

    private float GetBaseFinalDamage()
    {
        int damageLevel = PlayerPrefs.GetInt(DamageLevelKey, 0);
        return weaponData.baseDamage + damageLevel * damagePerLevel;
    }

    private float GetFinalCriticalChancePercent()
    {
        int criticalChanceLevel = PlayerPrefs.GetInt(CriticalChanceLevelKey, 0);
        criticalChanceLevel = Mathf.Clamp(criticalChanceLevel, 0, maxCriticalChanceLevel);

        if (maxCriticalChanceLevel <= 0)
            return 0f;

        float progress = (float)criticalChanceLevel / maxCriticalChanceLevel;
        return maxCriticalChancePercent * progress;
    }

    private float GetFinalKnockbackDistance()
    {
        int knockbackLevel = PlayerPrefs.GetInt(KnockbackLevelKey, 0);
        knockbackLevel = Mathf.Clamp(knockbackLevel, 0, maxKnockbackLevel);

        if (maxKnockbackLevel <= 0)
            return 0f;

        float progress = (float)knockbackLevel / maxKnockbackLevel;
        return maxKnockbackDistance * progress;
    }

    private float GetFinalAttackInterval()
    {
        int attackSpeedLevel = PlayerPrefs.GetInt(AttackSpeedLevelKey, 0);
        return CalculateAttackInterval(weaponData.attackInterval, attackSpeedLevel);
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