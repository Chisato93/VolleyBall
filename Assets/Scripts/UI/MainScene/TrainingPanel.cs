using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrainingPanel : MonoBehaviour
{
    private const int RandomTrainingCardCount = 4;

    [SerializeField] private CanvasGroupView trainingPanelView;
    [SerializeField] private GameObject resultContainer;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private Button returnHomeButton;
    [SerializeField] private Button resultCloseButton;
    [SerializeField] private TrainingCard[] trainingCards;

    private GameFlowManager gameFlowManager;

    private void Awake()
    {
        CacheSceneReferences();
        BindReturnHomeButton();
        BindResultCloseButton();
        SetResultActive(false);
        Hide();
    }

    public void StartTrainingAction(GameFlowManager owner)
    {
        gameFlowManager = owner;
        Show();
    }

    public void Show()
    {
        CacheSceneReferences();
        BindReturnHomeButton();
        BindResultCloseButton();

        if (trainingPanelView != null)
        {
            trainingPanelView.Show();
        }

        ShowRandomOptions();
    }

    public void Hide()
    {
        if (trainingPanelView != null)
        {
            trainingPanelView.Hide();
            return;
        }
    }

    private void ShowRandomOptions()
    {
        CacheSceneReferences();

        var options = TrainingOptionPicker.PickRandomTrainings(RandomTrainingCardCount);
        int optionCount = trainingCards != null
            ? Mathf.Min(trainingCards.Length, options.Count)
            : 0;

        if (optionCount == 0)
        {
            Debug.LogWarning("Training cards are not assigned.");
            SetResultActive(false);
            return;
        }

        SetTrainingCardsActive(true);
        SetResultActive(false);

        for (int i = 0; i < trainingCards.Length; i++)
        {
            if (trainingCards[i] == null)
            {
                continue;
            }

            bool isActive = i < optionCount;
            trainingCards[i].gameObject.SetActive(isActive);

            if (!isActive)
            {
                continue;
            }

            TrainingDefinition training = options[i];
            trainingCards[i].SetData(
                null,
                training.Name,
                training.GetDetailText(),
                training.GetEffectText(),
                training.GetTimeText(),
                () => SelectTraining(training));
        }
    }

    private void SelectTraining(TrainingDefinition training)
    {
        if (gameFlowManager != null)
        {
            gameFlowManager.PlayUiTransition(string.Empty, () => ApplySelectedTraining(training));
            return;
        }

        ApplySelectedTraining(training);
    }

    private void ApplySelectedTraining(TrainingDefinition training)
    {
        TrainingResult result = TrainingService.ApplyTraining(training);

        if (gameFlowManager != null)
        {
            result.RemainingActionPoints = gameFlowManager.ConsumeActionPoint();
            result.MaxActionPoints = gameFlowManager.MaxActionPoints;
        }

        ShowResult(result);
    }

    private void ShowResult(TrainingResult result)
    {
        SetTrainingCardsActive(false);
        SetResultActive(true);

        if (resultText == null && resultContainer != null)
        {
            resultText = resultContainer.GetComponentInChildren<TMP_Text>(true);
        }

        if (resultText == null)
        {
            Debug.LogWarning("Training result text is not assigned.");
            return;
        }

        resultText.text = $"{result.TrainingName} 완료!\n{result.StatType} +{result.Increase}\n{result.BeforeValue} -> {result.AfterValue}\n남은 행동력: {result.RemainingActionPoints} / {result.MaxActionPoints}";
    }

    private void HandleResultClose()
    {
        CloseTrainingView();

        if (gameFlowManager != null)
        {
            gameFlowManager.CompleteTrainingAction();
            return;
        }
    }

    private void CloseTrainingView()
    {
        SetResultActive(false);
        SetTrainingCardsActive(false);
        Hide();
    }

    private void SetTrainingCardsActive(bool isActive)
    {
        if (trainingCards == null)
        {
            return;
        }

        foreach (TrainingCard trainingCard in trainingCards)
        {
            if (trainingCard != null)
            {
                trainingCard.gameObject.SetActive(isActive);
            }
        }
    }

    private void SetResultActive(bool isActive)
    {
        if (resultContainer != null)
        {
            resultContainer.SetActive(isActive);
            return;
        }

        Debug.LogWarning("Training result container is not assigned.");
    }

    private void CacheSceneReferences()
    {
        if (trainingPanelView == null)
        {
            trainingPanelView = GetComponent<CanvasGroupView>();
        }

        if (trainingCards == null || trainingCards.Length == 0)
        {
            trainingCards = GetComponentsInChildren<TrainingCard>(true);
        }

        if (resultContainer == null)
        {
            Transform resultTransform = transform.Find("TrainingResult");
            if (resultTransform != null)
            {
                resultContainer = resultTransform.gameObject;
            }
        }

        if (resultText == null && resultContainer != null)
        {
            resultText = resultContainer.GetComponentInChildren<TMP_Text>(true);
        }

        if (returnHomeButton == null)
        {
            Transform returnHomeTransform = transform.Find("ReturnHome");
            if (returnHomeTransform != null)
            {
                returnHomeButton = returnHomeTransform.GetComponent<Button>();
            }
        }

        if (resultCloseButton == null && resultContainer != null)
        {
            Transform closeTransform = resultContainer.transform.Find("TrainingResultCloseButton");
            if (closeTransform != null)
            {
                resultCloseButton = closeTransform.GetComponent<Button>();
            }
        }
    }

    private void BindReturnHomeButton()
    {
        if (returnHomeButton == null)
        {
            return;
        }

        returnHomeButton.onClick.RemoveListener(Hide);
        returnHomeButton.onClick.AddListener(Hide);
    }

    private void BindResultCloseButton()
    {
        if (resultCloseButton == null)
        {
            return;
        }

        resultCloseButton.onClick.RemoveListener(HandleResultClose);
        resultCloseButton.onClick.AddListener(HandleResultClose);
    }

}
