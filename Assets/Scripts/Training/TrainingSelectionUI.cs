using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrainingSelectionUI : MonoBehaviour
{
    private const int MinTrainingIncrease = 1;
    private const int MaxTrainingIncreaseExclusive = 4;
    private const int MaxStatValue = 99;

    private static readonly string[] TrainingNames =
    {
        "파워 훈련",
        "점프 훈련",
        "리시브 훈련",
        "토스 훈련",
        "블로킹 훈련",
        "서브 훈련",
        "스피드 훈련",
        "체력 훈련",
        "멘탈 훈련",
        "종합 훈련"
    };

    private static readonly PlayerStatType[] AllStatTypes =
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

    [SerializeField] private GameObject panelObject;
    [SerializeField] private Button[] optionButtons;
    [SerializeField] private TMP_Text [] optionTexts;
    [SerializeField] private GameObject resultContainer;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private Button resultCloseButton;

    private void Awake()
    {
        if (resultCloseButton != null)
        {
            resultCloseButton.onClick.RemoveListener(Hide);
            resultCloseButton.onClick.AddListener(Hide);
        }

        SetResultActive(false);
        Hide();
    }

    public void ShowRandomOptions()
    {
        if (panelObject == null || optionButtons == null || optionTexts == null)
        {
            return;
        }

        List<string> options = PickRandomTrainings(3);
        int optionCount = Mathf.Min(optionButtons.Length, optionTexts.Length, options.Count);

        SetOptionButtonsActive(true);
        SetResultActive(false);

        for (int i = 0; i < optionCount; i++)
        {
            string trainingName = options[i];
            optionTexts[i].text = trainingName;
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => SelectTraining(trainingName));
        }

        panelObject.SetActive(true);
    }

    public void Hide()
    {
        if (panelObject != null)
        {
            panelObject.SetActive(false);
        }
    }

    private void SelectTraining(string trainingName)
    {
        TrainingResult result = ApplyTraining(trainingName);
        ShowResult(result);
    }

    private TrainingResult ApplyTraining(string trainingName)
    {
        if (!TryGetPlayerData(out PlayerData playerData))
        {
            Debug.LogWarning($"Training skipped. Player data was not found: {trainingName}");
            return TrainingResult.Failed(trainingName);
        }

        PlayerStatType statType = GetTrainingStatType(trainingName);
        int amount = Random.Range(MinTrainingIncrease, MaxTrainingIncreaseExclusive);
        PlayerStats stats = playerData.Stats;
        int beforeValue = stats.GetValue(statType);

        stats.AddValue(statType, amount, MaxStatValue);
        playerData.SaveAssetIfEditor();

        int afterValue = stats.GetValue(statType);
        int actualIncrease = afterValue - beforeValue;
        Debug.Log($"Training result: {trainingName} / {statType} +{actualIncrease} ({beforeValue} -> {afterValue})");

        return TrainingResult.Succeeded(trainingName, statType, actualIncrease, beforeValue, afterValue);
    }

    private void ShowResult(TrainingResult result)
    {
        SetOptionButtonsActive(false);
        SetResultActive(true);

        if (resultText == null)
        {
            Debug.LogWarning("Training result text is not assigned.");
            return;
        }

        resultText.text = result.Success
            ? $"{result.TrainingName} 완료!\n{result.StatType} +{result.Increase}\n{result.BeforeValue} -> {result.AfterValue}"
            : "플레이어 스탯을 찾지 못했습니다.";
    }

    private void SetOptionButtonsActive(bool isActive)
    {
        if (optionButtons == null)
        {
            return;
        }

        foreach (Button optionButton in optionButtons)
        {
            if (optionButton != null)
            {
                optionButton.gameObject.SetActive(isActive);
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

    private static PlayerStatType GetTrainingStatType(string trainingName)
    {
        switch (trainingName)
        {
            case "파워 훈련":
                return PlayerStatType.Power;
            case "점프 훈련":
                return PlayerStatType.Jump;
            case "리시브 훈련":
                return PlayerStatType.Receive;
            case "토스 훈련":
                return PlayerStatType.Set;
            case "블로킹 훈련":
                return PlayerStatType.Block;
            case "서브 훈련":
                return PlayerStatType.Serve;
            case "스피드 훈련":
                return PlayerStatType.Speed;
            case "체력 훈련":
                return PlayerStatType.Stamina;
            case "멘탈 훈련":
                return PlayerStatType.Mental;
            case "종합 훈련":
                return AllStatTypes[Random.Range(0, AllStatTypes.Length)];
            default:
                Debug.LogWarning($"Unknown training selected: {trainingName}");
                return PlayerStatType.Power;
        }
    }

    private static List<string> PickRandomTrainings(int count)
    {
        List<string> pool = new List<string>(TrainingNames);
        List<string> selected = new List<string>();

        while (selected.Count < count && pool.Count > 0)
        {
            int index = Random.Range(0, pool.Count);
            selected.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return selected;
    }

    private struct TrainingResult
    {
        public bool Success;
        public string TrainingName;
        public PlayerStatType StatType;
        public int Increase;
        public int BeforeValue;
        public int AfterValue;

        public static TrainingResult Succeeded(
            string trainingName,
            PlayerStatType statType,
            int increase,
            int beforeValue,
            int afterValue)
        {
            return new TrainingResult
            {
                Success = true,
                TrainingName = trainingName,
                StatType = statType,
                Increase = increase,
                BeforeValue = beforeValue,
                AfterValue = afterValue
            };
        }

        public static TrainingResult Failed(string trainingName)
        {
            return new TrainingResult
            {
                Success = false,
                TrainingName = trainingName
            };
        }
    }
}
