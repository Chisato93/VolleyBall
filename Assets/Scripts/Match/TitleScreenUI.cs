using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TitleScreenUI : MonoBehaviour
{
    private GameFlowManager gameFlowManager;
    private GameObject titlePanel;
    private GameObject positionPanel;
    private TMP_Text fontSource;

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
    }

    private void CreateTitlePanel(Transform uiRoot)
    {
        if (uiRoot == null)
        {
            Debug.LogWarning("타이틀 화면을 만들 UI 루트를 찾지 못했습니다.");
            return;
        }

        titlePanel = new GameObject("TitleScreenPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        titlePanel.transform.SetParent(uiRoot, false);

        RectTransform panelTransform = titlePanel.GetComponent<RectTransform>();
        panelTransform.anchorMin = Vector2.zero;
        panelTransform.anchorMax = Vector2.one;
        panelTransform.offsetMin = Vector2.zero;
        panelTransform.offsetMax = Vector2.zero;

        Image panelImage = titlePanel.GetComponent<Image>();
        panelImage.color = new Color(0.05f, 0.08f, 0.1f, 0.97f);

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

        positionPanel = new GameObject("PositionSelectPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        positionPanel.transform.SetParent(uiRoot, false);

        RectTransform panelTransform = positionPanel.GetComponent<RectTransform>();
        panelTransform.anchorMin = Vector2.zero;
        panelTransform.anchorMax = Vector2.one;
        panelTransform.offsetMin = Vector2.zero;
        panelTransform.offsetMax = Vector2.zero;

        Image panelImage = positionPanel.GetComponent<Image>();
        panelImage.color = new Color(0.05f, 0.08f, 0.1f, 0.97f);

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
        textComponent.fontSizeMin = 18f;
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

    private void CreateButton(GameObject parent, string objectName, string buttonText, Vector2 anchoredPosition, UnityEngine.Events.UnityAction action)
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
    }

    private void CreatePositionButton(string objectName, string buttonText, POSITION position, Vector2 anchoredPosition)
    {
        CreateButton(positionPanel, objectName, buttonText, anchoredPosition, () => HandlePositionSelected(position));
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
        if (gameFlowManager != null)
        {
            gameFlowManager.StartNewCareerFromTitle(position);
        }
    }

    private void HandleQuit()
    {
        if (gameFlowManager != null)
        {
            gameFlowManager.QuitGameFromTitle();
        }
    }
}
