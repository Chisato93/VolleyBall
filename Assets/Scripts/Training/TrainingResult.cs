public struct TrainingResult
{
    public string TrainingName;
    public PlayerStatType StatType;
    public int Increase;
    public int BeforeValue;
    public int AfterValue;
    public int RemainingActionPoints;
    public int MaxActionPoints;

    public static TrainingResult Succeeded(
        string trainingName,
        PlayerStatType statType,
        int increase,
        int beforeValue,
        int afterValue)
    {
        return new TrainingResult
        {
            TrainingName = trainingName,
            StatType = statType,
            Increase = increase,
            BeforeValue = beforeValue,
            AfterValue = afterValue
        };
    }
}
