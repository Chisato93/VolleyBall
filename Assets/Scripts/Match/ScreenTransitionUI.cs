using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScreenTransitionUI : MonoBehaviour
{
    private const string DefaultLoadingText = "불러오는 중...";

    [SerializeField] private float fadeOutDuration = 0.25f;
    [SerializeField] private float loadingHoldDuration = 0.35f;
    [SerializeField] private float fadeInDuration = 0.25f;
    [SerializeField] private float quickFadeDuration = 0.12f;

    [SerializeField] private GameObject overlayObject;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text loadingText;
    private TMP_Text fontSource;
    private bool isTransitioning;

    public bool IsTransitioning => isTransitioning;

    public void Initialize(TMP_Text sourceText)
    {
        fontSource = sourceText;
        if (overlayObject == null || canvasGroup == null || loadingText == null)
        {
            Debug.LogError("ScreenTransitionUI의 씬 UI 연결이 비어 있습니다.", this);
            return;
        }

        ApplyFontSource();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        overlayObject.SetActive(false);
    }

    public bool TryPlay(string message, Action transitionAction)
    {
        if (isTransitioning)
        {
            return false;
        }

        if (overlayObject == null || canvasGroup == null)
        {
            transitionAction?.Invoke();
            return true;
        }

        StartCoroutine(PlayTransition(message, true, loadingHoldDuration, fadeOutDuration, fadeInDuration, transitionAction));
        return true;
    }

    public bool TryPlayQuick(Action transitionAction)
    {
        if (isTransitioning)
        {
            return false;
        }

        if (overlayObject == null || canvasGroup == null)
        {
            transitionAction?.Invoke();
            return true;
        }

        StartCoroutine(PlayTransition(string.Empty, false, 0f, quickFadeDuration, quickFadeDuration, transitionAction));
        return true;
    }

    private IEnumerator PlayTransition(
        string message,
        bool showLoadingText,
        float holdDuration,
        float outDuration,
        float inDuration,
        Action transitionAction)
    {
        isTransitioning = true;
        overlayObject.SetActive(true);
        overlayObject.transform.SetAsLastSibling();
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        if (loadingText != null)
        {
            loadingText.gameObject.SetActive(showLoadingText);
            loadingText.text = string.IsNullOrEmpty(message) ? DefaultLoadingText : message;
        }

        yield return Fade(0f, 1f, outDuration);

        if (holdDuration > 0f)
        {
            yield return new WaitForSecondsRealtime(holdDuration);
        }

        try
        {
            transitionAction?.Invoke();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }

        overlayObject.transform.SetAsLastSibling();

        yield return Fade(1f, 0f, inDuration);

        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        overlayObject.SetActive(false);
        isTransitioning = false;
    }

    private IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        if (duration <= 0f)
        {
            canvasGroup.alpha = endAlpha;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
    }

    private void ApplyFontSource()
    {
        if (fontSource == null || loadingText == null)
        {
            return;
        }

        loadingText.font = fontSource.font;
        loadingText.fontSharedMaterial = fontSource.fontSharedMaterial;
    }
}
