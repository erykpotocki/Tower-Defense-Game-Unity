using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopProjectileSizeUpgradeUI : MonoBehaviour
{
    private const string TotalGoldKey = "TotalGold";
    private const string ProjectileSizeLevelKey = "ProjectileSizeLevel";

    [Header("UI")]
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private ShopStatsSummaryUI statsSummaryUI;

    [Header("Upgrade Cost")]
    [SerializeField] private int baseCost = 500;
    [SerializeField] private int costIncreasePerLevel = 500;
    [SerializeField] private int secondLastUpgradeExtraCost = 2000;
    [SerializeField] private int lastUpgradeExtraCost = 3000;

    [Header("Projectile Size Upgrade")]
    [SerializeField] private int maxProjectileSizeLevel = 6;

    [Header("Button Text Style")]
    [SerializeField] private Color outlineColor = Color.black;
    [SerializeField] private float outlineWidth = 0.18f;
    [SerializeField] private string costGoldColorHex = "#C69214";

    private int totalGold;
    private int projectileSizeLevel;
    private ShopHoldToRepeatButton holdToRepeatButton;

    private void Awake()
    {
        ApplyButtonTextStyle();
        SetupHoldButton();
    }

    private void OnEnable()
    {
        SetupHoldButton();

        if (holdToRepeatButton != null)
            holdToRepeatButton.OnPressedOrRepeated += BuyUpgrade;

        ShopRefreshEvents.OnShopRefreshRequested += RefreshUI;

        RefreshUI();
    }

    private void OnDisable()
    {
        if (holdToRepeatButton != null)
            holdToRepeatButton.OnPressedOrRepeated -= BuyUpgrade;

        ShopRefreshEvents.OnShopRefreshRequested -= RefreshUI;
    }

    private void SetupHoldButton()
    {
        if (upgradeButton == null)
            return;

        holdToRepeatButton = upgradeButton.GetComponent<ShopHoldToRepeatButton>();

        if (holdToRepeatButton == null)
            holdToRepeatButton = upgradeButton.gameObject.AddComponent<ShopHoldToRepeatButton>();
    }

    private void LoadData()
    {
        totalGold = PlayerPrefs.GetInt(TotalGoldKey, 0);
        projectileSizeLevel = PlayerPrefs.GetInt(ProjectileSizeLevelKey, 0);
    }

    private int GetCost()
    {
        int cost = baseCost + projectileSizeLevel * costIncreasePerLevel;
        int nextLevel = projectileSizeLevel + 1;

        if (nextLevel == maxProjectileSizeLevel - 1)
            cost += secondLastUpgradeExtraCost;

        if (nextLevel == maxProjectileSizeLevel)
            cost += lastUpgradeExtraCost;

        return cost;
    }

    private void BuyUpgrade()
    {
        LoadData();

        if (projectileSizeLevel >= maxProjectileSizeLevel)
        {
            ShopRefreshEvents.RequestShopRefresh();
            return;
        }

        int cost = GetCost();

        if (totalGold < cost)
        {
            ShopRefreshEvents.RequestShopRefresh();
            return;
        }

        totalGold -= cost;
        projectileSizeLevel++;

        projectileSizeLevel = Mathf.Clamp(projectileSizeLevel, 0, maxProjectileSizeLevel);

        PlayerPrefs.SetInt(TotalGoldKey, totalGold);
        PlayerPrefs.SetInt(ProjectileSizeLevelKey, projectileSizeLevel);
        PlayerPrefs.Save();

        if (statsSummaryUI != null)
            statsSummaryUI.RefreshStats();

        ShopRefreshEvents.RequestShopRefresh();
    }

    private void RefreshUI()
    {
        LoadData();

        int cost = GetCost();
        bool isMax = projectileSizeLevel >= maxProjectileSizeLevel;
        bool canAfford = totalGold >= cost;

        if (goldText != null)
            goldText.text = totalGold + " g";

        if (buttonText != null)
        {
            buttonText.richText = true;

            if (isMax)
            {
                buttonText.text =
                    "<b>Wielkość pocisku</b>" +
                    "<space=1.4em>" +
                    "Lv. " + projectileSizeLevel + "/" + maxProjectileSizeLevel +
                    "<space=1.4em>" +
                    "<color=" + costGoldColorHex + "><b>MAX</b></color>";
            }
            else
            {
                buttonText.text =
                    "<b>Wielkość pocisku</b>" +
                    "<space=1.4em>" +
                    "Lv. " + projectileSizeLevel + "/" + maxProjectileSizeLevel +
                    "<space=1.4em>" +
                    "<color=" + costGoldColorHex + "><b>Koszt: " + cost + " g</b></color>";
            }
        }

        if (upgradeButton != null)
            upgradeButton.interactable = !isMax && canAfford;
    }

    private void ApplyButtonTextStyle()
    {
        if (buttonText == null)
            return;

        Material textMaterial = new Material(buttonText.fontMaterial);
        textMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, outlineWidth);
        textMaterial.SetColor(ShaderUtilities.ID_OutlineColor, outlineColor);

        buttonText.fontMaterial = textMaterial;
    }
}