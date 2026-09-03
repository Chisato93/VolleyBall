using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TitleScreenUI : MonoBehaviour
{
    private const int StartingStatValue = 20;
    private const int StartingBonusPoints = 10;
    private const int InitialStatCap = 30;

    private static readonly PlayerStatType[] StatTypes =
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

    private GameFlowManager gameFlowManager;
    private GameObject titlePanel;
    private GameObject positionPanel;
    private GameObject allocationPanel;
    private TMP_Text fontSource;
    private TMP_Text allocationTitleText;
    private TMP_Text remainingPointText;
    private Button startCareerButton;
    private StatAllocationRow[] statRows;
    private POSITION selectedPosition;
    private PlayerStats allocatedStats = new PlayerStats();

    public void Initialize(GameFlowManager owner, Transform uiRoot, TMP_Text sourceText)
    {
        gameFlowManager = owner;
        fontSource = sourceText;

        if (titlePanel == null)
        {
            CreateTitlePanel(uiRoot);
        }

        if (positionPanel == null)
        {
            CreatePositionPanel(uiRoot);
        }

        if (allocationPanel == null)
        {
            CreateAllocationPanel(uiRoot);
        }
    }

    public void Show()
    {
        if (titlePanel != null)
        {
            titlePanel.SetActive(true);
            titlePanel.transform.SetAsLastSibling();
        }

        if (positionPanel != null)
        {
            positionPanel.SetActive(false);
        }

        if (allocationPanel != null)
        {
            allocationPanel.SetActive(false);
        }
    }

    public void Hide()
    {
        if (titlePanel != null)
        {
            titlePanel.SetActive(false);
        }

        if (positionPanel != null)
        {
            positionPanel.SetActive(false);
        }

        if (allocationPanel != null)
        {
            allocationPanel.SetActive(false);
        }
    }

    private void CreateTitlePanel(Transform uiRoot)
    {
        if (uiRoot == null)
        {
            Debug.LogWarning("타이틀 화면을 만들 UI 루트를 찾지 못했습니다.");
            return;
        }

        titlePanel = CreateFullscreenPanel(uiRoot, "TitleScreenPanel");

        TMP_Text titleText = CreateText("TitleText", "VOLLEYBALL", 72f, TextAlignmentOptions.Center);
        RectTransform titleTransform = titleText.GetComponent<RectTransform>();
        titleTransform.anchorMin = new Vector2(0.5f, 0.5f);
        titleTransform.anchorMax = new Vector2(0.5f, 0.5f);
        titleTransform.anchoredPosition = new Vector2(0f, 210f);
        titleTransform.sizeDelta = new Vector2(760f, 96f);

        TMP_Text subtitleText = CreateText("SubtitleText", "선수 육성 모드", 32f, TextAlignmentOptions.Center);
        RectTransform subtitleTransform = subtitleText.GetComponent<RectTransform>();
        subtitleTransform.anchorMin = new Vector2(0.5f, 0.5f);
        subtitleTransform.anchorMax = new Vector2(0.5f, 0.5f);
        subtitleTransform.anchoredPosition = new Vector2(0f, 132f);
        subtitleTransform.sizeDelta = new Vector2(560f, 60f);

        CreateButton("ContinueButton", "이어하기", new Vector2(0f, 30f), HandleContinue);
        CreateButton("NewGameButton", "새 게임", new Vector2(0f, -56f), HandleNewGame);
        CreateButton("QuitButton", "종료", new Vector2(0f, -142f), HandleQuit);

        titlePanel.SetActive(false);
    }

    private void CreatePositionPanel(Transform uiRoot)
    {
        if (uiRoot == null)
        {
            return;
        }

        positionPanel = CreateFullscreenPanel(uiRoot, "PositionSelectPanel");

        TMP_Text titleText = CreateText(positionPanel, "PositionTitleText", "포지션 선택", 54f, TextAlignmentOptions.Center);
        RectTransform titleTransform = titleText.GetComponent<RectTransform>();
        titleTransform.anchorMin = new Vector2(0.5f, 0.5f);
        titleTransform.anchorMax = new Vector2(0.5f, 0.5f);
        titleTransform.anchoredPosition = new Vector2(0f, 260f);
        titleTransform.sizeDelta = new Vector2(680f, 80f);

        TMP_Text guideText = CreateText(positionPanel, "PositionGuideText", "새 커리어에서 맡을 포지션을 선택하세요.", 26f, TextAlignmentOptions.Center);
        RectTransform guideTransform = guideText.GetComponent<RectTransform>();
        guideTransform.anchorMin = new Vector2(0.5f, 0.5f);
        guideTransform.anchorMax = new Vector2(0.5f, 0.5f);
        guideTransform.anchoredPosition = new Vector2(0f, 196f);
        guideTransform.sizeDelta = new Vector2(760f, 48f);

        CreatePositionButton("OutsideHitterButton", "아웃사이드 히터", POSITION.OutsideHitter, new Vector2(-190f, 92f));
        CreatePositionButton("OppositeHitterButton", "아포짓 히터", POSITION.OppositeHitter, new Vector2(190f, 92f));
        CreatePositionButton("MiddleBlockerButton", "미들 블로커", POSITION.MiddleBlocker, new Vector2(-190f, 8f));
        CreatePositionButton("SetterButton", "세터", POSITION.Setter, new Vector2(190f, 8f));
        CreatePositionButton("LiberoButton", "리베로", POSITION.Libero, new Vector2(-190f, -76f));
        CreatePositionButton("DefensiveSpecialistButton", "디펜시브 스페셜리스트", POSITION.DefensiveSpecialist, new Vector2(190f, -76f));
        CreateButton(positionPanel, "BackButton", "뒤로", new Vector2(0f, -190f), HandleBackFromPositionSelect);

        positionPanel.SetActive(false);
    }

    private void CreateAllocationPanel(Transform uiRoot)
    {
        if (uiRoot == null)
        {
            return;
        }

        allocationPanel = CreateFullscreenPanel(uiRoot, "StatAllocationPanel");

        allocationTitleText = CreateText(allocationPanel, "AllocationTitleText", "스탯 배분", 48f, TextAlignmentOptions.Center);
        RectTransform titleTransform = allocationTitleText.GetComponent<RectTransform>();
        titleTransform.anchorMin = new Vector2(0.5f, 0.5f);
        titleTransform.anchorMax = new Vector2(0.5f, 0.5f);
        titleTransform.anchoredPosition = new Vector2(0f, 276f);
        titleTransform.sizeDelta = new Vector2(760f, 64f);

        TMP_Text guideText = CreateText(allocationPanel, "AllocationGuideText", "기본 20에서 보너스 10포인트를 나눠주세요.", 23f, TextAlignmentOptions.Center);
        RectTransform guideTransform = guideText.GetComponent<RectTransform>();
        guideTransform.anchorMin = new Vector2(0.5f, 0.5f);
        guideTransform.anchorMax = new Vector2(0.5f, 0.5f);
        guideTransform.anchoredPosition = new Vector2(0f, 224f);
        guideTransform.sizeDelta = new Vector2(860f, 42f);

        remainingPointText = CreateText(allocationPanel, "RemainingPointText", string.Empty, 28f, TextAlignmentOptions.Center);
        RectTransform remainingTransform = remainingPointText.GetComponent<RectTransform>();
        remainingTransform.anchorMin = new Vector2(0.5f, 0.5f);
        remainingTransform.anchorMax = new Vector2(0.5f, 0.5f);
        remainingTransform.anchoredPosition = new Vector2(0f, 176f);
        remainingTransform.sizeDelta = new Vector2(520f, 44f);

        statRows = new StatAllocationRow[StatTypes.Length];
        for (int i = 0; i < StatTypes.Length; i++)
        {
            int column = i % 3;
            int row = i / 3;
            Vector2 rowPosition = new Vector2(-300f + column * 300f, 92f - row * 92f);
            statRows[i] = CreateStatAllocationRow(StatTypes[i], rowPosition);
        }

        startCareerButton = CreateButton(allocationPanel, "StartCareerButton", "시작", new Vector2(160f, -230f), HandleStartCareer);
        CreateButton(allocationPanel, "AllocationBackButton", "뒤로", new Vector2(-160f, -230f), HandleBackFromAllocation);

        allocationPanel.SetActive(false);
    }

    private GameObject CreateFullscreenPanel(Transform uiRoot, string objectName)
    {
        GameObject panel = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(uiRoot, false);

        RectTransform panelTransform = panel.GetComponent<RectTransform>();
        panelTransform.anchorMin = Vector2.zero;
        panelTransform.anchorMax = Vector2.one;
        panelTransform.offsetMin = Vector2.zero;
        panelTransform.offsetMax = Vector2.zero;

        Image panelImage = panel.GetComponent<Image>();
        panelImage.color = new Color(0.05f, 0.08f, 0.1f, 0.97f);
        return panel;
    }

    private TMP_Text CreateText(string objectName, string text, float fontSize, TextAlignmentOptions alignment)
    {
        return CreateText(titlePanel, objectName, text, fontSize, alignment);
    }

    private TMP_Text CreateText(GameObject parent, string objectName, string text, float fontSize, TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent.transform, false);

        TMP_Text textComponent = textObject.GetComponent<TMP_Text>();
        textComponent.text = text;
        textComponent.color = Color.white;
        textComponent.fontSize = fontSize;
        textComponent.enableAutoSizing = true;
        textComponent.fontSizeMin = 12f;
        textComponent.fontSizeMax = fontSize;
        textComponent.alignment = alignment;

        if (fontSource != null)
        {
            textComponent.font = fontSource.font;
            textComponent.fontSharedMaterial = fontSource.fontSharedMaterial;
        }

        return textComponent;
    }

    private void CreateButton(string objectName, string buttonText, Vector2 anchoredPosition, UnityEngine.Events.UnityAction action)
    {
        CreateButton(titlePanel, objectName, buttonText, anchoredPosition, action);
    }

    private Button CreateButton(GameObject parent, string objectName, string buttonText, Vector2 anchoredPosition, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent.transform, false);

        RectTransform buttonTransform = buttonObject.GetComponent<RectTransform>();
        buttonTransform.anchorMin = new Vector2(0.5f, 0.5f);
        buttonTransform.anchorMax = new Vector2(0.5f, 0.5f);
        buttonTransform.anchoredPosition = anchoredPosition;
        buttonTransform.sizeDelta = new Vector2(320f, 68f);

        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.color = new Color(0.14f, 0.24f, 0.22f, 1f);

        Button button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(action);

        TMP_Text textComponent = CreateText(buttonObject, $"{objectName}Text", buttonText, 30f, TextAlignmentOptions.Center);
        RectTransform textTransform = textComponent.GetComponent<RectTransform>();
        textTransform.anchorMin = Vector2.zero;
        textTransform.anchorMax = Vector2.one;
        textTransform.offsetMin = Vector2.zero;
        textTransform.offsetMax = Vector2.zero;

        return button;
    }

    private Button CreateSmallButton(GameObject parent, string objectName, string buttonText, Vector2 anchoredPosition, UnityEngine.Events.UnityAction action)
    {
        Button button = CreateButton(parent, objectName, buttonText, anchoredPosition, action);

        RectTransform buttonTransform = button.GetComponent<RectTransform>();
        buttonTransform.sizeDelta = new Vector2(42f, 42f);

        TMP_Text textComponent = button.GetComponentInChildren<TMP_Text>(true);
        if (textComponent != null)
        {
            textComponent.fontSize = 26f;
            textComponent.fontSizeMax = 26f;
        }

        return button;
    }

    private void CreatePositionButton(string objectName, string buttonText, POSITION position, Vector2 anchoredPosition)
    {
        CreateButton(positionPanel, objectName, buttonText, anchoredPosition, () => HandlePositionSelected(position));
    }

    private StatAllocationRow CreateStatAllocationRow(PlayerStatType statType, Vector2 anchoredPosition)
    {
        GameObject rowObject = new GameObject($"{statType}AllocationRow", typeof(RectTransform));
        rowObject.transform.SetParent(allocationPanel.transform, false);

        RectTransform rowTransform = rowObject.GetComponent<RectTransform>();
        rowTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rowTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rowTransform.anchoredPosition = anchoredPosition;
        rowTransform.sizeDelta = new Vector2(270f, 82f);

        TMP_Text labelText = CreateText(rowObject, $"{statType}Label", GetStatLabel(statType), 22f, TextAlignmentOptions.Center);
        RectTransform labelTransform = labelText.GetComponent<RectTransform>();
        labelTransform.anchorMin = new Vector2(0.5f, 1f);
        labelTransform.anchorMax = new Vector2(0.5f, 1f);
        labelTransform.anchoredPosition = new Vector2(0f, -12f);
        labelTransform.sizeDelta = new Vector2(150f, 30f);

        TMP_Text valueText = CreateText(rowObject, $"{statType}Value", string.Empty, 18f, TextAlignmentOptions.Center);
        RectTransform valueTransform = valueText.GetComponent<RectTransform>();
        valueTransform.anchorMin = new Vector2(0.5f, 0.5f);
        valueTransform.anchorMax = new Vector2(0.5f, 0.5f);
        valueTransform.anchoredPosition = new Vector2(0f, -14f);
        valueTransform.sizeDelta = new Vector2(150f, 48f);

        Button minusButton = CreateSmallButton(rowObject, $"{statType}MinusButton", "-", new Vector2(-96f, -14f), () => ChangeAllocatedStat(statType, -1));
        Button plusButton = CreateSmallButton(rowObject, $"{statType}PlusButton", "+", new Vector2(96f, -14f), () => ChangeAllocatedStat(statType, 1));

        return new StatAllocationRow
        {
            StatType = statType,
            ValueText = valueText,
            MinusButton = minusButton,
            PlusButton = plusButton
        };
    }

    private void HandleContinue()
    {
        if (gameFlowManager != null)
        {
            gameFlowManager.ContinueCareerFromTitle();
        }
    }

    private void HandleNewGame()
    {
        PlayQuickFade(ShowPositionPanel);
    }

    private void ShowPositionPanel()
    {
        if (titlePanel != null)
        {
            titlePanel.SetActive(false);
        }

        if (positionPanel != null)
        {
            positionPanel.SetActive(true);
            positionPanel.transform.SetAsLastSibling();
        }
    }

    private void HandleBackFromPositionSelect()
    {
        PlayQuickFade(ShowTitlePanel);
    }

    private void ShowTitlePanel()
    {
        if (positionPanel != null)
        {
            positionPanel.SetActive(false);
        }

        if (titlePanel != null)
        {
            titlePanel.SetActive(true);
            titlePanel.transform.SetAsLastSibling();
        }
    }

    private void HandlePositionSelected(POSITION position)
    {
        selectedPosition = position;
        allocatedStats = new PlayerStats();

        PlayQuickFade(ShowAllocationPanel);
    }

    private void ShowAllocationPanel()
    {
        if (positionPanel != null)
        {
            positionPanel.SetActive(false);
        }

        if (allocationPanel != null)
        {
            allocationPanel.SetActive(true);
            allocationPanel.transform.SetAsLastSibling();
        }

        UpdateAllocationPanel();
    }

    private void ChangeAllocatedStat(PlayerStatType statType, int delta)
    {
        int currentAllocatedValue = allocatedStats.GetValue(statType);

        if (delta > 0)
        {
            if (GetRemainingBonusPoints() <= 0 || StartingStatValue + currentAllocatedValue >= InitialStatCap)
            {
                return;
            }

            allocatedStats.SetValue(statType, currentAllocatedValue + 1);
        }
        else if (currentAllocatedValue > 0)
        {
            allocatedStats.SetValue(statType, currentAllocatedValue - 1);
        }

        UpdateAllocationPanel();
    }

    private void UpdateAllocationPanel()
    {
        if (allocationTitleText != null)
        {
            allocationTitleText.text = $"{GetPositionLabel(selectedPosition)} 스탯 배분";
        }

        if (remainingPointText != null)
        {
            remainingPointText.text = $"남은 포인트: {GetRemainingBonusPoints()} / {StartingBonusPoints}";
        }

        if (statRows != null)
        {
            foreach (StatAllocationRow row in statRows)
            {
                UpdateStatRow(row);
            }
        }

        if (startCareerButton != null)
        {
            startCareerButton.interactable = GetRemainingBonusPoints() == 0;
        }
    }

    private void UpdateStatRow(StatAllocationRow row)
    {
        int allocatedValue = allocatedStats.GetValue(row.StatType);
        int positionBonus = GetPositionBonus(selectedPosition, row.StatType);
        int currentValue = StartingStatValue + allocatedValue + positionBonus;

        if (row.ValueText != null)
        {
            row.ValueText.text = $"{currentValue}\n배분 +{allocatedValue} / 포지션 +{positionBonus}";
        }

        if (row.MinusButton != null)
        {
            row.MinusButton.interactable = allocatedValue > 0;
        }

        if (row.PlusButton != null)
        {
            row.PlusButton.interactable = GetRemainingBonusPoints() > 0 && StartingStatValue + allocatedValue < InitialStatCap;
        }
    }

    private void HandleBackFromAllocation()
    {
        PlayQuickFade(ShowPositionPanelFromAllocation);
    }

    private void ShowPositionPanelFromAllocation()
    {
        if (allocationPanel != null)
        {
            allocationPanel.SetActive(false);
        }

        if (positionPanel != null)
        {
            positionPanel.SetActive(true);
            positionPanel.transform.SetAsLastSibling();
        }
    }

    private void HandleStartCareer()
    {
        if (GetRemainingBonusPoints() != 0)
        {
            return;
        }

        if (gameFlowManager != null)
        {
            gameFlowManager.StartNewCareerFromTitle(selectedPosition, allocatedStats.Clone());
        }
    }

    private void PlayQuickFade(System.Action transitionAction)
    {
        if (gameFlowManager == null)
        {
            transitionAction?.Invoke();
            return;
        }

        gameFlowManager.PlayUiTransition(string.Empty, transitionAction);
    }

    private void HandleQuit()
    {
        if (gameFlowManager != null)
        {
            gameFlowManager.QuitGameFromTitle();
        }
    }

    private int GetRemainingBonusPoints()
    {
        int usedPoints = 0;

        foreach (PlayerStatType statType in StatTypes)
        {
            usedPoints += Mathf.Max(0, allocatedStats.GetValue(statType));
        }

        return Mathf.Max(0, StartingBonusPoints - usedPoints);
    }

    private static int GetPositionBonus(POSITION position, PlayerStatType statType)
    {
        switch (position)
        {
            case POSITION.OutsideHitter:
                return statType == PlayerStatType.Power ? 4
                    : statType == PlayerStatType.Jump ? 3
                    : statType == PlayerStatType.Receive ? 3
                    : 0;
            case POSITION.OppositeHitter:
                return statType == PlayerStatType.Power ? 5
                    : statType == PlayerStatType.Serve ? 3
                    : statType == PlayerStatType.Jump ? 3
                    : 0;
            case POSITION.MiddleBlocker:
                return statType == PlayerStatType.Block ? 5
                    : statType == PlayerStatType.Jump ? 4
                    : statType == PlayerStatType.Power ? 3
                    : 0;
            case POSITION.Setter:
                return statType == PlayerStatType.Set ? 5
                    : statType == PlayerStatType.Mental ? 4
                    : statType == PlayerStatType.Speed ? 3
                    : 0;
            case POSITION.Libero:
                return statType == PlayerStatType.Receive ? 5
                    : statType == PlayerStatType.Speed ? 4
                    : statType == PlayerStatType.Mental ? 3
                    : 0;
            case POSITION.DefensiveSpecialist:
                return statType == PlayerStatType.Receive ? 4
                    : statType == PlayerStatType.Speed ? 4
                    : statType == PlayerStatType.Stamina ? 3
                    : 0;
            default:
                return 0;
        }
    }

    private static string GetStatLabel(PlayerStatType statType)
    {
        switch (statType)
        {
            case PlayerStatType.Power:
                return "파워";
            case PlayerStatType.Jump:
                return "점프";
            case PlayerStatType.Receive:
                return "리시브";
            case PlayerStatType.Set:
                return "토스";
            case PlayerStatType.Block:
                return "블록";
            case PlayerStatType.Serve:
                return "서브";
            case PlayerStatType.Speed:
                return "스피드";
            case PlayerStatType.Stamina:
                return "체력";
            case PlayerStatType.Mental:
                return "멘탈";
            default:
                return statType.ToString();
        }
    }

    private static string GetPositionLabel(POSITION position)
    {
        switch (position)
        {
            case POSITION.OutsideHitter:
                return "아웃사이드 히터";
            case POSITION.OppositeHitter:
                return "아포짓 히터";
            case POSITION.MiddleBlocker:
                return "미들 블로커";
            case POSITION.Setter:
                return "세터";
            case POSITION.Libero:
                return "리베로";
            case POSITION.DefensiveSpecialist:
                return "디펜시브 스페셜리스트";
            default:
                return position.ToString();
        }
    }

    private struct StatAllocationRow
    {
        public PlayerStatType StatType;
        public TMP_Text ValueText;
        public Button MinusButton;
        public Button PlusButton;
    }
}
