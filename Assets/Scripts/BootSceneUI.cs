using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class BootSceneUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI continueText;
    [SerializeField] private Animator loadingAnimator;

    [Header("Scene")]
    [SerializeField] private string nextSceneName = "MainActivity";

    [Header("Timing")]
    [SerializeField] private float waitAfterLoad = 0.15f;

    [Header("Fade In")]
    [SerializeField] private float fadeInDuration = 1.2f;

    [Header("Pulse")]
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseAlphaMin = 0.75f;
    [SerializeField] private float pulseAlphaMax = 1f;

    private bool loadingFinished = false;
    private bool canContinue = false;
    private float fadeTimer = 0f;
    private float pulseTimer = 0f;
    private Color baseColor;

    private void Start()
    {
        if (continueText == null)
        {
            Debug.LogError("BootSceneUI: ContinueText nie jest podpięty.");
            enabled = false;
            return;
        }

        baseColor = continueText.color;
        SetTextAlpha(0f);

        float loadingLength = 0f;

        if (loadingAnimator != null &&
            loadingAnimator.runtimeAnimatorController != null &&
            loadingAnimator.GetCurrentAnimatorStateInfo(0).length > 0f)
        {
            loadingLength = loadingAnimator.GetCurrentAnimatorStateInfo(0).length;
        }

        if (loadingLength <= 0f)
        {
            loadingLength = 2f;
        }

        Invoke(nameof(BeginContinueText), loadingLength + waitAfterLoad);
    }

    private void BeginContinueText()
    {
        loadingFinished = true;
    }

    private void Update()
    {
        if (loadingFinished && !canContinue)
        {
            FadeInText();
        }
        else if (canContinue)
        {
            PulseText();

            if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
            {
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }

    private void FadeInText()
    {
        fadeTimer += Time.deltaTime;
        float t = Mathf.Clamp01(fadeTimer / fadeInDuration);

        SetTextAlpha(t);

        if (t >= 1f)
        {
            canContinue = true;
        }
    }

    private void PulseText()
    {
        pulseTimer += Time.deltaTime * pulseSpeed;
        float wave = (Mathf.Sin(pulseTimer) + 1f) * 0.5f;
        float alpha = Mathf.Lerp(pulseAlphaMin, pulseAlphaMax, wave);
        SetTextAlpha(alpha);
    }

    private void SetTextAlpha(float alpha)
    {
        Color c = baseColor;
        c.a = alpha;
        continueText.color = c;
    }
}