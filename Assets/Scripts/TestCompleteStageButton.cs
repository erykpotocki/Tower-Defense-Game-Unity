using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TestCompleteStageButton : MonoBehaviour
{
    private const string TotalGoldKey = "TotalGold";
    private const string CurrentStageKey = "CurrentStage";

    [Header("Button")]
    [SerializeField] private Button completeStageButton;

    private void Start()
    {
        if (completeStageButton == null)
            completeStageButton = GetComponent<Button>();

        if (completeStageButton != null)
            completeStageButton.onClick.AddListener(CompleteStageForTest);
    }

    private void OnDestroy()
    {
        if (completeStageButton != null)
            completeStageButton.onClick.RemoveListener(CompleteStageForTest);
    }

    private void CompleteStageForTest()
    {
        int currentStage = PlayerPrefs.GetInt(CurrentStageKey, 1);

        if (currentStage < 1)
            currentStage = 1;

        int reward = GetStageReward(currentStage);

        int totalGold = PlayerPrefs.GetInt(TotalGoldKey, 0);
        totalGold += reward;

        PlayerPrefs.SetInt(TotalGoldKey, totalGold);
        PlayerPrefs.SetInt(CurrentStageKey, currentStage + 1);
        PlayerPrefs.Save();

        Debug.Log("TEST: Ukończono stage " + currentStage + ", nagroda +" + reward + " g. Nowy stage: " + (currentStage + 1));

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private int GetStageReward(int stage)
    {
        if (stage % 10 == 0)
            return (stage / 10) * 1000;

        int rewardGroup = ((stage - 1) / 10) + 1;
        return rewardGroup * 100;
    }
}