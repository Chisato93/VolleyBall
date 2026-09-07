using UnityEngine;

public static class GameFlowStateManager
{
    private const int MaxStatValue = 99;
    private const int StartingStatValue = 20;
    private const int StartingBonusPoints = 10;
    private const int InitialStatCap = 30;
    private const int StartingMoney = 0;
    private const int StartingYear = 1;
    private const int StartingMonth = 11;
    private const string PlayerDataSaveId = "Player Outside Hitter";

    private static readonly PlayerStatType[] StartingStatTypes =
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

    public static void StartNewCareer(POSITION selectedPosition, PlayerStats allocatedStats)
    {
        PlayerStats startingStats = BuildNewCareerStartingStats(selectedPosition, allocatedStats);
        ResetCareerSaveData(selectedPosition, startingStats);
    }

    private static PlayerStats BuildNewCareerStartingStats(POSITION selectedPosition, PlayerStats allocatedStats)
    {
        PlayerStats startingStats = CreateBaseStartingStats();
        int remainingBonusPoints = StartingBonusPoints;

        if (allocatedStats != null)
        {
            foreach (PlayerStatType statType in StartingStatTypes)
            {
                int maxAllocatable = InitialStatCap - startingStats.GetValue(statType);
                int requestedAmount = allocatedStats.GetValue(statType);
                int appliedAmount = Mathf.Clamp(requestedAmount, 0, Mathf.Min(maxAllocatable, remainingBonusPoints));

                startingStats.AddValue(statType, appliedAmount, InitialStatCap);
                remainingBonusPoints -= appliedAmount;
            }
        }

        ApplyPositionBonus(startingStats, selectedPosition);
        return startingStats;
    }

    private static PlayerStats CreateBaseStartingStats()
    {
        PlayerStats stats = new PlayerStats();

        foreach (PlayerStatType statType in StartingStatTypes)
        {
            stats.SetValue(statType, StartingStatValue);
        }

        return stats;
    }

    private static void ApplyPositionBonus(PlayerStats stats, POSITION position)
    {
        switch (position)
        {
            case POSITION.OutsideHitter:
                AddStartingBonus(stats, PlayerStatType.Power, 4);
                AddStartingBonus(stats, PlayerStatType.Jump, 3);
                AddStartingBonus(stats, PlayerStatType.Receive, 3);
                break;
            case POSITION.OppositeHitter:
                AddStartingBonus(stats, PlayerStatType.Power, 5);
                AddStartingBonus(stats, PlayerStatType.Serve, 3);
                AddStartingBonus(stats, PlayerStatType.Jump, 3);
                break;
            case POSITION.MiddleBlocker:
                AddStartingBonus(stats, PlayerStatType.Block, 5);
                AddStartingBonus(stats, PlayerStatType.Jump, 4);
                AddStartingBonus(stats, PlayerStatType.Power, 3);
                break;
            case POSITION.Setter:
                AddStartingBonus(stats, PlayerStatType.Set, 5);
                AddStartingBonus(stats, PlayerStatType.Mental, 4);
                AddStartingBonus(stats, PlayerStatType.Speed, 3);
                break;
            case POSITION.Libero:
                AddStartingBonus(stats, PlayerStatType.Receive, 5);
                AddStartingBonus(stats, PlayerStatType.Speed, 4);
                AddStartingBonus(stats, PlayerStatType.Mental, 3);
                break;
            case POSITION.DefensiveSpecialist:
                AddStartingBonus(stats, PlayerStatType.Receive, 4);
                AddStartingBonus(stats, PlayerStatType.Speed, 4);
                AddStartingBonus(stats, PlayerStatType.Stamina, 3);
                break;
        }
    }

    private static void AddStartingBonus(PlayerStats stats, PlayerStatType statType, int amount)
    {
        stats.AddValue(statType, amount, MaxStatValue);
    }

    private static void ResetCareerSaveData(POSITION selectedPosition, PlayerStats startingStats)
    {
        PlayerSaveManager.ResetMoneySave(StartingMoney);
        PlayerSaveManager.ResetCareerSave(StartingYear, StartingMonth);
        PlayerSaveManager.ResetSeasonCoachEvaluationScore();
        PlayerSaveManager.SaveSelectedPosition(selectedPosition);

        if (!TryResetScenePlayer(selectedPosition, startingStats))
        {
            PlayerSaveManager.SavePlayerStats(PlayerDataSaveId, startingStats);
        }
    }

    private static bool TryResetScenePlayer(POSITION selectedPosition, PlayerStats startingStats)
    {
        bool didReset = false;
        CharacterRole[] characterRoles = Object.FindObjectsByType<CharacterRole>(FindObjectsSortMode.None);

        foreach (CharacterRole characterRole in characterRoles)
        {
            if (characterRole.GetComponent<Player>() == null)
            {
                continue;
            }

            characterRole.ResetSavedStatsToNewCareer(selectedPosition, startingStats);
            didReset = true;
        }

        return didReset;
    }

}
