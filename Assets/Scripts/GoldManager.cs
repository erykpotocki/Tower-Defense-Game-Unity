using TMPro;
using UnityEngine;

public class GoldManager : MonoBehaviour
{
    private const string TotalGoldKey = "TotalGold";
    private const string CurrentStageKey = "CurrentStage";

    [Header("UI")]
    [SerializeField] private TMP_Text goldText;

    [Header("Enemy Gold Reward")]
    [SerializeField] private int baseGoldPerEnemy = 20;
    [SerializeField] private int goldIncreasePerStage = 5;

    [Header("Boss Gold Reward")]
    [SerializeField] private int baseGoldPerBoss = 500;
    [SerializeField] private int bossGoldIncreaseEveryBossStage = 250;
    [SerializeField] private int firstBossStage = 5;
    [SerializeField] private int bossStageInterval = 5;

    private int totalGold;

    public int TotalGold => totalGold;

    private void Start()
    {
        totalGold = PlayerPrefs.GetInt(TotalGoldKey, 0);
        UpdateUI();
    }

    public void AddEnemyKillGold()
    {
        AddGold(GetCurrentEnemyKillGold());
    }

    public void AddBossKillGold()
    {
        AddGold(GetCurrentBossKillGold());
    }

    public void AddGold(int amount)
    {
        totalGold += amount;

        PlayerPrefs.SetInt(TotalGoldKey, totalGold);
        PlayerPrefs.Save();

        UpdateUI();
    }

    public int GetCurrentEnemyKillGold()
    {
        int currentStage = PlayerPrefs.GetInt(CurrentStageKey, 1);
        currentStage = Mathf.Max(currentStage, 1);

        return baseGoldPerEnemy + (currentStage - 1) * goldIncreasePerStage;
    }

    public int GetCurrentBossKillGold()
    {
        int currentStage = PlayerPrefs.GetInt(CurrentStageKey, 1);
        currentStage = Mathf.Max(currentStage, 1);

        if (currentStage < firstBossStage)
            return baseGoldPerBoss;

        int bossStageIndex = (currentStage - firstBossStage) / bossStageInterval;

        return baseGoldPerBoss + bossStageIndex * bossGoldIncreaseEveryBossStage;
    }

    private void UpdateUI()
    {
        if (goldText != null)
            goldText.text = totalGold + " g";
    }
}