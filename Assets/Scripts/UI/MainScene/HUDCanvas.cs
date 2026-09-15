using TMPro;
using UnityEngine;

public class HUDCanvas : MonoBehaviour
{
    [SerializeField] private TMP_Text countText;
    [SerializeField] private GameFlowManager gameFlowManager;

    private void OnEnable()
    {
        if (gameFlowManager != null)
        {
            gameFlowManager.ActionPointsChanged += UpdateCountText;
        }
    }

    private void Start()
    {
        RefreshCountText();
    }

    private void OnDisable()
    {
        if (gameFlowManager != null)
        {
            gameFlowManager.ActionPointsChanged -= UpdateCountText;
        }
    }

    private void RefreshCountText()
    {
        if (gameFlowManager == null)
        {
            return;
        }

        UpdateCountText(gameFlowManager.RemainingActionPoints, gameFlowManager.MaxActionPoints);
    }

    private void UpdateCountText(int remainingActionPoints, int maxActionPoints)
    {
        if (countText != null)
        {
            countText.text = $"{remainingActionPoints} / {maxActionPoints}";
        }
    }
}
