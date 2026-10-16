using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RandomItemRevealPanel : MonoBehaviour
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

    public bool IsOpen => gameObject.activeSelf;
    public bool IsConfigured => openingPanel != null && resultPanel != null && animatedIcon != null
        && glowImage != null && resultIcon != null && itemNameText != null && itemDescriptionText != null;

    public void Open(ItemData item)
    {
        if (item == null || !IsConfigured || IsOpen)
        {
            return;
        }

        // 구매·지급은 상점에서 이미 끝났습니다. 여기서는 결과를 보여주기만 합니다.
        reward = item;
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
        itemNameText.text = reward.ItemName;
        itemDescriptionText.text = ItemTooltipUI.BuildText(reward);
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
