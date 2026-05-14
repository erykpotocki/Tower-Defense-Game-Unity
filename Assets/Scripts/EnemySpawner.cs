using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    private const string CurrentStageKey = "CurrentStage";

    [Header("Normal Enemy Spawn Settings")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private float spawnDelay = 2f;
    [SerializeField] private float spawnX = 9f;
    [SerializeField] private float minSpawnY = -3.5f;
    [SerializeField] private float maxSpawnY = 3.5f;

    [Header("Boss Spawn Settings")]
    [SerializeField] private GameObject singleBossPrefab;
    [SerializeField] private GameObject[] doubleBossPrefabs;
    [SerializeField] private float bossSpawnX = 11f;
    [SerializeField] private float bossSpawnSpacingY = 2.3f;
    [SerializeField] private int bossStageInterval = 5;
    [SerializeField] private int doubleBossStageInterval = 10;
    [SerializeField] private int maxBossStage = 100;

    [Header("Stage Scaling")]
    [SerializeField] private float spawnDelayDecreasePerStage = 0.08f;
    [SerializeField] private float minSpawnDelay = 0.5f;

    private bool isSpawning;
    private bool bossesSpawned;
    private float finalSpawnDelay;
    private int currentStage;

    private void Start()
    {
        LoadCurrentStage();
        CalculateStageSpawnDelay();
        StartSpawning();
    }

    private void LoadCurrentStage()
    {
        currentStage = PlayerPrefs.GetInt(CurrentStageKey, 1);
        currentStage = Mathf.Max(currentStage, 1);
    }

    private void CalculateStageSpawnDelay()
    {
        float delayReduction = (currentStage - 1) * spawnDelayDecreasePerStage;
        finalSpawnDelay = Mathf.Max(spawnDelay - delayReduction, minSpawnDelay);
    }

    public void StartSpawning()
    {
        isSpawning = true;

        CancelInvoke(nameof(SpawnEnemy));
        InvokeRepeating(nameof(SpawnEnemy), 1f, finalSpawnDelay);
    }

    public void StopSpawning()
    {
        isSpawning = false;
        CancelInvoke(nameof(SpawnEnemy));
    }

    public bool SpawnBossesForCurrentStage()
    {
        if (bossesSpawned)
            return false;

        if (!IsBossStage())
            return false;

        bossesSpawned = true;

        if (IsDoubleBossStage())
            return SpawnDoubleBosses();

        return SpawnSingleBoss(singleBossPrefab, 0f);
    }

    private bool IsBossStage()
    {
        if (currentStage < bossStageInterval)
            return false;

        if (currentStage > maxBossStage)
            return false;

        return currentStage % bossStageInterval == 0;
    }

    private bool IsDoubleBossStage()
    {
        return currentStage % doubleBossStageInterval == 0;
    }

    private void SpawnEnemy()
    {
        if (!isSpawning)
            return;

        GameObject selectedEnemyPrefab = GetRandomEnemyPrefab();

        if (selectedEnemyPrefab == null)
            return;

        float randomY = Random.Range(minSpawnY, maxSpawnY);
        Vector3 spawnPosition = new Vector3(spawnX, randomY, 0f);

        Instantiate(selectedEnemyPrefab, spawnPosition, Quaternion.identity);
    }

    private GameObject GetRandomEnemyPrefab()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
            return null;

        int randomIndex = Random.Range(0, enemyPrefabs.Length);
        return enemyPrefabs[randomIndex];
    }

    private bool SpawnSingleBoss(GameObject bossPrefab, float yPosition)
    {
        if (bossPrefab == null)
            return false;

        Vector3 spawnPosition = new Vector3(bossSpawnX, yPosition, 0f);
        Instantiate(bossPrefab, spawnPosition, Quaternion.identity);

        return true;
    }

    private bool SpawnDoubleBosses()
    {
        if (doubleBossPrefabs == null || doubleBossPrefabs.Length == 0)
            return false;

        bool spawnedAny = false;

        if (doubleBossPrefabs.Length == 1)
        {
            spawnedAny |= SpawnSingleBoss(doubleBossPrefabs[0], 0f);
            return spawnedAny;
        }

        if (doubleBossPrefabs[0] != null)
            spawnedAny |= SpawnSingleBoss(doubleBossPrefabs[0], bossSpawnSpacingY);

        if (doubleBossPrefabs[1] != null)
            spawnedAny |= SpawnSingleBoss(doubleBossPrefabs[1], -bossSpawnSpacingY);

        return spawnedAny;
    }
}