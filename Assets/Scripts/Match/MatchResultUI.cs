using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MatchResultUI : MonoBehaviour
{
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

    private const int WinEvaluationScore = 30;
    private const int LoseEvaluationScore = 10;
    private const int ScorePointEvaluationScore = 2;
    private const int AttackSuccessEvaluationScore = 2;
    private const int AttackFailureEvaluationPenalty = 1;
    private const int WinAllowance = 100;
    private const int LoseAllowance = 30;

    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private Button closeButton;

    private bool hasShownResult;

    public void Show(MatchManager matchManager)
    {
        if (hasShownResult)
        {
            return;
        }

        hasShownResult = true;
        EnsureUi();

        if (resultText != null)
        {
            MatchRewardResult rewardResult = ApplyReward(matchManager);
            resultText.text = BuildResultText(matchManager, rewardResult);
        }

        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }
    }

    private void EnsureUi()
    {
        if (resultPanel != null && resultText != null && closeButton != null)
        {
            return;
        }

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("MatchResultCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
        }

        resultPanel = new GameObject("MatchResultPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        resultPanel.transform.SetParent(canvas.transform, false);

        RectTransform panelTransform = resultPanel.GetComponent<RectTransform>();
        panelTransform.anchorMin = new Vector2(0.5f, 0.5f);
        panelTransform.anchorMax = new Vector2(0.5f, 0.5f);
        panelTransform.anchoredPosition = Vector2.zero;
        panelTransform.sizeDelta = new Vector2(680f, 620f);

        Image panelImage = resultPanel.GetComponent<Image>();
        panelImage.color = new Color(0.08f, 0.1f, 0.12f, 0.94f);

        GameObject textObject = new GameObject("MatchResultText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(resultPanel.transform, false);

        RectTransform textTransform = textObject.GetComponent<RectTransform>();
        textTransform.anchorMin = Vector2.zero;
        textTransform.anchorMax = Vector2.one;
        textTransform.offsetMin = new Vector2(32f, 116f);
        textTransform.offsetMax = new Vector2(-32f, -32f);

        resultText = textObject.GetComponent<TMP_Text>();
        resultText.color = Color.white;
        resultText.fontSize = 24f;
        resultText.enableAutoSizing = true;
        resultText.fontSizeMin = 14f;
        resultText.fontSizeMax = 26f;
        resultText.alignment = TextAlignmentOptions.TopLeft;

        closeButton = CreateCloseButton(resultPanel.transform);
        closeButton.onClick.AddListener(ReturnToMainScene);
        resultPanel.SetActive(false);
    }

    private Button CreateCloseButton(Transform parent)
    {
        GameObject buttonObject = new GameObject("ReturnToMainButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform buttonTransform = buttonObject.GetComponent<RectTransform>();
        buttonTransform.anchorMin = new Vector2(0.5f, 0f);
        buttonTransform.anchorMax = new Vector2(0.5f, 0f);
        buttonTransform.anchoredPosition = new Vector2(0f, 48f);
        buttonTransform.sizeDelta = new Vector2(240f, 64f);

        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.color = new Color(0.9f, 0.92f, 0.95f, 1f);

        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(buttonObject.transform, false);

        RectTransform textTransform = textObject.GetComponent<RectTransform>();
        textTransform.anchorMin = Vector2.zero;
        textTransform.anchorMax = Vector2.one;
        textTransform.offsetMin = Vector2.zero;
        textTransform.offsetMax = Vector2.zero;

        TMP_Text buttonText = textObject.GetComponent<TMP_Text>();
        buttonText.text = "마을로";
        buttonText.color = Color.black;
        buttonText.fontSize = 28f;
        buttonText.alignment = TextAlignmentOptions.Center;

        return buttonObject.GetComponent<Button>();
    }

    private MatchRewardResult ApplyReward(MatchManager matchManager)
    {
        MatchRewardResult rewardResult = CalculateReward(matchManager);
        rewardResult.SeasonEvaluationScore = PlayerSaveManager.AddSeasonCoachEvaluationScore(rewardResult.MatchEvaluationScore);

        WalletManager walletManager = WalletManager.Instance != null ? WalletManager.Instance : FindFirstObjectByType<WalletManager>();
        if (walletManager != null)
        {
            walletManager.AddMoney(rewardResult.Allowance);
            rewardResult.CurrentMoney = walletManager.CurrentMoney;
            rewardResult.HasWallet = true;
        }
        else
        {
            Debug.LogWarning("WalletManager를 찾지 못해서 경기 수당을 지급하지 못했습니다.");
        }

        return rewardResult;
    }

    private static MatchRewardResult CalculateReward(MatchManager matchManager)
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

        return new MatchRewardResult
        {
            IsWin = isWin,
            ResultScore = resultScore,
            ScorePointScore = scorePointScore,
            AttackSuccessScore = attackSuccessScore,
            AttackFailurePenalty = attackFailurePenalty,
            MatchEvaluationScore = Mathf.Max(0, resultScore + scorePointScore + attackSuccessScore - attackFailurePenalty),
            Allowance = isWin ? WinAllowance : LoseAllowance
        };
    }

    private static string BuildResultText(MatchManager matchManager, MatchRewardResult rewardResult)
    {
        MatchStats stats = matchManager != null ? matchManager.CurrentMatchStats : null;
        int playerScore = matchManager != null ? matchManager.PlayerScore : 0;
        int opponentScore = matchManager != null ? matchManager.OpponentScore : 0;

        StringBuilder builder = new StringBuilder();
        builder.AppendLine(playerScore > opponentScore ? "경기 승리!" : "경기 패배");
        builder.AppendLine($"점수: Player {playerScore} - {opponentScore} Opponent");
        builder.AppendLine();
        AppendRewardText(builder, rewardResult);
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

    private static void AppendRewardText(StringBuilder builder, MatchRewardResult rewardResult)
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
        builder.AppendLine(rewardResult.HasWallet
            ? $"현재 보유 돈: {rewardResult.CurrentMoney:N0}원"
            : "현재 보유 돈: 지갑 정보를 찾지 못했습니다.");
    }

    private static void ReturnToMainScene()
    {
        if (SceneLoadManager.Instance != null)
        {
            SceneLoadManager.Instance.LoadScene(SceneEnums.MainScene);
            return;
        }

        SceneManager.LoadScene((int)SceneEnums.MainScene);
    }
}
