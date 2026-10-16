using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnhancementRevealPanel : MonoBehaviour
{
    [SerializeField] private GameObject openingPanel;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private RectTransform animatedIcon;
    [SerializeField] private Image glowImage;
    [SerializeField] private Image resultIcon;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text itemDescriptionText;
    [SerializeField, Min(0.2f)] private float openingDuration = 2.5f;

    private Coroutine openingRoutine;
    private ItemData reward;
    private bool[] results;
    [SerializeField] private TMP_Text progressText;

    public bool IsOpen => gameObject.activeSelf;
    public bool IsConfigured => openingPanel != null && resultPanel != null && animatedIcon != null
        && progressText != null && glowImage != null && resultIcon != null && itemNameText != null && itemDescriptionText != null;

    public void Open(ItemData item, bool[] rolls)
    {
        if (item == null || !IsConfigured || IsOpen)
        {
            return;
        }

        // 강화 결과는 이미 저장되어 있으며 이 화면은 연출만 담당합니다.
        reward = item;
        results = (bool[])rolls.Clone();
        progressText.text = "강화 준비 중...";
        openingPanel.SetActive(true);
        resultPanel.SetActive(false);
        animatedIcon.localScale = Vector3.one;
        animatedIcon.localRotation = Quaternion.identity;
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        openingRoutine = StartCoroutine(PlayOpening());
    }

    private IEnumerator PlayOpening()
    {
        float elapsed = 0f;
        float duration = Mathf.Max(0.2f, openingDuration);
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            int step = Mathf.Clamp(Mathf.FloorToInt(progress * 5f), 0, 4);
            int currentLevel = 0;
            for (int i = 0; i < step; i++)
                if (results[i]) currentLevel++;
            progressText.text = $"{step + 1}/5회 · +{currentLevel} → +{currentLevel + 1} · "
                + (results[step] ? "성공!" : $"실패 (+{currentLevel} 유지)");
            float pulse = Mathf.Sin(elapsed * 9f);
            animatedIcon.localRotation = Quaternion.Euler(0f, 0f, -elapsed * 180f);
            animatedIcon.localScale = Vector3.one * (0.8f + progress * 0.35f + pulse * 0.07f);
            glowImage.color = new Color(1f, 0.82f, 0.3f, 0.35f + progress * 0.4f + pulse * 0.1f);
            yield return null;
        }

        openingRoutine = null;
        ShowResult();
    }

    public void Skip()
    {
        if (!IsOpen || !openingPanel.activeSelf)
        {
            return;
        }

        StopOpening();
        ShowResult();
    }

    private void ShowResult()
    {
        if (reward == null)
        {
            return;
        }

        openingPanel.SetActive(false);
        resultIcon.sprite = reward.Icon;
        resultIcon.enabled = reward.Icon != null;
        itemNameText.text = $"최종 강화 +{reward.EnhancementLevel}";
        var summary = new System.Text.StringBuilder();
        int level = 0;
        for (int i = 0; i < results.Length; i++)
        {
            string outcome = results[i] ? "<color=#65DC8A>성공</color>" : $"실패 (+{level} 유지)";
            summary.AppendLine($"{i + 1}회: +{level} → +{level + 1} {outcome}");
            if (results[i]) level++;
        }
        summary.AppendLine("재강화 불가");
        summary.AppendLine();
        summary.Append(ItemTooltipUI.BuildText(reward));
        itemDescriptionText.text = summary.ToString();
        resultPanel.SetActive(true);
    }

    public void Close()
    {
        StopOpening();
        reward = null;
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        StopOpening();
        reward = null;
    }

    private void StopOpening()
    {
        if (openingRoutine != null)
        {
            StopCoroutine(openingRoutine);
            openingRoutine = null;
        }
    }
}
