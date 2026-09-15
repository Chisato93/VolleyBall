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
    private const int MaxStatValue = 99;
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
        CacheSceneReferences();

        EnsureDataManagers();

        CreateMissingMainControls();
        CreateMissingResultPanel();
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

        if (resultCloseButton != null)
        {
            resultCloseButton.onClick.RemoveListener(HandleResultClose);
            resultCloseButton.onClick.AddListener(HandleResultClose);
        }
    }

    private void Start()
    {
        EnsureDataManagers();
        StartMainPhase();
    }

    public void CompleteTrainingAction()
    {
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
        remainingActionPoints = Mathf.Max(0, maxActionPoints);
        ShowMainPanel();
    }

    private void ShowMainPanel()
    {
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

        ActionPointsChanged?.Invoke(remainingActionPoints, maxActionPoints);
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
        ConsumeActionPoint();
        ShowActionResult(result);
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

    public void StartMatchAction()
    {
        PlayScreenTransition("경기 준비 중...", LoadMatchScene);
    }

    private void LoadMatchScene()
    {
        if (SceneLoadManager.Instance != null)
        {
            SceneLoadManager.Instance.LoadScene(SceneEnums.MatchScene);
            return;
        }

        SceneManager.LoadScene((int)SceneEnums.MatchScene);
    }

    private string BuildShopText()
    {
        EnsureDataManagers();
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
        builder.AppendLine("능력치");
        builder.AppendLine($"파워 {stats.Power} / 점프 {stats.Jump} / 리시브 {stats.Receive}");
        builder.AppendLine($"토스 {stats.Set} / 블록 {stats.Block} / 서브 {stats.Serve}");
        builder.AppendLine($"스피드 {stats.Speed} / 체력 {stats.Stamina} / 멘탈 {stats.Mental}");
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

        resultText.text = $"휴식 완료!\n체력 +{result.StaminaAfter - result.StaminaBefore} ({result.StaminaBefore} -> {result.StaminaAfter})\n멘탈 +{result.MentalAfter - result.MentalBefore} ({result.MentalBefore} -> {result.MentalAfter})\n남은 행동력: {remainingActionPoints} / {maxActionPoints}";
    }

    private void HandleResultClose()
    {
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

        if (trainingPanel != null)
        {
            return trainingPanel.transform;
        }

        return null;
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

    private void CacheSceneReferences()
    {
        if (trainingPanel == null)
        {
            trainingPanel = FindFirstObjectByType<TrainingPanel>();
        }

        if (mainPanel == null)
        {
            mainPanel = FindSceneGameObject("MainPanel");
        }

        if (actionPointText == null)
        {
            actionPointText = FindSceneComponent<TMP_Text>("ActionPointText");
        }

        if (trainingActionButton == null)
        {
            trainingActionButton = FindSceneComponent<Button>("TrainingActionButton");
        }

        if (restActionButton == null)
        {
            restActionButton = FindSceneComponent<Button>("RestActionButton");
        }

        if (shopActionButton == null)
        {
            shopActionButton = FindSceneComponent<Button>("ShopActionButton");
        }

        if (playerInfoActionButton == null)
        {
            playerInfoActionButton = FindSceneComponent<Button>("PlayerInfoActionButton");
        }

        if (matchActionButton == null)
        {
            matchActionButton = FindSceneComponent<Button>("MatchActionButton");
        }
    }

    private static GameObject FindSceneGameObject(string objectName)
    {
        GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();

        foreach (GameObject rootObject in rootObjects)
        {
            Transform foundTransform = FindChildRecursive(rootObject.transform, objectName);
            if (foundTransform != null)
            {
                return foundTransform.gameObject;
            }
        }

        return null;
    }

    private static T FindSceneComponent<T>(string objectName) where T : Component
    {
        GameObject sceneObject = FindSceneGameObject(objectName);
        return sceneObject != null ? sceneObject.GetComponent<T>() : null;
    }

    private static Transform FindChildRecursive(Transform current, string objectName)
    {
        if (current.name == objectName)
        {
            return current;
        }

        foreach (Transform child in current)
        {
            Transform foundChild = FindChildRecursive(child, objectName);
            if (foundChild != null)
            {
                return foundChild;
            }
        }

        return null;
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
