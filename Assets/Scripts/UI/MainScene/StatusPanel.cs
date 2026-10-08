using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusPanel : TabContentPanel
{
    public enum StatusPanelType
    {
        Status = 0,
        Equipment = 1,
        Record = 2
    }
    [SerializeField] private CanvasGroupView statusPanelView;
    [SerializeField] private Button returnHomeButton;
    [SerializeField] private List<GameObject> tabList;

    public void Show()
    {
        statusPanelView.Show();

        SetActiveTab(StatusPanelType.Status);
    }

    public void Hide()
    {
        statusPanelView.Hide();
    }

    public void SetActiveTab(StatusPanelType type)
    {
        SetActiveTab((int)type);
    }

    public override void SetActiveTab(int index)
    {
        if (tabList == null || tabList.Count == 0)
        {
            return;
        }

        if (index < 0 || index >= tabList.Count)
        {
            index = 0;
        }

        for (int i = 0; i < tabList.Count; i++)
        {
            if (tabList[i] == null)
            {
                continue;
            }

            if (i == index)
            {
                tabList[i].SetActive(true);
            }
            else
            {
                tabList[i].SetActive(false);
            }
        }
    }

}
