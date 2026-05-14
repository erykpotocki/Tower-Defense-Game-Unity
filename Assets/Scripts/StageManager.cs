using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageManager : MonoBehaviour
{
    private const string CurrentStageKey = "CurrentStage";

    [Header("UI")]
    [SerializeField] private TMP_Text stageText;
    [SerializeField] private Image progressFill;

    [Header("Stage Complete UI")]
    [SerializeField] private CanvasGroup completeOverlayGroup;
    [SerializeField] private TMP_Text completeTitleText;
    [SerializeField] private TMP_Text completeRewardText;
    [SerializeField] private TMP_Text continueText;

    [Header("Scene")]
    [SerializeField] private string mainMenuSceneName = "MainActivity";

    [Header("Stage Time")]
    [SerializeField] private float baseStageDuration = 30f;
    [SerializeField] private float extraSecondsEveryTenStages = 5f;

    [Header("Boss Alert")]
    [SerializeField] private bool pulseProgressBarOnBoss = true;
    [SerializeField] private float bossPulseSpeed = 5f;
    [Range(0f, 1f)]
    [SerializeField] private float bossPulseDimMultiplier = 0.35f;
    [SerializeField] private string bossStageText = "BOSS!";

    [Header("Stage Complete Animation")]
    [SerializeField] private float overlayTargetAlpha = 0.55f;
    [SerializeField] private float overlayFadeDuration = 0.7f;
    [SerializeField] private float titlePopDuration = 0.35f;
    [SerializeField] private float rewardPopDuration = 0.3f;
    [SerializeField] private float continuePopDuration = 0.25f;
    [SerializeField] private float delayBetweenTexts = 0.15f;
    [SerializeField] private float continueInputDelay = 1f;

    private int currentStage;
    private float stageDuration;
    private float timer;

    private bool stageTimeEnded;
    private bool stageCompleted;
    private bool rewardGranted;
    private bool canContinue;
    private bool gameOverStarted;

    private Color originalProgressFillColor;
    private Coroutine bossPulseCoroutine;

    private GoldManager goldManager;

    private void Start()
    {
        currentStage = PlayerPrefs.GetInt(CurrentStageKey, 1);

        if (currentStage < 1)
            currentStage = 1;

        stageDuration = GetStageDuration(currentStage);
        timer = 0f;

        goldManager = FindAnyObjectByType<GoldManager>();

        if (stageText != null)
            stageText.text = "Stage " + currentStage;

        if (progressFill != null)
        {
            progressFill.fillAmount = 0f;
            originalProgressFillColor = progressFill.color;
        }

        PrepareCompleteUI();
    }

    private void Update()
    {
        if (gameOverStarted)
            return;

        if (stageCompleted)
        {
            if (canContinue && (Input.GetMouseButtonDown(0) || Input.touchCount > 0))
            {
                SceneManager.LoadScene(mainMenuSceneName);
            }

            return;
        }

        if (!stageTimeEnded)
        {
            timer += Time.deltaTime;

            float progress = Mathf.Clamp01(timer / stageDuration);

            if (progressFill != null)
                progressFill.fillAmount = progress;

            if (timer >= stageDuration)
            {
                EndStageTimer();
            }
        }
        else
        {
            if (AreAllEnemiesGone())
            {
                CompleteStage();
            }
        }
    }

    public void HandleGameOverStarted()
    {
        if (gameOverStarted)
            return;

        gameOverStarted = true;
        StopBossProgressPulse();
        StopAllSpawners();
    }

    private void PrepareCompleteUI()
    {
        if (completeOverlayGroup != null)
        {
            completeOverlayGroup.alpha = 0f;
            completeOverlayGroup.interactable = false;
            completeOverlayGroup.blocksRaycasts = false;
        }

        PrepareText(completeTitleText);
        PrepareText(completeRewardText);
        PrepareText(continueText);
    }

    private void PrepareText(TMP_Text text)
    {
        if (text == null)
            return;

        text.gameObject.SetActive(true);
        text.transform.localScale = Vector3.zero;
    }

    private void EndStageTimer()
    {
        if (stageTimeEnded)
            return;

        stageTimeEnded = true;

        if (progressFill != null)
            progressFill.fillAmount = 1f;

        StopAllSpawners();

        bool bossSpawned = SpawnBossesForCurrentStage();

        if (bossSpawned)
        {
            StartBossProgressPulse();

            if (stageText != null)
                stageText.text = bossStageText;
        }
        else
        {
            if (stageText != null)
                stageText.text = "Dobij ostatnich!";
        }
    }

    private bool SpawnBossesForCurrentStage()
    {
        bool anyBossSpawned = false;

        EnemySpawner[] spawners = FindObjectsByType<EnemySpawner>();

        foreach (EnemySpawner spawner in spawners)
        {
            if (spawner.SpawnBossesForCurrentStage())
                anyBossSpawned = true;
        }

        return anyBossSpawned;
    }

    private void StartBossProgressPulse()
    {
        if (!pulseProgressBarOnBoss || progressFill == null)
            return;

        if (bossPulseCoroutine != null)
            StopCoroutine(bossPulseCoroutine);

        bossPulseCoroutine = StartCoroutine(BossProgressPulseRoutine());
    }

    private void StopBossProgressPulse()
    {
        if (bossPulseCoroutine != null)
        {
            StopCoroutine(bossPulseCoroutine);
            bossPulseCoroutine = null;
        }

        if (progressFill != null)
            progressFill.color = originalProgressFillColor;
    }

    private IEnumerator BossProgressPulseRoutine()
    {
        Color dimmedColor = new Color(
            originalProgressFillColor.r * bossPulseDimMultiplier,
            originalProgressFillColor.g * bossPulseDimMultiplier,
            originalProgressFillColor.b * bossPulseDimMultiplier,
            originalProgressFillColor.a
        );

        while (true)
        {
            float pulse = (Mathf.Sin(Time.time * bossPulseSpeed) + 1f) * 0.5f;
            progressFill.color = Color.Lerp(dimmedColor, originalProgressFillColor, pulse);

            yield return null;
        }
    }

    private bool AreAllEnemiesGone()
    {
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>();
        return enemies == null || enemies.Length == 0;
    }

    private void CompleteStage()
    {
        if (stageCompleted)
            return;

        stageCompleted = true;

        int reward = GetStageReward(currentStage);

        StopBossProgressPulse();
        DisableWeapons();
        StopAllSpawners();

        StartCoroutine(StageCompleteRoutine(reward));
    }

    private void GrantStageReward(int reward)
    {
        if (rewardGranted)
            return;

        rewardGranted = true;

        if (goldManager != null)
            goldManager.AddGold(reward);
        else
            AddGoldDirectly(reward);

        PlayerPrefs.SetInt(CurrentStageKey, currentStage + 1);
        PlayerPrefs.Save();
    }

    private float GetStageDuration(int stage)
    {
        int tenStageGroups = stage / 10;
        return baseStageDuration + tenStageGroups * extraSecondsEveryTenStages;
    }

    private int GetStageReward(int stage)
    {
        if (stage % 10 == 0)
            return (stage / 10) * 1000;

        int rewardGroup = ((stage - 1) / 10) + 1;
        return rewardGroup * 100;
    }

    private void AddGoldDirectly(int amount)
    {
        int currentGold = PlayerPrefs.GetInt("TotalGold", 0);
        currentGold += amount;

        PlayerPrefs.SetInt("TotalGold", currentGold);
        PlayerPrefs.Save();
    }

    private void DisableWeapons()
    {
        WeaponShooter[] weapons = FindObjectsByType<WeaponShooter>();

        foreach (WeaponShooter weapon in weapons)
            weapon.enabled = false;
    }

    private void StopAllSpawners()
    {
        EnemySpawner[] spawners = FindObjectsByType<EnemySpawner>();

        foreach (EnemySpawner spawner in spawners)
            spawner.StopSpawning();
    }

    private IEnumerator StageCompleteRoutine(int reward)
    {
        if (completeTitleText != null)
            completeTitleText.text = "STAGE UKOŃCZONY";

        if (completeRewardText != null)
            completeRewardText.text = "+" + reward + " g";

        if (continueText != null)
            continueText.text = "Naciśnij gdziekolwiek, żeby kontynuować";

        yield return FadeOverlay();

        yield return PopText(completeTitleText, titlePopDuration, 1.12f, 1f);

        yield return new WaitForSeconds(delayBetweenTexts);

        yield return PopText(completeRewardText, rewardPopDuration, 1.08f, 1f);

        GrantStageReward(reward);

        yield return new WaitForSeconds(delayBetweenTexts);

        yield return PopText(continueText, continuePopDuration, 1f, 1f);

        yield return new WaitForSeconds(continueInputDelay);

        canContinue = true;
    }

    private IEnumerator FadeOverlay()
    {
        if (completeOverlayGroup == null)
            yield break;

        completeOverlayGroup.interactable = false;
        completeOverlayGroup.blocksRaycasts = false;

        float fadeTimer = 0f;

        while (fadeTimer < overlayFadeDuration)
        {
            fadeTimer += Time.deltaTime;
            completeOverlayGroup.alpha = Mathf.Lerp(0f, overlayTargetAlpha, fadeTimer / overlayFadeDuration);
            yield return null;
        }

        completeOverlayGroup.alpha = overlayTargetAlpha;
    }

    private IEnumerator PopText(TMP_Text text, float duration, float maxScale, float finalScale)
    {
        if (text == null)
            yield break;

        float popTimer = 0f;

        while (popTimer < duration)
        {
            popTimer += Time.deltaTime;
            float t = popTimer / duration;
            float scale = Mathf.Lerp(0f, maxScale, t);

            text.transform.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        float settleDuration = 0.12f;
        float settleTimer = 0f;

        while (settleTimer < settleDuration)
        {
            settleTimer += Time.deltaTime;
            float t = settleTimer / settleDuration;
            float scale = Mathf.Lerp(maxScale, finalScale, t);

            text.transform.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        text.transform.localScale = new Vector3(finalScale, finalScale, 1f);
    }
}