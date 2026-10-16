using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class RecordListItem
{
    public int CareerYear;
    public int Month;
    public string OpponentTeamName;
    public int PlayerSetWins;
    public int OpponentSetWins;
    public string[] CommentaryLines = Array.Empty<string>();
}

public class RecordListItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text dateText;
    [SerializeField] private TMP_Text opponentText;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private Button detailsButton;

    private RecordListItem currentRecord;
    private Action<RecordListItem> showDetails;

    public void Bind(RecordListItem record, Action<RecordListItem> onShowDetails)
    {
        currentRecord = record;
        showDetails = onShowDetails;
        dateText.text = $"{record.CareerYear}년차 {record.Month}월";
        opponentText.text = $"상대팀: {record.OpponentTeamName}";
        resultText.text = record.PlayerSetWins > record.OpponentSetWins ? "경기결과: 승리" : "경기결과: 패배";
        scoreText.text = $"{record.PlayerSetWins}:{record.OpponentSetWins}";
        detailsButton.onClick.RemoveListener(ShowDetails);
        detailsButton.onClick.AddListener(ShowDetails);
        detailsButton.interactable = showDetails != null;
    }

    private void ShowDetails()
    {
        showDetails?.Invoke(currentRecord);
    }

    private void OnDestroy()
    {
        detailsButton.onClick.RemoveListener(ShowDetails);
    }
}
