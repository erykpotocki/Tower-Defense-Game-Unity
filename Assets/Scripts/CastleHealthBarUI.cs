using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CastleHealthBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CastleHealth castleHealth;
    [SerializeField] private Image fillImage;
    [SerializeField] private TMP_Text hpText;

    private void Start()
    {
        if (castleHealth == null)
            castleHealth = FindAnyObjectByType<CastleHealth>();

        UpdateUI();
    }

    private void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (castleHealth == null)
            return;

        float currentHealth = castleHealth.CurrentHealth;
        float maxHealth = castleHealth.MaxHealth;

        if (fillImage != null)
            fillImage.fillAmount = maxHealth > 0f ? currentHealth / maxHealth : 0f;

        if (hpText != null)
            hpText.text = $"{currentHealth:0} / {maxHealth:0}";
    }
}