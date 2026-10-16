using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameFlowManager : MonoBehaviour
{
    private const int MinRestIncrease = 1;
    private const int MaxRestIncreaseExclusive = 4;
    private const int MaxStatValue = PlayerStats.MaxValue;
    private const int StartingStatValue = 20;
    private const string CareerPlayerSaveId = "Player Outside Hitter";

    [SerializeField] private int maxActionPoints = 3;
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private TMP_Text actionPointText;
    [SerializeField] private Button trainingActionButton;
    [SerializeField] private Button restActionButton;
    [SerializeField] private Button shopActionButton;
    [SerializeField] private Button playerInfoActionButton;
    [SerializeField] private Button matchActionButton;
    [SerializeField] private TrainingPanel trainingPanel;
    [SerializeField] private WalletManager walletManager;
    [SerializeField] private CareerCalendarManager careerCalendarManager;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private Button resultCloseButton;
    [SerializeField] private ScreenTransitionUI screenTransitionUI;
    [SerializeField] private ShopPanel shopPanel;
    [SerializeField] private StatusPanel statusPanel;
    [SerializeField] private Button homeShopButton;

    private int remainingActionPoints;
    private ResultCloseAction resultCloseAction;

    public int MaxActionPoints => maxActionPoints;
    public int RemainingActionPoints => remainingActionPoints;
    public event Action<int, int> ActionPointsChanged;

    private enum ResultCloseAction
    {
        None,
        CompleteAction,
        ReturnToVillage
    }

    private void Awake()
    {
        EnsureDataManagers();
        SetResultPanelActive(false);
        if (screenTransitionUI != null)
        {
            screenTransitionUI.Initialize(actionPointText);
        }
    }

    private void Start()
    {
        EnsureDataManagers();
        StartMainPhase();
    }

    public void CompleteTrainingAction()
    {
        SaveCurrentProgress();
        PlayQuickFade(CompleteAction);
    }

    public void PlayUiTransition(string loadingMessage, Action transitionAction)
    {
        if (string.IsNullOrEmpty(loadingMessage))
        {
            PlayQuickFade(transitionAction);
            return;
        }

        PlayScreenTransition(loadingMessage, transitionAction);
    }

    private void StartMainPhase()
    {
        remainingActionPoints = PlayerSaveManager.LoadEnergy(maxActionPoints,
            PlayerSaveManager.LoadCareerYear(1), PlayerSaveManager.LoadCareerMonth(11));
        SaveCurrentProgress();
        ShowMainPanel();
    }

    private void SaveCurrentProgress()
    {
        PlayerSaveManager.SaveEnergy(remainingActionPoints,
            PlayerSaveManager.LoadCareerYear(1), PlayerSaveManager.LoadCareerMonth(11));
        if (InventoryPanel.ActivePanel != null) InventoryPanel.ActivePanel.SaveCurrentState();
        PlayerSaveManager.SaveCheckpoint();
    }

    private void ShowMainPanel()
    {
        if (shopPanel != null)
        {
            shopPanel.gameObject.SetActive(false);
        }

        if (trainingPanel != null)
        {
            trainingPanel.Hide();
        }

        if (mainPanel != null)
        {
            mainPanel.SetActive(true);
        }

        UpdateMainPanel();
    }

    public void StartTrainingAction()
    {
        if (remainingActionPoints <= 0)
        {
            PlayQuickFade(ShowMainPanel);
            return;
        }

        PlayQuickFade(StartTrainingActionNow);
    }

    private void StartTrainingActionNow()
    {
        if (mainPanel != null)
        {
            mainPanel.SetActive(false);
        }

        if (trainingPanel != null)
        {
            trainingPanel.StartTrainingAction(this);
        }
    }

    private void UpdateMainPanel()
    {
        EnsureDataManagers();

        if (actionPointText != null)
        {
            int money = walletManager != null ? walletManager.CurrentMoney : 0;
            int year = careerCalendarManager != null ? careerCalendarManager.CurrentYear : 1;
            int month = careerCalendarManager != null ? careerCalendarManager.CurrentMonth : 11;
            int evaluationScore = PlayerSaveManager.LoadSeasonCoachEvaluationScore();

            actionPointText.text = $"마을\n에너지: {remainingActionPoints} / {maxActionPoints}\n돈: {money:N0}원\n날짜: {year}년 {month}월\n감독 평가점: {evaluationScore}";
        }

        if (trainingActionButton != null)
        {
            trainingActionButton.interactable = remainingActionPoints > 0;
        }

        if (restActionButton != null)
        {
            restActionButton.interactable = remainingActionPoints > 0;
        }

        if (shopActionButton != null)
        {
            shopActionButton.interactable = true;
        }

        if (playerInfoActionButton != null)
        {
            playerInfoActionButton.interactable = true;
        }

        if (matchActionButton != null)
        {
            matchActionButton.interactable = true;
        }

        ActionPointsChanged?.Invoke(remainingActionPoints, maxActionPoints);
    }

    public void StartRestAction()
    {
        if (remainingActionPoints <= 0)
        {
            PlayQuickFade(ShowMainPanel);
            return;
        }

        PlayQuickFade(StartRestActionNow);
    }

    private void StartRestActionNow()
    {
        if (mainPanel != null)
        {
            mainPanel.SetActive(false);
        }

        RestResult result = ApplyRestAction();
        ConsumeActionPoint();
        ShowActionResult(result);
    }

    public bool TryConsumeActionPoints(int amount)
    {
        if (amount <= 0 || remainingActionPoints < amount) return false;
        remainingActionPoints -= amount;
        SaveCurrentProgress();
        UpdateMainPanel();
        return true;
    }

    public int ConsumeActionPoint()
    {
        if (remainingActionPoints > 0)
        {
            remainingActionPoints--;
        }

        SaveCurrentProgress();
        UpdateMainPanel();
        return remainingActionPoints;
    }

    private RestResult ApplyRestAction()
    {
        RestResult result = new RestResult();
        PlayerStats stats = LoadCareerPlayerStats();
        int staminaBefore = stats.Stamina;
        int mentalBefore = stats.Mental;
        int staminaIncrease = UnityEngine.Random.Range(MinRestIncrease, MaxRestIncreaseExclusive);
        int mentalIncrease = UnityEngine.Random.Range(MinRestIncrease, MaxRestIncreaseExclusive);

        stats.AddValue(PlayerStatType.Stamina, staminaIncrease, MaxStatValue);
        stats.AddValue(PlayerStatType.Mental, mentalIncrease, MaxStatValue);
        PlayerSaveManager.SavePlayerStats(CareerPlayerSaveId, stats);

        result.StaminaBefore = staminaBefore;
        result.StaminaAfter = stats.Stamina;
        result.MentalBefore = mentalBefore;
        result.MentalAfter = stats.Mental;
        return result;
    }

    private void CompleteAction()
    {
        ShowMainPanel();
    }

    public void StartShopAction()
    {
        PlayQuickFade(StartShopActionNow);
    }

    private void StartShopActionNow()
    {
        if (shopPanel == null)
        {
            Debug.LogError("MainScene의 GameFlowManager에 ShopCanvas의 ShopPanel을 연결해야 합니다.", this);
            return;
        }

        statusPanel?.Hide();
        if (trainingPanel != null)
        {
            trainingPanel.Hide();
        }

        if (mainPanel != null)
        {
            mainPanel.SetActive(false);
        }

        SetResultPanelActive(false);
        shopPanel.Open();
    }

    public void CloseShop()
    {
        PlayQuickFade(ShowMainPanel);
    }

    public void StartPlayerInfoAction()
    {
        PlayQuickFade(StartPlayerInfoActionNow);
    }

    private void StartPlayerInfoActionNow()
    {
        resultCloseAction = ResultCloseAction.ReturnToVillage;

        if (mainPanel != null)
        {
            mainPanel.SetActive(false);
        }

        SetResultPanelActive(true);

        if (resultText != null)
        {
            resultText.text = BuildPlayerInfoText();
        }
    }

    public void StartMatchAction()
    {
        PlayScreenTransition("경기 준비 중...", LoadMatchScene);
    }

    private void LoadMatchScene()
    {
        SaveCurrentProgress();
        if (SceneLoadManager.Instance != null)
        {
            SceneLoadManager.Instance.LoadScene(SceneEnums.MatchScene);
            return;
        }

        SceneManager.LoadScene((int)SceneEnums.MatchScene);
    }

    private string BuildPlayerInfoText()
    {
        EnsureDataManagers();
        StringBuilder builder = new StringBuilder();
        int money = walletManager != null ? walletManager.CurrentMoney : 0;
        int year = careerCalendarManager != null ? careerCalendarManager.CurrentYear : 1;
        int month = careerCalendarManager != null ? careerCalendarManager.CurrentMonth : 11;
        int evaluationScore = PlayerSaveManager.LoadSeasonCoachEvaluationScore();

        builder.AppendLine("내정보");
        builder.AppendLine();
        builder.AppendLine($"돈: {money:N0}원");
        builder.AppendLine($"날짜: {year}년 {month}월");
        builder.AppendLine($"시즌 감독 평가점: {evaluationScore}");
        builder.AppendLine();

        PlayerStats stats = LoadCareerPlayerStats();
        POSITION selectedPosition = PlayerSaveManager.LoadSelectedPosition(POSITION.OutsideHitter);
        builder.AppendLine($"이름: {CareerPlayerSaveId}");
        builder.AppendLine($"포지션: {selectedPosition}");
        builder.AppendLine();
        builder.AppendLine(EquipmentStatsService.BuildStatsText(stats));
        return builder.ToString();
    }

    private void ShowActionResult(RestResult result)
    {
        resultCloseAction = ResultCloseAction.CompleteAction;
        SetResultPanelActive(true);

        if (resultText == null)
        {
            return;
        }

        resultText.text = $"휴식 완료!\n체력 +{result.StaminaAfter - result.StaminaBefore} ({result.StaminaBefore} -> {result.StaminaAfter})\n멘탈 +{result.MentalAfter - result.MentalBefore} ({result.MentalBefore} -> {result.MentalAfter})\n남은 에너지: {remainingActionPoints} / {maxActionPoints}";
    }

    public void HandleResultClose()
    {
        PlayQuickFade(HandleResultCloseNow);
    }

    private void HandleResultCloseNow()
    {
        SaveCurrentProgress();
        SetResultPanelActive(false);

        switch (resultCloseAction)
        {
            case ResultCloseAction.CompleteAction:
                CompleteAction();
                break;
            case ResultCloseAction.ReturnToVillage:
                ShowMainPanel();
                break;
        }

        resultCloseAction = ResultCloseAction.None;
    }

    private void SetResultPanelActive(bool isActive)
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(isActive);
        }
    }

    private void PlayScreenTransition(string loadingMessage, Action transitionAction)
    {
        if (screenTransitionUI == null)
        {
            transitionAction?.Invoke();
            return;
        }

        screenTransitionUI.TryPlay(loadingMessage, transitionAction);
    }

    private void PlayQuickFade(Action transitionAction)
    {
        if (screenTransitionUI == null)
        {
            transitionAction?.Invoke();
            return;
        }

        screenTransitionUI.TryPlayQuick(transitionAction);
    }

    private void EnsureDataManagers()
    {
        if (walletManager == null)
        {
            walletManager = WalletManager.Instance != null ? WalletManager.Instance : FindFirstObjectByType<WalletManager>();
        }

        if (careerCalendarManager == null)
        {
            careerCalendarManager = CareerCalendarManager.Instance != null ? CareerCalendarManager.Instance : FindFirstObjectByType<CareerCalendarManager>();
        }
    }

    private static PlayerStats LoadCareerPlayerStats()
    {
        PlayerStats stats = CreateDefaultCareerPlayerStats();
        PlayerSaveManager.LoadPlayerStats(CareerPlayerSaveId, stats);
        return stats;
    }

    private static PlayerStats CreateDefaultCareerPlayerStats()
    {
        PlayerStats stats = new PlayerStats();
        stats.Power = StartingStatValue;
        stats.Jump = StartingStatValue;
        stats.Receive = StartingStatValue;
        stats.Set = StartingStatValue;
        stats.Block = StartingStatValue;
        stats.Serve = StartingStatValue;
        stats.Speed = StartingStatValue;
        stats.Stamina = StartingStatValue;
        stats.Mental = StartingStatValue;
        return stats;
    }

    private struct RestResult
    {
        public int StaminaBefore;
        public int StaminaAfter;
        public int MentalBefore;
        public int MentalAfter;
    }

}
