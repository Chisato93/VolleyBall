using UnityEngine;

public static class TrainingService
{
    private const int MinTrainingIncrease = 1;
    private const int MaxTrainingIncreaseExclusive = 4;
    private const int MaxStatValue = PlayerStats.MaxValue;
    private const int StartingStatValue = 20;
    private const string CareerPlayerSaveId = "Player Outside Hitter";

    public static bool CanTrain(TrainingDefinition training)
    {
        return training != null && !training.UsesRandomStat
            && LoadCareerPlayerStats().GetValue(training.StatType) < MaxStatValue;
    }

    public static TrainingResult ApplyTraining(TrainingDefinition training)
    {
        if (training == null)
        {
            Debug.LogWarning("Training definition is missing.");
            return TrainingResult.Succeeded(string.Empty, PlayerStatType.Power, 0, 0, 0);
        }

        PlayerStats stats = LoadCareerPlayerStats();
        PlayerStatType statType = TrainingCatalog.ResolveStatType(training);
        int amount = Random.Range(MinTrainingIncrease, MaxTrainingIncreaseExclusive);
        int beforeValue = stats.GetValue(statType);

        stats.AddValue(statType, amount, MaxStatValue);
        PlayerSaveManager.SavePlayerStats(CareerPlayerSaveId, stats);

        int afterValue = stats.GetValue(statType);
        int actualIncrease = afterValue - beforeValue;
        Debug.Log($"Training result: {training.Name} / {statType} +{actualIncrease} ({beforeValue} -> {afterValue})");

        return TrainingResult.Succeeded(training.Name, statType, actualIncrease, beforeValue, afterValue);
    }

    public static PlayerStats LoadCareerPlayerStats()
    {
        PlayerStats stats = CreateDefaultCareerPlayerStats();
        PlayerSaveManager.LoadPlayerStats(CareerPlayerSaveId, stats);
        return stats;
    }

    private static PlayerStats CreateDefaultCareerPlayerStats()
    {
        PlayerStats stats = new PlayerStats();
        stats.Power = StartingStatValue;
        stats.Jump = StartingStatValue;
        stats.Receive = StartingStatValue;
        stats.Set = StartingStatValue;
        stats.Block = StartingStatValue;
        stats.Serve = StartingStatValue;
        stats.Speed = StartingStatValue;
        stats.Stamina = StartingStatValue;
        stats.Mental = StartingStatValue;
        return stats;
    }
}
