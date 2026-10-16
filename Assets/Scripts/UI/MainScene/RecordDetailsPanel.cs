using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecordDetailsPanel : MonoBehaviour
{
    private const int VisibleLineCount = 10;

    [SerializeField] private TMP_Text matchInfoText;
    [SerializeField] private TMP_Text[] logTexts;
    [SerializeField] private TMP_Text rangeText;
    [SerializeField] private TMP_Text emptyText;
    [SerializeField] private Button upButton;
    [SerializeField] private Button downButton;
    [SerializeField] private Button closeButton;

    private string[] lines = Array.Empty<string>();
    private int firstLineIndex;

    private int LastStartIndex => Mathf.Max(0, lines.Length - VisibleLineCount);

    private void Awake()
    {
        upButton.onClick.AddListener(ShowPreviousLine);
        downButton.onClick.AddListener(ShowNextLine);
        closeButton.onClick.AddListener(Hide);
    }

    public void Show(RecordListItem record)
    {
        lines = record.CommentaryLines ?? Array.Empty<string>();
        firstLineIndex = 0;
        string result = record.PlayerSetWins > record.OpponentSetWins ? "승리" : "패배";
        matchInfoText.text = $"{record.CareerYear}년차 {record.Month}월  상대팀: {record.OpponentTeamName}  {result}  {record.PlayerSetWins}:{record.OpponentSetWins}";
        gameObject.SetActive(true);
        RefreshLines();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void ShowPreviousLine()
    {
        firstLineIndex = Mathf.Max(0, firstLineIndex - 1);
        RefreshLines();
    }

    private void ShowNextLine()
    {
        firstLineIndex = Mathf.Min(LastStartIndex, firstLineIndex + 1);
        RefreshLines();
    }

    private void RefreshLines()
    {
        for (int i = 0; i < logTexts.Length; i++)
        {
            int lineIndex = firstLineIndex + i;
            bool hasLine = i < VisibleLineCount && lineIndex < lines.Length;
            logTexts[i].gameObject.SetActive(hasLine);
            logTexts[i].text = hasLine ? lines[lineIndex] : string.Empty;
        }

        emptyText.gameObject.SetActive(lines.Length == 0);
        rangeText.text = lines.Length == 0
            ? "0 / 0"
            : $"{firstLineIndex + 1}~{Mathf.Min(firstLineIndex + VisibleLineCount, lines.Length)} / {lines.Length}";
        upButton.interactable = firstLineIndex > 0;
        downButton.interactable = firstLineIndex < LastStartIndex;
    }

    private void OnDestroy()
    {
        upButton.onClick.RemoveListener(ShowPreviousLine);
        downButton.onClick.RemoveListener(ShowNextLine);
        closeButton.onClick.RemoveListener(Hide);
    }
}
