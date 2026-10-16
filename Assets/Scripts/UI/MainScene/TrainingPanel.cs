using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrainingPanel : MonoBehaviour
{
    private const int RandomTrainingCardCount = 3;

    [SerializeField] private CanvasGroupView trainingPanelView;
    [SerializeField] private GameObject resultContainer;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text energyText;
    [SerializeField] private Button returnHomeButton;
    [SerializeField] private Button resultCloseButton;
    [SerializeField] private TrainingCard[] trainingCards;

    private GameFlowManager gameFlowManager;
    private readonly List<TrainingOffer> offers = new List<TrainingOffer>();
    private int offerYear;
    private int offerMonth;
    private bool isTraining;

    private void Awake()
    {
        resultContainer.SetActive(false);
        Hide();
    }

    public void StartTrainingAction(GameFlowManager owner)
    {
        gameFlowManager = owner;
        Show();
    }

    public void Show()
    {
        trainingPanelView.Show();
        EnsureOffers();
        RefreshCards();
    }

    public void Hide()
    {
        trainingPanelView.Hide();
    }

    private void EnsureOffers()
    {
        int year = PlayerSaveManager.LoadCareerYear(1);
        int month = PlayerSaveManager.LoadCareerMonth(11);
        if (offers.Count == RandomTrainingCardCount && offerYear == year && offerMonth == month) return;
        offers.Clear();
        foreach (TrainingDefinition training in TrainingOptionPicker.PickRandomTrainings(RandomTrainingCardCount))
            offers.Add(new TrainingOffer(training, Random.Range(1, 4)));
        offerYear = year;
        offerMonth = month;
    }

    private void RefreshCards()
    {
        resultContainer.SetActive(false);
        int remaining = gameFlowManager != null ? gameFlowManager.RemainingActionPoints : 0;
        int maximum = gameFlowManager != null ? gameFlowManager.MaxActionPoints : 3;
        energyText.text = $"에너지: {remaining} / {maximum}";
        for (int i = 0; i < trainingCards.Length; i++)
        {
            bool hasOffer = i < offers.Count;
            trainingCards[i].gameObject.SetActive(hasOffer);
            if (!hasOffer) continue;
            TrainingOffer offer = offers[i];
            bool atMaximum = !TrainingService.CanTrain(offer.Training);
            string reason = atMaximum ? "능력치가 최대치(99)입니다."
                : remaining < offer.EnergyCost ? "에너지가 부족합니다." : string.Empty;
            trainingCards[i].SetData(offer, !isTraining && gameFlowManager != null && string.IsNullOrEmpty(reason), reason);
        }
    }

    public void SelectTraining(int index)
    {
        if (isTraining || gameFlowManager == null || index < 0 || index >= offers.Count) return;
        TrainingOffer offer = offers[index];
        if (!TrainingService.CanTrain(offer.Training) || gameFlowManager.RemainingActionPoints < offer.EnergyCost)
        {
            RefreshCards();
            return;
        }
        isTraining = true;
        foreach (TrainingCard card in trainingCards) card.SetInteractable(false);
        returnHomeButton.interactable = false;
        gameFlowManager.PlayUiTransition(string.Empty, () => ApplySelectedTraining(offer));
    }

    private void ApplySelectedTraining(TrainingOffer offer)
    {
        if (!TrainingService.CanTrain(offer.Training) || !gameFlowManager.TryConsumeActionPoints(offer.EnergyCost))
        {
            isTraining = false;
            returnHomeButton.interactable = true;
            RefreshCards();
            return;
        }
        TrainingResult result = TrainingService.ApplyTraining(offer.Training);
        result.EnergySpent = offer.EnergyCost;
        result.RemainingActionPoints = gameFlowManager.RemainingActionPoints;
        result.MaxActionPoints = gameFlowManager.MaxActionPoints;
        offers.Clear();
        isTraining = false;
        returnHomeButton.interactable = true;
        foreach (TrainingCard card in trainingCards) card.gameObject.SetActive(false);
        resultText.text = $"{result.TrainingName} 완료!\n{TrainingCatalog.GetStatName(result.StatType)} +{result.Increase}"
            + $"\n{result.BeforeValue} → {result.AfterValue}\n소모 에너지: {result.EnergySpent}"
            + $"\n남은 에너지: {result.RemainingActionPoints} / {result.MaxActionPoints}";
        resultContainer.SetActive(true);
    }

    public void ReturnHome()
    {
        if (isTraining) return;
        resultContainer.SetActive(false);
        Hide();
        if (gameFlowManager != null) gameFlowManager.CompleteTrainingAction();
    }

    public void HandleResultClose()
    {
        ReturnHome();
    }
}
