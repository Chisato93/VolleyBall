using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecordTabPanel : MonoBehaviour
{
    private const int RecordsPerPage = 5;

    [SerializeField] private RecordListItemUI[] rows;
    [SerializeField] private Button previousPageButton;
    [SerializeField] private Button nextPageButton;
    [SerializeField] private TMP_Text pageText;
    [SerializeField] private RecordDetailsPanel detailsPanel;

    private RecordListItem[] records;
    private int pageIndex;

    private int PageCount => Mathf.Max(1, Mathf.CeilToInt(records.Length / (float)RecordsPerPage));

    private void Awake()
    {
        records = RecordMockData.Create();
        previousPageButton.onClick.AddListener(ShowPreviousPage);
        nextPageButton.onClick.AddListener(ShowNextPage);
    }

    private void OnEnable()
    {
        detailsPanel.Hide();
        pageIndex = 0;
        RefreshPage();
    }

    private void OnDisable()
    {
        detailsPanel.Hide();
    }

    private void OnDestroy()
    {
        previousPageButton.onClick.RemoveListener(ShowPreviousPage);
        nextPageButton.onClick.RemoveListener(ShowNextPage);
    }

    private void ShowPreviousPage()
    {
        pageIndex = Mathf.Max(0, pageIndex - 1);
        RefreshPage();
    }

    private void ShowNextPage()
    {
        pageIndex = Mathf.Min(PageCount - 1, pageIndex + 1);
        RefreshPage();
    }

    private void RefreshPage()
    {
        for (int i = 0; i < rows.Length; i++)
        {
            int recordIndex = pageIndex * RecordsPerPage + i;
            bool hasRecord = i < RecordsPerPage && recordIndex < records.Length;
            rows[i].gameObject.SetActive(hasRecord);
            if (hasRecord)
            {
                rows[i].Bind(records[recordIndex], detailsPanel.Show);
            }
        }

        pageText.text = $"{pageIndex + 1} / {PageCount}";
        previousPageButton.interactable = pageIndex > 0;
        nextPageButton.interactable = pageIndex < PageCount - 1;
    }
}
