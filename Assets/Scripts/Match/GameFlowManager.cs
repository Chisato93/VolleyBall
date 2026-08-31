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

    [SerializeField] private int maxActionPoints = 3;
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private TMP_Text actionPointText;
    [SerializeField] private Button trainingActionButton;
    [SerializeField] private Button restActionButton;
    [SerializeField] private MatchManager matchManager;
    [SerializeField] private TrainingSelectionUI trainingSelectionUI;
    [SerializeField] private WalletManager walletManager;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private Button resultCloseButton;

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

        CreateMissingMainControls();
        CreateMissingResultPanel();

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

        if (resultCloseButton != null)
        {
            resultCloseButton.onClick.RemoveListener(HandleResultClose);
            resultCloseButton.onClick.AddListener(HandleResultClose);
        }
    }

    private void Start()
    {
        StartMainPhase();
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
        CompleteAction();
    }

    public void OnMatchEnded()
    {
        ApplyMatchRewardIfNeeded();
        ShowMatchResult();
    }

    private void StartMainPhase()
    {
        remainingActionPoints = Mathf.Max(0, maxActionPoints);
        ShowMainPanel();
    }

    private void ShowMainPanel()
    {
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
            StartMatchPhase();
            return;
        }

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
            actionPointText.text = $"행동력: {remainingActionPoints} / {maxActionPoints}";
        }

        if (trainingActionButton != null)
        {
            trainingActionButton.interactable = remainingActionPoints > 0;
        }

        if (restActionButton != null)
        {
            restActionButton.interactable = remainingActionPoints > 0;
        }
    }

    private void StartRestAction()
    {
        if (remainingActionPoints <= 0)
        {
            StartMatchPhase();
            return;
        }

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
        if (remainingActionPoints > 0)
        {
            ShowMainPanel();
            return;
        }

        StartMatchPhase();
    }

    private RestResult ApplyRestAction()
    {
        RestResult result = new RestResult();

        if (!TryGetPlayerData(out PlayerData playerData))
        {
            result.Success = false;
            return result;
        }

        PlayerStats stats = playerData.Stats;
        int staminaBefore = stats.Stamina;
        int mentalBefore = stats.Mental;
        int staminaIncrease = Random.Range(MinRestIncrease, MaxRestIncreaseExclusive);
        int mentalIncrease = Random.Range(MinRestIncrease, MaxRestIncreaseExclusive);

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
        SetResultPanelActive(false);

        switch (resultCloseAction)
        {
            case ResultCloseAction.CompleteAction:
                CompleteAction();
                break;
            case ResultCloseAction.ReturnToMain:
                StartMainPhase();
                break;
        }

        resultCloseAction = ResultCloseAction.None;
    }

    private void CreateMissingMainControls()
    {
        if (restActionButton != null || trainingActionButton == null || mainPanel == null)
        {
            return;
        }

        restActionButton = Instantiate(trainingActionButton, mainPanel.transform);
        restActionButton.name = "RestActionButton";
        restActionButton.onClick.RemoveAllListeners();

        RectTransform restTransform = restActionButton.GetComponent<RectTransform>();
        RectTransform trainingTransform = trainingActionButton.GetComponent<RectTransform>();
        if (restTransform != null && trainingTransform != null)
        {
            restTransform.anchoredPosition = trainingTransform.anchoredPosition + new Vector2(0f, -88f);
        }

        RectTransform mainPanelTransform = mainPanel.GetComponent<RectTransform>();
        if (mainPanelTransform != null)
        {
            mainPanelTransform.sizeDelta = new Vector2(mainPanelTransform.sizeDelta.x, 388f);
        }

        TMP_Text restButtonText = restActionButton.GetComponentInChildren<TMP_Text>(true);
        if (restButtonText != null)
        {
            restButtonText.text = "휴식";
        }

        TMP_Text trainingButtonText = trainingActionButton.GetComponentInChildren<TMP_Text>(true);
        if (trainingButtonText != null)
        {
            trainingButtonText.text = "훈련";
        }
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

    private static bool TryGetPlayerData(out PlayerData playerData)
    {
        playerData = null;

        Player player = FindFirstObjectByType<Player>();
        if (player == null)
        {
            return false;
        }

        CharacterRole characterRole = player.GetComponent<CharacterRole>();
        if (characterRole == null || characterRole.PlayerData == null || characterRole.PlayerData.Stats == null)
        {
            return false;
        }

        playerData = characterRole.PlayerData;
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
