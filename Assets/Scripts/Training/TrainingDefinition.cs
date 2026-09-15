public class TrainingDefinition
{
    public string Name { get; }
    public PlayerStatType StatType { get; }
    public bool UsesRandomStat { get; }

    public TrainingDefinition(string name, PlayerStatType statType)
    {
        Name = name;
        StatType = statType;
        UsesRandomStat = false;
    }

    public TrainingDefinition(string name)
    {
        Name = name;
        StatType = PlayerStatType.Power;
        UsesRandomStat = true;
    }

    public string GetDetailText()
    {
        return "선택하면 행동력을 1 사용합니다.";
    }

    public string GetEffectText()
    {
        if (UsesRandomStat)
        {
            return "랜덤 능력치 +1~3";
        }

        return $"{StatType} +1~3";
    }

    public string GetTimeText()
    {
        return "1회";
    }
}
