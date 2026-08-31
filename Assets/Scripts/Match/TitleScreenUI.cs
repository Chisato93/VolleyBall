using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TitleScreenUI : MonoBehaviour
{
    private GameFlowManager gameFlowManager;
    private GameObject titlePanel;
    private TMP_Text fontSource;

    public void Initialize(GameFlowManager owner, Transform uiRoot, TMP_Text sourceText)
    {
        gameFlowManager = owner;
        fontSource = sourceText;

        if (titlePanel == null)
        {
            CreateTitlePanel(uiRoot);
        }
    }

    public void Show()
    {
        if (titlePanel != null)
        {
            titlePanel.SetActive(true);
            titlePanel.transform.SetAsLastSibling();
        }
    }

    public void Hide()
    {
        if (titlePanel != null)
        {
            titlePanel.SetActive(false);
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

    private TMP_Text CreateText(string objectName, string text, float fontSize, TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(titlePanel.transform, false);

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
        GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(titlePanel.transform, false);

        RectTransform buttonTransform = buttonObject.GetComponent<RectTransform>();
        buttonTransform.anchorMin = new Vector2(0.5f, 0.5f);
        buttonTransform.anchorMax = new Vector2(0.5f, 0.5f);
        buttonTransform.anchoredPosition = anchoredPosition;
        buttonTransform.sizeDelta = new Vector2(320f, 68f);

        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.color = new Color(0.14f, 0.24f, 0.22f, 1f);

        Button button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(action);

        TMP_Text textComponent = CreateText($"{objectName}Text", buttonText, 30f, TextAlignmentOptions.Center);
        RectTransform textTransform = textComponent.GetComponent<RectTransform>();
        textTransform.SetParent(buttonObject.transform, false);
        textTransform.anchorMin = Vector2.zero;
        textTransform.anchorMax = Vector2.one;
        textTransform.offsetMin = Vector2.zero;
        textTransform.offsetMax = Vector2.zero;
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
        if (gameFlowManager != null)
        {
            gameFlowManager.StartNewCareerFromTitle();
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
