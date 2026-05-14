using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TestResetProgressButton : MonoBehaviour
{
    [Header("Button")]
    [SerializeField] private Button resetButton;

    [Header("Debug")]
    [SerializeField] private bool resetAllPlayerPrefs = true;

    private const string TotalGoldKey = "TotalGold";
    private const string DamageLevelKey = "SlingshotDamageLevel";
    private const string AttackSpeedLevelKey = "SlingshotAttackSpeedLevel";
    private const string CastleHpLevelKey = "CastleHpLevel";
    private const string CurrentStageKey = "CurrentStage";
    private const string ProjectileSizeLevelKey = "ProjectileSizeLevel";
    private const string ProjectileSpeedLevelKey = "ProjectileSpeedLevel";
    private const string CriticalChanceLevelKey = "CriticalChanceLevel";
    private const string KnockbackLevelKey = "KnockbackLevel";
    private const string ProjectileCountLevelKey = "ProjectileCountLevel";

    private void Start()
    {
        if (resetButton == null)
            resetButton = GetComponent<Button>();

        if (resetButton != null)
            resetButton.onClick.AddListener(ResetProgress);
    }

    private void OnDestroy()
    {
        if (resetButton != null)
            resetButton.onClick.RemoveListener(ResetProgress);
    }

    private void ResetProgress()
    {
        if (resetAllPlayerPrefs)
        {
            PlayerPrefs.DeleteAll();
        }
        else
        {
            PlayerPrefs.DeleteKey(TotalGoldKey);
            PlayerPrefs.DeleteKey(DamageLevelKey);
            PlayerPrefs.DeleteKey(AttackSpeedLevelKey);
            PlayerPrefs.DeleteKey(CastleHpLevelKey);
            PlayerPrefs.DeleteKey(CurrentStageKey);
            PlayerPrefs.DeleteKey(ProjectileSizeLevelKey);
            PlayerPrefs.DeleteKey(ProjectileSpeedLevelKey);
            PlayerPrefs.DeleteKey(CriticalChanceLevelKey);
            PlayerPrefs.DeleteKey(KnockbackLevelKey);
            PlayerPrefs.DeleteKey(ProjectileCountLevelKey);
        }

        PlayerPrefs.Save();

        Debug.Log("TEST RESET: progress, gold, ulepszenia i stage wyzerowane");

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}