using System.Collections.Generic;
using UnityEngine;

public static class TrainingCatalog
{
    private static readonly TrainingDefinition[] TrainingDefinitions =
    {
        new TrainingDefinition("파워 훈련", PlayerStatType.Power),
        new TrainingDefinition("점프 훈련", PlayerStatType.Jump),
        new TrainingDefinition("리시브 훈련", PlayerStatType.Receive),
        new TrainingDefinition("토스 훈련", PlayerStatType.Set),
        new TrainingDefinition("블로킹 훈련", PlayerStatType.Block),
        new TrainingDefinition("서브 훈련", PlayerStatType.Serve),
        new TrainingDefinition("스피드 훈련", PlayerStatType.Speed),
        new TrainingDefinition("체력 훈련", PlayerStatType.Stamina),
        new TrainingDefinition("멘탈 훈련", PlayerStatType.Mental),
        new TrainingDefinition("종합 훈련")
    };

    private static readonly PlayerStatType[] AllStatTypes =
    {
        PlayerStatType.Power,
        PlayerStatType.Jump,
        PlayerStatType.Receive,
        PlayerStatType.Set,
        PlayerStatType.Block,
        PlayerStatType.Serve,
        PlayerStatType.Speed,
        PlayerStatType.Stamina,
        PlayerStatType.Mental
    };

    public static List<TrainingDefinition> GetAllTrainings()
    {
        return new List<TrainingDefinition>(TrainingDefinitions);
    }

    public static PlayerStatType ResolveStatType(TrainingDefinition training)
    {
        if (training == null)
        {
            Debug.LogWarning("Training definition is missing.");
            return PlayerStatType.Power;
        }

        if (!training.UsesRandomStat)
        {
            return training.StatType;
        }

        return AllStatTypes[Random.Range(0, AllStatTypes.Length)];
    }
}
