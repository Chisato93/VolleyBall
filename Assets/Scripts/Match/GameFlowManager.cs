using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameFlowManager : MonoBehaviour
{
    private const int WinEvaluationScore = 30;
    private const int LoseEvaluationScore = 10;
    private const int ScorePointEvaluationScore = 2;
    private const int AttackSuccessEvaluationScore = 2;
    private const int AttackFailureEvaluationPenalty = 1;
    private const int WinAllowance = 100;
    private const int LoseAllowance = 30;
    private const int MinRestIncrease = 1;
    private const int MaxRestIncreaseExclusive = 4;
    private const int MaxStatValue = 99;
    private const int StartingStatValue = 20;
    private const int StartingBonusPoints = 10;
    private const int InitialStatCap = 30;

    private static readonly PlayerStatType[] StartingStatTypes =
    {
        PlayerStatType.Power,
        PlayerStatType.Jump,
        PlayerStatType.Receive,
        PlayerStatType.Set,
        PlayerStatType.Block,
        PlayerStatType.Serve,
        PlayerStatType.Speed,
        PlayerStatType.Stamina,
        PlayerStatType.Mental
    };

    [SerializeField] private int maxActionPoints = 3;
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private TMP_Text actionPointText;
    [SerializeField] private Button trainingActionButton;
    [SerializeField] private Button restActionButton;
    [SerializeField] private Button shopActionButton;
    [SerializeField] private Button playerInfoActionButton;
    [SerializeField] private Button matchActionButton;
    [SerializeField] private MatchManager matchManager;
    [SerializeField] private TrainingSelectionUI trainingSelectionUI;
    [SerializeField] private WalletManager walletManager;
    [SerializeField] private CareerCalendarManager careerCalendarManager;
    [SerializeField] private TitleScreenCanvas titleScreenCanvas;
    [SerializeField] private TitleScreenUI titleScreenUI;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private Button resultCloseButton;
    [SerializeField] private ScreenTransitionUI screenTransitionUI;

    private int remainingActionPoints;
    private ResultCloseAction resultCloseAction;
    private bool hasAppliedMatchReward;
    private MatchRewardResult lastMatchRewardResult;

    public int MaxActionPoints => maxActionPoints;
    public int RemainingActionPoints => remainingActionPoints;

    private enum ResultCloseAction
    {
        None,
        CompleteAction,
        ReturnToVillage,
        ReturnToMain
    }

    private void Awake()
    {
        if (matchManager == null)
        {
            matchManager = FindFirstObjectByType<MatchManager>();
        }

        if (trainingSelectionUI == null)
        {
            trainingSelectionUI = FindFirstObjectByType<TrainingSelectionUI>();
        }

        if (walletManager == null)
        {
            walletManager = FindFirstObjectByType<WalletManager>();
        }

        if (careerCalendarManager == null)
        {
            careerCalendarManager = FindFirstObjectByType<CareerCalendarManager>();
        }

        if (titleScreenUI == null)
        {
            titleScreenUI = FindFirstObjectByType<TitleScreenUI>();
        }

        if (titleScreenUI == null)
        {
            titleScreenUI = gameObject.AddComponent<TitleScreenUI>();
        }

        CreateMissingMainControls();
        CreateMissingResultPanel();
        titleScreenUI.Initialize(this, GetUiRoot(), actionPointText);
        EnsureScreenTransitionUI();

        if (trainingActionButton != null)
        {
            trainingActionButton.onClick.RemoveListener(StartTrainingAction);
            trainingActionButton.onClick.AddListener(StartTrainingAction);
        }

        if (restActionButton != null)
        {
            restActionButton.onClick.RemoveListener(StartRestAction);
            restActionButton.onClick.AddListener(StartRestAction);
        }

        if (shopActionButton != null)
        {
            shopActionButton.onClick.RemoveListener(StartShopAction);
            shopActionButton.onClick.AddListener(StartShopAction);
        }

        if (playerInfoActionButton != null)
        {
            playerInfoActionButton.onClick.RemoveListener(StartPlayerInfoAction);
            playerInfoActionButton.onClick.AddListener(StartPlayerInfoAction);
        }

        if (matchActionButton != null)
        {
            matchActionButton.onClick.RemoveListener(StartMatchAction);
            matchActionButton.onClick.AddListener(StartMatchAction);
        }

        if (resultCloseButton != null)
        {
            resultCloseButton.onClick.RemoveListener(HandleResultClose);
            resultCloseButton.onClick.AddListener(HandleResultClose);
        }
    }

    private void Start()
    {
        ShowTitleScreen();
    }

    public int ConsumeActionPoint()
    {
        if (remainingActionPoints > 0)
        {
            remainingActionPoints--;
        }

        UpdateMainPanel();
        return remainingActionPoints;
    }

    public void CompleteTrainingAction()
    {
        PlayQuickFade(CompleteAction);
    }

    public void OnMatchEnded()
    {
        ApplyMatchRewardIfNeeded();
        PlayScreenTransition("결과 정리 중...", ShowMatchResult);
    }

    public void ContinueCareerFromTitle()
    {
        PlayScreenTransition("불러오는 중...", ContinueCareerFromTitleNow);
        SceneLoadManager.Instance.LoadScene(SceneEnums.MainScene);
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

    private void ContinueCareerFromTitleNow()
    {
        if (titleScreenUI != null)
        {
            titleScreenUI.Hide();
        }

        StartMainPhase();
    }

    public void StartNewGameSelection()
    {
        titleScreenCanvas.TryPositionMenu();
    }

    public void StartNewCareerFromTitle(POSITION selectedPosition, PlayerStats allocatedStats)
    {
        PlayScreenTransition("커리어 시작 중...", () =>
        {
            PlayerStats startingStats = BuildNewCareerStartingStats(selectedPosition, allocatedStats);
            ResetCareerSaveData(selectedPosition, startingStats);
            // ContinueCareerFromTitleNow();
            SceneLoadManager.Instance.LoadScene(SceneEnums.MainScene);
        });
    }

    public void StartNewCareerFromTitle(POSITION selectedPosition)
    {
        StartNewCareerFromTitle(selectedPosition, new PlayerStats());
    }

    public void QuitGameFromTitle()
    {
#if UNITY_EDITOR
        Debug.Log("에디터에서는 종료 버튼이 게임을 닫지 않습니다.");
#else
        Application.Quit();
#endif
    }

    private void ShowTitleScreen()
    {
        if (mainPanel != null)
        {
            mainPanel.SetActive(false);
        }

        if (trainingSelectionUI != null)
        {
            trainingSelectionUI.Hide();
        }

        SetResultPanelActive(false);

        if (titleScreenUI != null)
        {
            titleScreenUI.Show();
            return;
        }

        StartMainPhase();
    }

    private void StartMainPhase()
    {
        remainingActionPoints = Mathf.Max(0, maxActionPoints);
        ShowMainPanel();
    }

    private void ShowMainPanel()
    {
        if (titleScreenUI != null)
        {
            titleScreenUI.Hide();
        }

        if (trainingSelectionUI != null)
        {
            trainingSelectionUI.Hide();
        }

        if (mainPanel != null)
        {
            mainPanel.SetActive(true);
        }

        UpdateMainPanel();
    }

    private void StartTrainingAction()
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

        if (trainingSelectionUI != null)
        {
            trainingSelectionUI.StartTrainingAction(this);
        }
    }

    private void StartMatchPhase()
    {
        hasAppliedMatchReward = false;
        lastMatchRewardResult = new MatchRewardResult();

        if (mainPanel != null)
        {
            mainPanel.SetActive(false);
        }

        if (trainingSelectionUI != null)
        {
            trainingSelectionUI.Hide();
        }

        if (matchManager != null)
        {
            matchManager.StartMatch();
        }
    }

    private void UpdateMainPanel()
    {
        if (actionPointText != null)
        {
            int money = walletManager != null ? walletManager.CurrentMoney : 0;
            int year = careerCalendarManager != null ? careerCalendarManager.CurrentYear : 1;
            int month = careerCalendarManager != null ? careerCalendarManager.CurrentMonth : 11;
            int evaluationScore = PlayerSaveManager.LoadSeasonCoachEvaluationScore();

            actionPointText.text = $"마을\n행동력: {remainingActionPoints} / {maxActionPoints}\n돈: {money:N0}원\n날짜: {year}년 {month}월\n감독 평가점: {evaluationScore}";
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
    }

    private void StartRestAction()
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
        if (result.Success)
        {
            ConsumeActionPoint();
        }

        ShowActionResult(result);
    }

    private void CompleteAction()
    {
        ShowMainPanel();
    }

    private void StartShopAction()
    {
        PlayQuickFade(StartShopActionNow);
    }

    private void StartShopActionNow()
    {
        resultCloseAction = ResultCloseAction.ReturnToVillage;

        if (mainPanel != null)
        {
            mainPanel.SetActive(false);
        }

        SetResultPanelActive(true);

        if (resultText != null)
        {
            resultText.text = BuildShopText();
        }
    }

    private void StartPlayerInfoAction()
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

    private void StartMatchAction()
    {
        PlayScreenTransition("경기 준비 중...", StartMatchPhase);
    }

    private string BuildShopText()
    {
        int money = walletManager != null ? walletManager.CurrentMoney : 0;

        StringBuilder builder = new StringBuilder();
        builder.AppendLine("상점");
        builder.AppendLine();
        builder.AppendLine($"보유 돈: {money:N0}원");
        builder.AppendLine();
        builder.AppendLine("상점 기능은 준비 중입니다.");
        builder.AppendLine("나중에 장비, 훈련 보조 아이템, 회복 아이템 등을 여기에 추가할 수 있습니다.");
        return builder.ToString();
    }

    private string BuildPlayerInfoText()
    {
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

        if (!TryGetPlayerRole(out CharacterRole characterRole))
        {
            builder.AppendLine("플레이어 정보를 찾지 못했습니다.");
            return builder.ToString();
        }

        PlayerData playerData = characterRole.PlayerData;
        PlayerStats stats = playerData.Stats;
        builder.AppendLine($"이름: {playerData.PlayerName}");
        builder.AppendLine($"등번호: {playerData.Number}");
        builder.AppendLine($"포지션: {characterRole.Position}");
        builder.AppendLine();
        builder.AppendLine("능력치");
        builder.AppendLine($"파워 {stats.Power} / 점프 {stats.Jump} / 리시브 {stats.Receive}");
        builder.AppendLine($"토스 {stats.Set} / 블록 {stats.Block} / 서브 {stats.Serve}");
        builder.AppendLine($"스피드 {stats.Speed} / 체력 {stats.Stamina} / 멘탈 {stats.Mental}");
        return builder.ToString();
    }

    private RestResult ApplyRestAction()
    {
        RestResult result = new RestResult();

        if (!TryGetPlayerRole(out CharacterRole characterRole))
        {
            result.Success = false;
            return result;
        }

        PlayerData playerData = characterRole.PlayerData;
        PlayerStats stats = playerData.Stats;
        int staminaBefore = stats.Stamina;
        int mentalBefore = stats.Mental;
        int staminaIncrease = UnityEngine.Random.Range(MinRestIncrease, MaxRestIncreaseExclusive);
        int mentalIncrease = UnityEngine.Random.Range(MinRestIncrease, MaxRestIncreaseExclusive);

        stats.AddValue(PlayerStatType.Stamina, staminaIncrease, MaxStatValue);
        stats.AddValue(PlayerStatType.Mental, mentalIncrease, MaxStatValue);
        PlayerSaveManager.SavePlayerStats(playerData);

        result.Success = true;
        result.StaminaBefore = staminaBefore;
        result.StaminaAfter = stats.Stamina;
        result.MentalBefore = mentalBefore;
        result.MentalAfter = stats.Mental;
        return result;
    }

    private void ShowActionResult(RestResult result)
    {
        resultCloseAction = ResultCloseAction.CompleteAction;
        SetResultPanelActive(true);

        if (resultText == null)
        {
            return;
        }

        resultText.text = result.Success
            ? $"휴식 완료!\n체력 +{result.StaminaAfter - result.StaminaBefore} ({result.StaminaBefore} -> {result.StaminaAfter})\n멘탈 +{result.MentalAfter - result.MentalBefore} ({result.MentalBefore} -> {result.MentalAfter})\n남은 행동력: {remainingActionPoints} / {maxActionPoints}"
            : "플레이어 스탯을 찾지 못했습니다.";
    }

    private void ShowMatchResult()
    {
        resultCloseAction = ResultCloseAction.ReturnToMain;

        if (mainPanel != null)
        {
            mainPanel.SetActive(false);
        }

        if (trainingSelectionUI != null)
        {
            trainingSelectionUI.Hide();
        }

        SetResultPanelActive(true);

        if (resultText != null)
        {
            resultText.text = BuildMatchResultText();
        }
    }

    private string BuildMatchResultText()
    {
        MatchStats stats = matchManager != null ? matchManager.CurrentMatchStats : null;
        int playerScore = matchManager != null ? matchManager.PlayerScore : 0;
        int opponentScore = matchManager != null ? matchManager.OpponentScore : 0;

        StringBuilder builder = new StringBuilder();
        builder.AppendLine(playerScore > opponentScore ? "경기 승리!" : "경기 패배");
        builder.AppendLine($"점수: Player {playerScore} - {opponentScore} Opponent");
        builder.AppendLine();
        AppendMatchRewardText(builder, lastMatchRewardResult);
        builder.AppendLine();
        builder.AppendLine("선수별 기록");

        if (stats == null || stats.PlayerStats.Count == 0)
        {
            builder.AppendLine("기록된 선수 행동이 없습니다.");
            return builder.ToString();
        }

        foreach (PlayerMatchStats playerStats in stats.PlayerStats)
        {
            builder.AppendLine($"{playerStats.PlayerName} ({playerStats.TeamSide})");
            builder.AppendLine($"서브 {playerStats.Serves} / 리시브 {playerStats.Receives} / 토스 {playerStats.Sets} / 스파이크 {playerStats.Spikes}");
            builder.AppendLine($"블록 {playerStats.Blocks} / 득점 {playerStats.Points} / 공격성공 {playerStats.AttackSuccesses} / 공격실패 {playerStats.AttackFailures}");
            builder.AppendLine();
        }

        return builder.ToString();
    }

    private void ApplyMatchRewardIfNeeded()
    {
        if (hasAppliedMatchReward)
        {
            return;
        }

        lastMatchRewardResult = CalculateMatchReward();
        lastMatchRewardResult.SeasonEvaluationScore = PlayerSaveManager.AddSeasonCoachEvaluationScore(lastMatchRewardResult.MatchEvaluationScore);

        if (walletManager != null)
        {
            walletManager.AddMoney(lastMatchRewardResult.Allowance);
            lastMatchRewardResult.CurrentMoney = walletManager.CurrentMoney;
            lastMatchRewardResult.HasWallet = true;
        }
        else
        {
            Debug.LogWarning("WalletManager를 찾지 못해 출전 수당을 지급하지 못했습니다.");
        }

        hasAppliedMatchReward = true;
    }

    private MatchRewardResult CalculateMatchReward()
    {
        MatchStats stats = matchManager != null ? matchManager.CurrentMatchStats : null;
        int playerScore = matchManager != null ? matchManager.PlayerScore : 0;
        int opponentScore = matchManager != null ? matchManager.OpponentScore : 0;
        bool isWin = playerScore > opponentScore;
        int attackSuccesses = 0;
        int attackFailures = 0;

        if (stats != null)
        {
            foreach (PlayerMatchStats playerStats in stats.PlayerStats)
            {
                if (playerStats.TeamSide != TeamSide.Player)
                {
                    continue;
                }

                attackSuccesses += playerStats.AttackSuccesses;
                attackFailures += playerStats.AttackFailures;
            }
        }

        int resultScore = isWin ? WinEvaluationScore : LoseEvaluationScore;
        int scorePointScore = playerScore * ScorePointEvaluationScore;
        int attackSuccessScore = attackSuccesses * AttackSuccessEvaluationScore;
        int attackFailurePenalty = attackFailures * AttackFailureEvaluationPenalty;
        int matchEvaluationScore = Mathf.Max(0, resultScore + scorePointScore + attackSuccessScore - attackFailurePenalty);

        return new MatchRewardResult
        {
            IsWin = isWin,
            ResultScore = resultScore,
            ScorePointScore = scorePointScore,
            AttackSuccessScore = attackSuccessScore,
            AttackFailurePenalty = attackFailurePenalty,
            MatchEvaluationScore = matchEvaluationScore,
            Allowance = isWin ? WinAllowance : LoseAllowance
        };
    }

    private static void AppendMatchRewardText(StringBuilder builder, MatchRewardResult rewardResult)
    {
        builder.AppendLine("감독 평가");
        builder.AppendLine($"{(rewardResult.IsWin ? "승리" : "패배")} +{rewardResult.ResultScore}");
        builder.AppendLine($"득점 기여 +{rewardResult.ScorePointScore}");
        builder.AppendLine($"공격 성공 +{rewardResult.AttackSuccessScore}");

        if (rewardResult.AttackFailurePenalty > 0)
        {
            builder.AppendLine($"공격 실패 -{rewardResult.AttackFailurePenalty}");
        }

        builder.AppendLine();
        builder.AppendLine($"이번 경기 평가점: {rewardResult.MatchEvaluationScore}");
        builder.AppendLine($"시즌 누적 평가점: {rewardResult.SeasonEvaluationScore}");
        builder.AppendLine();
        builder.AppendLine("출전 수당");
        builder.AppendLine($"이번 경기 수당: {rewardResult.Allowance:N0}원");

        if (rewardResult.HasWallet)
        {
            builder.AppendLine($"현재 보유 돈: {rewardResult.CurrentMoney:N0}원");
        }
        else
        {
            builder.AppendLine("현재 보유 돈: 지갑 정보를 찾지 못했습니다.");
        }
    }

    private void HandleResultClose()
    {
        if (resultCloseAction == ResultCloseAction.ReturnToMain)
        {
            PlayScreenTransition("마을로 돌아가는 중...", HandleResultCloseNow);
            return;
        }

        PlayQuickFade(HandleResultCloseNow);
    }

    private void HandleResultCloseNow()
    {
        SetResultPanelActive(false);

        switch (resultCloseAction)
        {
            case ResultCloseAction.CompleteAction:
                CompleteAction();
                break;
            case ResultCloseAction.ReturnToVillage:
                ShowMainPanel();
                break;
            case ResultCloseAction.ReturnToMain:
                StartMainPhase();
                break;
        }

        resultCloseAction = ResultCloseAction.None;
    }

    private void CreateMissingMainControls()
    {
        if (trainingActionButton == null || mainPanel == null)
        {
            return;
        }

        RectTransform trainingTransform = trainingActionButton.GetComponent<RectTransform>();
        if (trainingTransform != null)
        {
            trainingTransform.anchoredPosition = new Vector2(0f, 72f);
        }

        if (restActionButton == null)
        {
            restActionButton = CreateVillageButton("RestActionButton", "휴식", -16f);
        }
        else
        {
            restActionButton.gameObject.SetActive(true);

            RectTransform restTransform = restActionButton.GetComponent<RectTransform>();
            if (restTransform != null)
            {
                restTransform.anchoredPosition = new Vector2(0f, -16f);
            }

            TMP_Text restButtonText = restActionButton.GetComponentInChildren<TMP_Text>(true);
            if (restButtonText != null)
            {
                restButtonText.text = "휴식";
            }
        }

        RectTransform mainPanelTransform = mainPanel.GetComponent<RectTransform>();
        if (mainPanelTransform != null)
        {
            mainPanelTransform.sizeDelta = new Vector2(mainPanelTransform.sizeDelta.x, 620f);
        }

        RectTransform actionPointTransform = actionPointText != null ? actionPointText.GetComponent<RectTransform>() : null;
        if (actionPointTransform != null)
        {
            actionPointTransform.anchoredPosition = new Vector2(0f, 184f);
            actionPointTransform.sizeDelta = new Vector2(460f, 156f);
        }

        if (shopActionButton == null)
        {
            shopActionButton = CreateVillageButton("ShopActionButton", "상점", -104f);
        }

        if (playerInfoActionButton == null)
        {
            playerInfoActionButton = CreateVillageButton("PlayerInfoActionButton", "내정보", -192f);
        }

        if (matchActionButton == null)
        {
            matchActionButton = CreateVillageButton("MatchActionButton", "경기 시작", -280f);
        }

        TMP_Text trainingButtonText = trainingActionButton.GetComponentInChildren<TMP_Text>(true);
        if (trainingButtonText != null)
        {
            trainingButtonText.text = "훈련";
        }
    }

    private Button CreateVillageButton(string buttonName, string buttonText, float yPosition)
    {
        Button button = Instantiate(trainingActionButton, mainPanel.transform);
        button.name = buttonName;
        button.onClick.RemoveAllListeners();

        RectTransform buttonTransform = button.GetComponent<RectTransform>();
        if (buttonTransform != null)
        {
            buttonTransform.anchoredPosition = new Vector2(0f, yPosition);
        }

        TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);
        if (text != null)
        {
            text.text = buttonText;
        }

        return button;
    }

    private void CreateMissingResultPanel()
    {
        if (resultPanel != null || mainPanel == null)
        {
            SetResultPanelActive(false);
            return;
        }

        resultPanel = new GameObject("GameFlowResultPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        resultPanel.transform.SetParent(mainPanel.transform.parent, false);

        RectTransform panelTransform = resultPanel.GetComponent<RectTransform>();
        panelTransform.anchorMin = new Vector2(0.5f, 0.5f);
        panelTransform.anchorMax = new Vector2(0.5f, 0.5f);
        panelTransform.anchoredPosition = Vector2.zero;
        panelTransform.sizeDelta = new Vector2(620f, 520f);

        Image panelImage = resultPanel.GetComponent<Image>();
        panelImage.color = new Color(0.08f, 0.1f, 0.12f, 0.94f);

        GameObject textObject = new GameObject("GameFlowResultText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(resultPanel.transform, false);
        RectTransform textTransform = textObject.GetComponent<RectTransform>();
        textTransform.anchorMin = new Vector2(0f, 0f);
        textTransform.anchorMax = new Vector2(1f, 1f);
        textTransform.offsetMin = new Vector2(32f, 112f);
        textTransform.offsetMax = new Vector2(-32f, -32f);

        resultText = textObject.GetComponent<TMP_Text>();
        resultText.color = Color.white;
        if (actionPointText != null)
        {
            resultText.font = actionPointText.font;
            resultText.fontSharedMaterial = actionPointText.fontSharedMaterial;
        }

        resultText.fontSize = 26f;
        resultText.enableAutoSizing = true;
        resultText.fontSizeMin = 14f;
        resultText.fontSizeMax = 28f;
        resultText.alignment = TextAlignmentOptions.TopLeft;

        resultCloseButton = Instantiate(trainingActionButton, resultPanel.transform);
        resultCloseButton.name = "GameFlowResultCloseButton";
        resultCloseButton.onClick.RemoveAllListeners();

        RectTransform buttonTransform = resultCloseButton.GetComponent<RectTransform>();
        if (buttonTransform != null)
        {
            buttonTransform.anchorMin = new Vector2(0.5f, 0f);
            buttonTransform.anchorMax = new Vector2(0.5f, 0f);
            buttonTransform.anchoredPosition = new Vector2(0f, 48f);
            buttonTransform.sizeDelta = new Vector2(240f, 64f);
        }

        TMP_Text closeButtonText = resultCloseButton.GetComponentInChildren<TMP_Text>(true);
        if (closeButtonText != null)
        {
            closeButtonText.text = "확인";
        }

        SetResultPanelActive(false);
    }

    private void SetResultPanelActive(bool isActive)
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(isActive);
        }
    }

    private void EnsureScreenTransitionUI()
    {
        if (screenTransitionUI == null)
        {
            screenTransitionUI = FindFirstObjectByType<ScreenTransitionUI>();
        }

        if (screenTransitionUI == null)
        {
            screenTransitionUI = gameObject.AddComponent<ScreenTransitionUI>();
        }

        screenTransitionUI.Initialize(GetUiRoot(), actionPointText);
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

    private Transform GetUiRoot()
    {
        if (mainPanel != null && mainPanel.transform.parent != null)
        {
            return mainPanel.transform.parent;
        }

        if (trainingSelectionUI != null)
        {
            return trainingSelectionUI.transform;
        }

        return null;
    }

    private void ResetCareerSaveData(POSITION selectedPosition, PlayerStats startingStats)
    {
        ResolveCareerManagers();

        if (walletManager != null)
        {
            walletManager.ResetMoney();
        }

        if (careerCalendarManager != null)
        {
            careerCalendarManager.ResetCareerCalendar();
        }

        PlayerSaveManager.ResetSeasonCoachEvaluationScore();
        PlayerSaveManager.SaveSelectedPosition(selectedPosition);

        CharacterRole[] characterRoles = FindObjectsByType<CharacterRole>(FindObjectsSortMode.None);
        foreach (CharacterRole characterRole in characterRoles)
        {
            characterRole.ResetSavedStatsToNewCareer(selectedPosition, startingStats);
        }
    }

    private static PlayerStats BuildNewCareerStartingStats(POSITION selectedPosition, PlayerStats allocatedStats)
    {
        PlayerStats startingStats = CreateBaseStartingStats();
        int remainingBonusPoints = StartingBonusPoints;

        if (allocatedStats != null)
        {
            foreach (PlayerStatType statType in StartingStatTypes)
            {
                int maxAllocatable = InitialStatCap - startingStats.GetValue(statType);
                int requestedAmount = allocatedStats.GetValue(statType);
                int appliedAmount = Mathf.Clamp(requestedAmount, 0, Mathf.Min(maxAllocatable, remainingBonusPoints));

                startingStats.AddValue(statType, appliedAmount, InitialStatCap);
                remainingBonusPoints -= appliedAmount;
            }
        }

        ApplyPositionBonus(startingStats, selectedPosition);
        return startingStats;
    }

    private static PlayerStats CreateBaseStartingStats()
    {
        PlayerStats stats = new PlayerStats();

        foreach (PlayerStatType statType in StartingStatTypes)
        {
            stats.SetValue(statType, StartingStatValue);
        }

        return stats;
    }

    private static void ApplyPositionBonus(PlayerStats stats, POSITION position)
    {
        switch (position)
        {
            case POSITION.OutsideHitter:
                AddStartingBonus(stats, PlayerStatType.Power, 4);
                AddStartingBonus(stats, PlayerStatType.Jump, 3);
                AddStartingBonus(stats, PlayerStatType.Receive, 3);
                break;
            case POSITION.OppositeHitter:
                AddStartingBonus(stats, PlayerStatType.Power, 5);
                AddStartingBonus(stats, PlayerStatType.Serve, 3);
                AddStartingBonus(stats, PlayerStatType.Jump, 3);
                break;
            case POSITION.MiddleBlocker:
                AddStartingBonus(stats, PlayerStatType.Block, 5);
                AddStartingBonus(stats, PlayerStatType.Jump, 4);
                AddStartingBonus(stats, PlayerStatType.Power, 3);
                break;
            case POSITION.Setter:
                AddStartingBonus(stats, PlayerStatType.Set, 5);
                AddStartingBonus(stats, PlayerStatType.Mental, 4);
                AddStartingBonus(stats, PlayerStatType.Speed, 3);
                break;
            case POSITION.Libero:
                AddStartingBonus(stats, PlayerStatType.Receive, 5);
                AddStartingBonus(stats, PlayerStatType.Speed, 4);
                AddStartingBonus(stats, PlayerStatType.Mental, 3);
                break;
            case POSITION.DefensiveSpecialist:
                AddStartingBonus(stats, PlayerStatType.Receive, 4);
                AddStartingBonus(stats, PlayerStatType.Speed, 4);
                AddStartingBonus(stats, PlayerStatType.Stamina, 3);
                break;
        }
    }

    private static void AddStartingBonus(PlayerStats stats, PlayerStatType statType, int amount)
    {
        stats.AddValue(statType, amount, MaxStatValue);
    }

    private void ResolveCareerManagers()
    {
        if (walletManager == null)
        {
            walletManager = FindFirstObjectByType<WalletManager>();
        }

        if (careerCalendarManager == null)
        {
            careerCalendarManager = FindFirstObjectByType<CareerCalendarManager>();
        }
    }

    private static bool TryGetPlayerRole(out CharacterRole characterRole)
    {
        characterRole = null;

        Player player = FindFirstObjectByType<Player>();
        if (player == null)
        {
            return false;
        }

        characterRole = player.GetComponent<CharacterRole>();
        if (characterRole == null || characterRole.PlayerData == null || characterRole.PlayerData.Stats == null)
        {
            return false;
        }

        return true;
    }

    private struct RestResult
    {
        public bool Success;
        public int StaminaBefore;
        public int StaminaAfter;
        public int MentalBefore;
        public int MentalAfter;
    }

    private struct MatchRewardResult
    {
        public bool IsWin;
        public bool HasWallet;
        public int ResultScore;
        public int ScorePointScore;
        public int AttackSuccessScore;
        public int AttackFailurePenalty;
        public int MatchEvaluationScore;
        public int SeasonEvaluationScore;
        public int Allowance;
        public int CurrentMoney;
    }
}
