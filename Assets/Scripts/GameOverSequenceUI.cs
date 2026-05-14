using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverSequenceUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup overlayGroup;
    [SerializeField] private TMP_Text gameOverText;
    [SerializeField] private TMP_Text continueText;

    [Header("Scene")]
    [SerializeField] private string mainMenuSceneName = "MainActivity";

    [Header("Fade")]
    [SerializeField] private float targetDarkAlpha = 0.7f;
    [SerializeField] private float fadeDuration = 0.6f;

    [Header("Game Over Text")]
    [SerializeField] private float gameOverPopDuration = 0.35f;
    [SerializeField] private float gameOverMaxScale = 1.15f;

    [Header("Continue Text")]
    [SerializeField] private float continuePopDuration = 0.25f;
    [SerializeField] private float continueInputDelay = 1f;

    private bool isShowing;
    private bool canContinue;

    private void Awake()
    {
        if (overlayGroup != null)
            overlayGroup.alpha = 0f;

        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
            gameOverText.transform.localScale = Vector3.zero;
        }

        if (continueText != null)
        {
            continueText.gameObject.SetActive(true);
            continueText.transform.localScale = Vector3.zero;
        }
    }

    private void Update()
    {
        if (!canContinue)
            return;

        if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    public void ShowGameOver()
    {
        if (isShowing)
            return;

        isShowing = true;
        StartCoroutine(GameOverRoutine());
    }

    private IEnumerator GameOverRoutine()
    {
        yield return FadeOverlay();

        DisableWeapons();

        yield return PopText(gameOverText, gameOverPopDuration, gameOverMaxScale);
        yield return PopText(continueText, continuePopDuration, 1f);

        yield return new WaitForSeconds(continueInputDelay);

        canContinue = true;
    }

    private void DisableWeapons()
    {
        WeaponShooter[] weapons = FindObjectsByType<WeaponShooter>();

        foreach (WeaponShooter weapon in weapons)
        {
            weapon.enabled = false;
        }
    }

    private IEnumerator FadeOverlay()
    {
        if (overlayGroup == null)
            yield break;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            overlayGroup.alpha = Mathf.Lerp(0f, targetDarkAlpha, timer / fadeDuration);
            yield return null;
        }

        overlayGroup.alpha = targetDarkAlpha;
    }

    private IEnumerator PopText(TMP_Text text, float duration, float maxScale)
    {
        if (text == null)
            yield break;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            float scale = Mathf.Lerp(0f, maxScale, t);

            text.transform.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        text.transform.localScale = Vector3.one;
    }
}