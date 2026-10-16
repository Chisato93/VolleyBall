public class TrainingDefinition
{
    public string Name { get; }
    public PlayerStatType StatType { get; }
    public bool UsesRandomStat { get; }

    public TrainingDefinition(string name, PlayerStatType statType)
    {
        Name = name;
        StatType = statType;
    }

    public TrainingDefinition(string name)
    {
        Name = name;
        StatType = PlayerStatType.Power;
        UsesRandomStat = true;
    }

    public string GetDetailText() => "훈련하면 해당 능력치가 증가합니다.";
    public string GetEffectText() => UsesRandomStat ? "랜덤 능력치 +1~3"
        : $"{TrainingCatalog.GetStatName(StatType)} +1~3";
}

public class TrainingOffer
{
    public TrainingDefinition Training { get; }
    public int EnergyCost { get; }

    public TrainingOffer(TrainingDefinition training, int energyCost)
    {
        Training = training;
        EnergyCost = energyCost;
    }
}
