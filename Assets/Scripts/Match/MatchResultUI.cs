using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MatchResultUI : MonoBehaviour
{
    private struct MatchRewardResult
    {
        public bool HasWallet;
        public int MatchEvaluationScore;
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
    [SerializeField] private ScrollRect resultScrollRect;

    private bool hasShownResult;

    public void Show(MatchManager matchManager)
    {
        if (hasShownResult)
        {
            return;
        }

        hasShownResult = true;
        MatchRewardResult rewardResult = ApplyReward(matchManager);
        MatchHistoryStore.SaveMatch(matchManager, rewardResult.MatchEvaluationScore,
            rewardResult.HasWallet ? rewardResult.Allowance : 0);
        PlayerSaveManager.SaveCheckpoint();
        EnsureUi();

        if (resultText != null)
        {
            resultText.text = BuildResultText(matchManager, rewardResult);
        }

        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }

        if (resultScrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            resultScrollRect.StopMovement();
            resultScrollRect.verticalNormalizedPosition = 1f;
        }
    }

    private void EnsureUi()
    {
        TryFindSceneUi();

        if (resultPanel != null && resultText != null && closeButton != null)
        {
            closeButton.onClick.RemoveListener(ReturnToMainScene);
            closeButton.onClick.AddListener(ReturnToMainScene);
            return;
        }

        Debug.LogWarning("MatchResultUI가 씬에 미리 배치된 결과 UI를 찾지 못했습니다. MatchResultPanel, MatchResultText, ReturnToMainButton을 확인하세요.");
    }

    private void TryFindSceneUi()
    {
        if (resultPanel == null)
        {
            Transform panelTransform = FindChildTransform(transform, "MatchResultPanel");
            if (panelTransform == null)
            {
                panelTransform = FindSceneTransform("MatchResultPanel");
            }

            resultPanel = panelTransform != null ? panelTransform.gameObject : null;
        }

        if (resultText == null && resultPanel != null)
        {
            resultText = FindChildComponent<TMP_Text>(resultPanel.transform, "MatchResultText");
        }

        if (closeButton == null && resultPanel != null)
        {
            closeButton = FindChildComponent<Button>(resultPanel.transform, "ReturnToMainButton");
        }

        if (resultScrollRect == null && resultPanel != null)
        {
            resultScrollRect = FindChildComponent<ScrollRect>(resultPanel.transform, "ResultScrollView");
        }
    }

    private static Transform FindSceneTransform(string objectName)
    {
        GameObject foundObject = GameObject.Find(objectName);
        return foundObject != null ? foundObject.transform : null;
    }

    private static Transform FindChildTransform(Transform root, string objectName)
    {
        if (root == null)
        {
            return null;
        }

        Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in transforms)
        {
            if (child.name == objectName)
            {
                return child;
            }
        }

        return null;
    }

    private static T FindChildComponent<T>(Transform root, string objectName) where T : Component
    {
        if (root == null)
        {
            return null;
        }

        T[] components = root.GetComponentsInChildren<T>(true);
        foreach (T component in components)
        {
            if (component.name == objectName)
            {
                return component;
            }
        }

        return null;
    }

    private MatchRewardResult ApplyReward(MatchManager matchManager)
    {
        MatchRewardResult rewardResult = CalculateReward(matchManager);
        PlayerSaveManager.AddSeasonCoachEvaluationScore(rewardResult.MatchEvaluationScore);

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
            MatchEvaluationScore = stats?.CommentaryEvaluationScore
                ?? Mathf.Max(0, resultScore + scorePointScore + attackSuccessScore - attackFailurePenalty),
            Allowance = isWin ? WinAllowance : LoseAllowance
        };
    }

    private static string BuildResultText(MatchManager matchManager, MatchRewardResult rewardResult)
    {
        int playerScore = matchManager != null ? matchManager.PlayerScore : 0;
        int opponentScore = matchManager != null ? matchManager.OpponentScore : 0;

        StringBuilder builder = new StringBuilder();
        builder.AppendLine(playerScore > opponentScore ? "경기 승리!" : "경기 패배");
        builder.AppendLine($"점수: Player {playerScore} - {opponentScore} Opponent");
        builder.AppendLine();
        AppendRewardText(builder, rewardResult);

        return builder.ToString();
    }

    private static void AppendRewardText(StringBuilder builder, MatchRewardResult rewardResult)
    {
        builder.AppendLine($"이번 경기 평가점: {rewardResult.MatchEvaluationScore}점");
        builder.AppendLine();
        builder.AppendLine("출전 수당");
        builder.AppendLine($"이번 경기 수당: {rewardResult.Allowance:N0}원");
        builder.AppendLine(rewardResult.HasWallet
            ? $"현재 보유 돈: {rewardResult.CurrentMoney:N0}원"
            : "현재 보유 돈: 지갑 정보를 찾지 못했습니다.");
    }

    private static void ReturnToMainScene()
    {
        PlayerSaveManager.SaveCheckpoint();
        if (SceneLoadManager.Instance != null)
        {
            SceneLoadManager.Instance.LoadScene(SceneEnums.MainScene);
            return;
        }

        SceneManager.LoadScene((int)SceneEnums.MainScene);
    }
}
