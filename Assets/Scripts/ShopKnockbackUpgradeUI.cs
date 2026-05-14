using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopKnockbackUpgradeUI : MonoBehaviour
{
    private const string TotalGoldKey = "TotalGold";
    private const string KnockbackLevelKey = "KnockbackLevel";

    [Header("UI")]
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private ShopStatsSummaryUI statsSummaryUI;

    [Header("Upgrade Cost")]
    [SerializeField] private int baseCost = 100;
    [SerializeField] private int costIncreasePerLevel = 75;

    [Header("Knockback Upgrade")]
    [SerializeField] private int maxKnockbackLevel = 50;

    [Header("Button Text Style")]
    [SerializeField] private Color outlineColor = Color.black;
    [SerializeField] private float outlineWidth = 0.18f;
    [SerializeField] private string costGoldColorHex = "#C69214";

    private int totalGold;
    private int knockbackLevel;
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
        knockbackLevel = PlayerPrefs.GetInt(KnockbackLevelKey, 0);
    }

    private int GetCost()
    {
        return baseCost + knockbackLevel * costIncreasePerLevel;
    }

    private void BuyUpgrade()
    {
        LoadData();

        if (knockbackLevel >= maxKnockbackLevel)
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
        knockbackLevel++;

        knockbackLevel = Mathf.Clamp(knockbackLevel, 0, maxKnockbackLevel);

        PlayerPrefs.SetInt(TotalGoldKey, totalGold);
        PlayerPrefs.SetInt(KnockbackLevelKey, knockbackLevel);
        PlayerPrefs.Save();

        if (statsSummaryUI != null)
            statsSummaryUI.RefreshStats();

        ShopRefreshEvents.RequestShopRefresh();
    }

    private void RefreshUI()
    {
        LoadData();

        int cost = GetCost();
        bool isMax = knockbackLevel >= maxKnockbackLevel;
        bool canAfford = totalGold >= cost;

        if (goldText != null)
            goldText.text = totalGold + " g";

        if (buttonText != null)
        {
            buttonText.richText = true;

            if (isMax)
            {
                buttonText.text =
                    "<b>Siła odepchnięcia</b>" +
                    "<space=1.4em>" +
                    "Lv. " + knockbackLevel + "/" + maxKnockbackLevel +
                    "<space=1.4em>" +
                    "<color=" + costGoldColorHex + "><b>MAX</b></color>";
            }
            else
            {
                buttonText.text =
                    "<b>Siła odepchnięcia</b>" +
                    "<space=1.4em>" +
                    "Lv. " + knockbackLevel + "/" + maxKnockbackLevel +
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