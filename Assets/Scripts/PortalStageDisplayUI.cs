using TMPro;
using UnityEngine;

public class PortalStageDisplayUI : MonoBehaviour
{
    private const string CurrentStageKey = "CurrentStage";

    [Header("UI")]
    [SerializeField] private TMP_Text stageText;
    [SerializeField] private TMP_Text stageTextOutline;

    private void Start()
    {
        RefreshStageText();
    }

    private void OnEnable()
    {
        RefreshStageText();
    }

    public void RefreshStageText()
    {
        int currentStage = PlayerPrefs.GetInt(CurrentStageKey, 1);

        if (currentStage < 1)
            currentStage = 1;

        string text = "Stage " + currentStage;

        if (stageText != null)
            stageText.text = text;

        if (stageTextOutline != null)
            stageTextOutline.text = text;
    }
}