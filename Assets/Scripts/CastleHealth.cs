using UnityEngine;

public class CastleHealth : MonoBehaviour
{
    private const string CastleHpLevelKey = "CastleHpLevel";

    [Header("Castle Stats")]
    [SerializeField] private float baseMaxHealth = 100f;
    [SerializeField] private float healthPerLevel = 11f;

    private float maxHealth;
    private float currentHealth;
    private bool isDestroyed;
    private GameOverSequenceUI gameOverSequenceUI;
    private StageManager stageManager;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDestroyed => isDestroyed;

    private void Start()
    {
        int castleHpLevel = PlayerPrefs.GetInt(CastleHpLevelKey, 0);
        castleHpLevel = Mathf.Max(castleHpLevel, 0);

        maxHealth = baseMaxHealth + castleHpLevel * healthPerLevel;
        currentHealth = maxHealth;

        gameOverSequenceUI = FindAnyObjectByType<GameOverSequenceUI>();
        stageManager = FindAnyObjectByType<StageManager>();
    }

    public void TakeDamage(float damage)
    {
        if (isDestroyed)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        if (currentHealth <= 0f)
        {
            isDestroyed = true;
            Debug.Log("Castle destroyed");

            if (stageManager != null)
                stageManager.HandleGameOverStarted();

            if (gameOverSequenceUI != null)
                gameOverSequenceUI.ShowGameOver();
            else
                Debug.LogWarning("Nie znaleziono GameOverSequenceUI w scenie.");
        }
    }
}