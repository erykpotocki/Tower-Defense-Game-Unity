using UnityEngine;

public class Projectile : MonoBehaviour
{
    private const string ProjectileSizeLevelKey = "ProjectileSizeLevel";
    private const string ProjectileSpeedLevelKey = "ProjectileSpeedLevel";
    private const float AutoDestroyAfterSeconds = 8f;

    [Header("Hit")]
    [SerializeField] private float hitRadius = 0.12f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Size Upgrade")]
    [SerializeField] private int maxProjectileSizeLevel = 6;
    [SerializeField] private float baseProjectileScale = 0.03f;
    [SerializeField] private float maxProjectileScale = 0.09f;
    [SerializeField] private float baseHitRadius = 0.12f;
    [SerializeField] private float maxHitRadius = 0.21f;

    [Header("Speed Upgrade")]
    [SerializeField] private int maxProjectileSpeedLevel = 50;
    [SerializeField] private float baseProjectileSpeed = 3f;
    [SerializeField] private float speedIncreasePerLevel = 1f;

    [Header("Screen Bounds")]
    [SerializeField] private bool destroyOutsideCamera = true;
    [SerializeField] private float cameraBoundsMargin = 0.05f;

    private Vector2 moveDirection;
    private float damage;
    private float currentSpeed;
    private float knockbackDistance;

    private void Awake()
    {
        ApplyProjectileUpgrades();
    }

    public void Init(Vector2 direction, float newDamage, float newKnockbackDistance)
    {
        moveDirection = direction.normalized;
        damage = newDamage;
        knockbackDistance = newKnockbackDistance;

        ApplyProjectileUpgrades();

        Destroy(gameObject, AutoDestroyAfterSeconds);
    }

    private void ApplyProjectileUpgrades()
    {
        ApplyProjectileSizeUpgrade();
        ApplyProjectileSpeedUpgrade();
    }

    private void ApplyProjectileSizeUpgrade()
    {
        int projectileSizeLevel = PlayerPrefs.GetInt(ProjectileSizeLevelKey, 0);
        projectileSizeLevel = Mathf.Clamp(projectileSizeLevel, 0, maxProjectileSizeLevel);

        float progress = maxProjectileSizeLevel > 0
            ? (float)projectileSizeLevel / maxProjectileSizeLevel
            : 0f;

        float finalScale = Mathf.Lerp(baseProjectileScale, maxProjectileScale, progress);
        float finalHitRadius = Mathf.Lerp(baseHitRadius, maxHitRadius, progress);

        transform.localScale = new Vector3(finalScale, finalScale, transform.localScale.z);
        hitRadius = finalHitRadius;
    }

    private void ApplyProjectileSpeedUpgrade()
    {
        int projectileSpeedLevel = PlayerPrefs.GetInt(ProjectileSpeedLevelKey, 0);
        projectileSpeedLevel = Mathf.Clamp(projectileSpeedLevel, 0, maxProjectileSpeedLevel);

        currentSpeed = baseProjectileSpeed + projectileSpeedLevel * speedIncreasePerLevel;
    }

    private void Update()
    {
        transform.position += (Vector3)(moveDirection * currentSpeed * Time.deltaTime);

        if (destroyOutsideCamera && IsOutsideCameraBounds())
        {
            Destroy(gameObject);
            return;
        }

        Collider2D hit = Physics2D.OverlapCircle(transform.position, hitRadius, enemyLayer);

        if (hit == null)
            return;

        bool damageApplied = false;

        EnemyHealth enemyHealth = hit.GetComponent<EnemyHealth>();

        if (enemyHealth != null)
            damageApplied = enemyHealth.TakeDamage(damage);

        if (damageApplied)
        {
            EnemyMover enemyMover = hit.GetComponent<EnemyMover>();

            if (enemyMover != null && knockbackDistance > 0f)
                enemyMover.ApplyKnockback(knockbackDistance);
        }

        Destroy(gameObject);
    }

    private bool IsOutsideCameraBounds()
    {
        Camera mainCamera = Camera.main;

        if (mainCamera == null)
            return false;

        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(transform.position);

        return viewportPosition.x < -cameraBoundsMargin ||
               viewportPosition.x > 1f + cameraBoundsMargin ||
               viewportPosition.y < -cameraBoundsMargin ||
               viewportPosition.y > 1f + cameraBoundsMargin;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
}