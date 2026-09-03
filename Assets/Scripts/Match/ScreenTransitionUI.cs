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

    private GameObject overlayObject;
    private CanvasGroup canvasGroup;
    private TMP_Text loadingText;
    private TMP_Text fontSource;
    private bool isTransitioning;

    public bool IsTransitioning => isTransitioning;

    public void Initialize(Transform uiRoot, TMP_Text sourceText)
    {
        fontSource = sourceText;

        if (overlayObject != null)
        {
            ApplyFontSource();
            return;
        }

        if (uiRoot == null)
        {
            return;
        }

        overlayObject = new GameObject("ScreenTransitionOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
        overlayObject.transform.SetParent(uiRoot, false);

        RectTransform overlayTransform = overlayObject.GetComponent<RectTransform>();
        overlayTransform.anchorMin = Vector2.zero;
        overlayTransform.anchorMax = Vector2.one;
        overlayTransform.offsetMin = Vector2.zero;
        overlayTransform.offsetMax = Vector2.zero;

        Image overlayImage = overlayObject.GetComponent<Image>();
        overlayImage.color = Color.black;
        overlayImage.raycastTarget = true;

        canvasGroup = overlayObject.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        GameObject textObject = new GameObject("ScreenTransitionText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(overlayObject.transform, false);

        RectTransform textTransform = textObject.GetComponent<RectTransform>();
        textTransform.anchorMin = new Vector2(0.5f, 0.5f);
        textTransform.anchorMax = new Vector2(0.5f, 0.5f);
        textTransform.anchoredPosition = Vector2.zero;
        textTransform.sizeDelta = new Vector2(520f, 96f);

        loadingText = textObject.GetComponent<TMP_Text>();
        loadingText.text = DefaultLoadingText;
        loadingText.color = Color.white;
        loadingText.fontSize = 34f;
        loadingText.enableAutoSizing = true;
        loadingText.fontSizeMin = 16f;
        loadingText.fontSizeMax = 34f;
        loadingText.alignment = TextAlignmentOptions.Center;

        ApplyFontSource();
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
