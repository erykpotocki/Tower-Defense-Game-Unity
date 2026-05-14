using TMPro;
using UnityEngine;

public class GoldDisplayUI : MonoBehaviour
{
    private const string TotalGoldKey = "TotalGold";

    [Header("UI")]
    [SerializeField] private TMP_Text goldText;

    private void Start()
    {
        UpdateGoldText();
    }

    private void OnEnable()
    {
        UpdateGoldText();
    }

    public void UpdateGoldText()
    {
        int totalGold = PlayerPrefs.GetInt(TotalGoldKey, 0);

        if (goldText != null)
            goldText.text = totalGold + " g";
    }
}