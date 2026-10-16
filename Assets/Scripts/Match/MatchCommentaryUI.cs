using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchCommentaryUI : MonoBehaviour
{
    private const int MaxVisibleLineCount = 10;
    private const float LineMoveDuration = 0.2f;

    [SerializeField] private TMP_Text setText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text setScoreText;
    [SerializeField] private TMP_Text commentaryText;
    [SerializeField] private Image leftCharacterImage;
    [SerializeField] private Image rightCharacterImage;
    [SerializeField] private TMP_Text speedButtonText;
    [SerializeField] private Button speedButton;
    [SerializeField] private Button skipMatchButton;

    private readonly Queue<MatchCommentaryMessage> pendingLines = new Queue<MatchCommentaryMessage>();
    private readonly Dictionary<string, Sprite> portraitCache = new Dictionary<string, Sprite>();
    private readonly Queue<TMP_Text> availableTexts = new Queue<TMP_Text>();
    private readonly List<TMP_Text> visibleTexts = new List<TMP_Text>();
    private float lineMoveElapsed;
    private float playbackSpeed = 1f;
    private bool isMovingLines;

    public bool IsPlaybackComplete => pendingLines.Count == 0 && !isMovingLines;
    public Button SpeedButton => speedButton;
    public Button SkipMatchButton => skipMatchButton;

    private void Awake()
    {
        FindMissingReferences();
        InitializeCommentaryLines();
    }

    private void Update()
    {
        if (!isMovingLines)
        {
            BeginNextLine();
        }

        if (!isMovingLines)
        {
            return;
        }

        lineMoveElapsed += Time.deltaTime * playbackSpeed;
        float progress = Mathf.Clamp01(lineMoveElapsed / LineMoveDuration);
        float offset = Mathf.SmoothStep(0f, 1f, progress);

        for (int i = 0; i < visibleTexts.Count; i++)
        {
            SetLinePosition(visibleTexts[i], visibleTexts.Count - 2 - i + offset);
        }

        if (progress >= 1f)
        {
            CompleteLineMove();
        }
    }

    private void OnDisable()
    {
        if (isMovingLines)
        {
            CompleteLineMove();
        }
    }

    public void ResetUi()
    {
        isMovingLines = false;
        lineMoveElapsed = 0f;
        pendingLines.Clear();

        foreach (TMP_Text text in visibleTexts)
        {
            RecycleLine(text);
        }

        visibleTexts.Clear();
        ClearCharacterImage(leftCharacterImage);
        ClearCharacterImage(rightCharacterImage);
    }

    public void SetScore(int currentSet, int playerScore, int opponentScore, int playerSetWins, int opponentSetWins)
    {
        if (setText != null)
        {
            setText.text = $"{currentSet} Set";
        }

        if (scoreText != null)
        {
            scoreText.text = $"Player {playerScore} : {opponentScore} Opponent";
        }

        if (setScoreText != null)
        {
            setScoreText.text = $"Sets {playerSetWins} : {opponentSetWins}";
        }
    }

    public void AddLine(string line)
    {
        AddLine(new MatchCommentaryMessage { text = line });
    }

    public void AddLine(MatchCommentaryMessage message)
    {
        if (message == null || string.IsNullOrWhiteSpace(message.text) || commentaryText == null)
        {
            return;
        }

        pendingLines.Enqueue(message);
    }

    private void ShowCharacterImage(MatchCommentaryMessage message)
    {
        ClearCharacterImage(leftCharacterImage);
        ClearCharacterImage(rightCharacterImage);

        Image image;
        if (message.side == "left")
        {
            image = leftCharacterImage;
        }
        else if (message.side == "right")
        {
            image = rightCharacterImage;
        }
        else
        {
            return;
        }

        if (image == null)
        {
            return;
        }

        string path = message.resourcePath;
        if (string.IsNullOrWhiteSpace(path) && !string.IsNullOrWhiteSpace(message.character))
        {
            path = "Character/Portraits/" + message.character;
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            ClearCharacterImage(image);
            return;
        }

        if (!portraitCache.TryGetValue(path, out Sprite sprite))
        {
            sprite = Resources.Load<Sprite>(path);
            portraitCache.Add(path, sprite);
            if (sprite == null)
            {
                Debug.LogWarning($"캐릭터 이미지를 찾지 못했습니다: Resources/{path} (Sprite 설정 확인)");
            }
        }

        image.sprite = sprite;
        image.enabled = sprite != null;
    }

    private void ClearCharacterImage(Image image)
    {
        if (image != null)
        {
            image.sprite = null;
            image.enabled = false;
        }
    }

    private void InitializeCommentaryLines()
    {
        if (commentaryText == null)
        {
            return;
        }

        RectTransform viewport = commentaryText.rectTransform;
        if (viewport.GetComponent<RectMask2D>() == null)
        {
            viewport.gameObject.AddComponent<RectMask2D>();
        }

        // Keep the existing text as the font/style reference and use its rectangle as the viewport.
        commentaryText.text = string.Empty;
        commentaryText.enabled = false;

        for (int i = 0; i <= MaxVisibleLineCount; i++)
        {
            GameObject lineObject = new GameObject($"CommentaryLine{i + 1}", typeof(RectTransform));
            lineObject.layer = viewport.gameObject.layer;
            lineObject.transform.SetParent(viewport, false);

            TMP_Text text = lineObject.AddComponent<TextMeshProUGUI>();
            text.font = commentaryText.font;
            text.fontSharedMaterial = commentaryText.fontSharedMaterial;
            text.color = commentaryText.color;
            text.fontSize = commentaryText.fontSize;
            text.fontStyle = commentaryText.fontStyle;
            text.fontWeight = commentaryText.fontWeight;
            text.enableAutoSizing = commentaryText.enableAutoSizing;
            text.fontSizeMin = commentaryText.fontSizeMin;
            text.fontSizeMax = commentaryText.fontSizeMax;
            text.horizontalAlignment = commentaryText.horizontalAlignment;
            text.verticalAlignment = VerticalAlignmentOptions.Middle;
            text.richText = commentaryText.richText;
            text.margin = commentaryText.margin;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.raycastTarget = false;
            text.maskable = true;
            SetLinePosition(text, 0f);
            RecycleLine(text);
        }
    }

    private void BeginNextLine()
    {
        if (pendingLines.Count == 0 || availableTexts.Count == 0)
        {
            return;
        }

        TMP_Text text = availableTexts.Dequeue();
        MatchCommentaryMessage message = pendingLines.Dequeue();
        text.text = message.text;
        ShowCharacterImage(message);
        SetLinePosition(text, -1f);
        text.gameObject.SetActive(true);
        visibleTexts.Add(text);
        lineMoveElapsed = 0f;
        isMovingLines = true;
    }

    private void CompleteLineMove()
    {
        if (visibleTexts.Count > MaxVisibleLineCount)
        {
            TMP_Text oldestText = visibleTexts[0];
            visibleTexts.RemoveAt(0);
            RecycleLine(oldestText);
        }

        for (int i = 0; i < visibleTexts.Count; i++)
        {
            SetLinePosition(visibleTexts[i], visibleTexts.Count - 1 - i);
        }

        isMovingLines = false;
    }

    private void RecycleLine(TMP_Text text)
    {
        text.text = string.Empty;
        text.gameObject.SetActive(false);
        availableTexts.Enqueue(text);
    }

    private void SetLinePosition(TMP_Text text, float slot)
    {
        RectTransform rect = text.rectTransform;
        rect.anchorMin = new Vector2(0f, slot / MaxVisibleLineCount);
        rect.anchorMax = new Vector2(1f, (slot + 1f) / MaxVisibleLineCount);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
    }
    public void SetPlaybackSpeed(float speed)
    {
        playbackSpeed = Mathf.Max(1f, speed);
        if (speedButtonText != null)
        {
            speedButtonText.text = $"x{playbackSpeed:0.##}";
        }
    }
    private void FindMissingReferences()
    {
        if (setText == null) setText = FindChildText("SetText");
        if (scoreText == null) scoreText = FindChildText("ScoreText");
        if (setScoreText == null) setScoreText = FindChildText("SetScoreText");
        if (commentaryText == null) commentaryText = FindChildText("CommentaryText");
        if (speedButton == null) speedButton = FindChildButton("SpeedButton");
        if (skipMatchButton == null) skipMatchButton = FindChildButton("SkipMatchButton");
    }

    private TMP_Text FindChildText(string objectName)
    {
        TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text text in texts)
        {
            if (text.name == objectName)
            {
                return text;
            }
        }

        return null;
    }

    private Button FindChildButton(string objectName)
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            if (button.name == objectName)
            {
                return button;
            }
        }

        return null;
    }
}
