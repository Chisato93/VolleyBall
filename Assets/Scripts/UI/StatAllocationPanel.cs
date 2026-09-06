using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatAllocationPanel : MonoBehaviour
{
    [SerializeField] private TitleScreenCanvas titleScreenCanvas;
    [SerializeField] private CanvasGroupView titleScreenPanelView;
    [SerializeField] private TMP_Text allocationTitleText;
    [SerializeField] private StatAllocationRow[] statRows;

    private const int StartingStatValue = 20;
    private const int InitialMaxStatCapacity = 30;
    private const int RemainingPoints = 10;

    private PlayerStats rolledStats = new PlayerStats();
    private bool isWating = false;
    private void Start()
    {
        UpdateAllocationPanel();
        UnSelectAllocationRow();
    }
    public void Hide()
    {
        titleScreenPanelView.Hide();
    }
    public void Show()
    {
        StartCoroutine(RollForStats());
        titleScreenPanelView.Show();
    }
    public void OnBackClicked()
    {
        titleScreenCanvas.TryPositionMenu();
    }
    public void OnStartClicked()
    {
        titleScreenCanvas.HandleStartCareer(rolledStats);
    }
    // 주사위 굴리기
    public void OnRolledClicked()
    {
        if (isWating)
            return;
        StartCoroutine(RollForStats());
    }
    private void UnSelectAllocationRow()
    {
        foreach(StatAllocationRow row in statRows)
        {
            row.SelectedAllocationRow(false);
        }
    }
    private IEnumerator RollForStats()
    {
        isWating = true;

        rolledStats = new PlayerStats();
        int remainPoints = RemainingPoints;
        UnSelectAllocationRow();
        
        while (remainPoints > 0)
        {
            int randomStatIndex = UnityEngine.Random.Range(0, Enum.GetValues(typeof(PlayerStatType)).Length);
            var updateAllocation = statRows[randomStatIndex];
            if (AddChangeAllocatedStat(updateAllocation))
            {
                updateAllocation.SelectedAllocationRow(true);
                remainPoints--;
            }
        }
        UpdateAllocationPanel();


        yield return new WaitForSeconds(1f);

        isWating = false;
    }

    private bool AddChangeAllocatedStat(StatAllocationRow allocation)
    {
        var statType = allocation.StatType;
        int currentAllocatedValue = rolledStats.GetValue(statType);
        
        if (StartingStatValue + currentAllocatedValue >= InitialMaxStatCapacity)
            return false;

        rolledStats.SetValue(statType, currentAllocatedValue + 1);
        return true;
    }
    private POSITION SelectedPosition()
    {
        return titleScreenCanvas.GetPosition();
    }
    private string GetPositionLabel()
    {
        return titleScreenCanvas.GetPosition().ToString();
    }
    private void UpdateAllocationPanel()
    {
        if (allocationTitleText != null)
        {
            allocationTitleText.text = $"{GetPositionLabel()} 스탯 배분";
        }
        if (statRows != null)
        {
            foreach (StatAllocationRow row in statRows)
            {
                UpdateStatRow(row);
            }
        }
    }
    private void UpdateStatRow(StatAllocationRow row)
    {
        int allocatedValue = rolledStats.GetValue(row.StatType);
        int positionBonus = GetPositionBonus(SelectedPosition(), row.StatType);
        int currentValue = StartingStatValue + allocatedValue + positionBonus;

        row.SetValueText($"{currentValue}\n배분 +{allocatedValue} / 포지션 +{positionBonus}");
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
}
