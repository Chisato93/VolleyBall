using UnityEngine;

public static class PlayerSaveManager
{
    private const string MoneyPrefsKey = "PlayerOwnedMoney";
    private const string CareerYearPrefsKey = "CareerCurrentYear";
    private const string CareerMonthPrefsKey = "CareerCurrentMonth";
    private const string SeasonCoachEvaluationScorePrefsKey = "SeasonCoachEvaluationScore";
    private const string SelectedPositionPrefsKey = "PlayerSelectedPosition";
    private const string PlayerStatsPrefsKeyPrefix = "PlayerStats";
    private const int MinCareerYear = 1;

    public static int LoadMoney(int defaultValue)
    {
        return Mathf.Max(0, PlayerPrefs.GetInt(MoneyPrefsKey, defaultValue));
    }

    public static void SaveMoney(int amount)
    {
        PlayerPrefs.SetInt(MoneyPrefsKey, Mathf.Max(0, amount));
        PlayerPrefs.Save();
    }

    public static int LoadCareerYear(int defaultValue)
    {
        return Mathf.Max(MinCareerYear, PlayerPrefs.GetInt(CareerYearPrefsKey, defaultValue));
    }

    public static int LoadCareerMonth(int defaultValue)
    {
        return PlayerPrefs.GetInt(CareerMonthPrefsKey, defaultValue);
    }

    public static void SaveCareerDate(int year, int month)
    {
        PlayerPrefs.SetInt(CareerYearPrefsKey, Mathf.Max(MinCareerYear, year));
        PlayerPrefs.SetInt(CareerMonthPrefsKey, month);
        PlayerPrefs.Save();
    }

    public static int LoadSeasonCoachEvaluationScore()
    {
        return Mathf.Max(0, PlayerPrefs.GetInt(SeasonCoachEvaluationScorePrefsKey, 0));
    }

    public static int AddSeasonCoachEvaluationScore(int amount)
    {
        int nextScore = Mathf.Max(0, LoadSeasonCoachEvaluationScore() + amount);
        SaveSeasonCoachEvaluationScore(nextScore);
        return nextScore;
    }

    public static void SaveSeasonCoachEvaluationScore(int score)
    {
        PlayerPrefs.SetInt(SeasonCoachEvaluationScorePrefsKey, Mathf.Max(0, score));
        PlayerPrefs.Save();
    }

    public static POSITION LoadSelectedPosition(POSITION defaultValue)
    {
        int savedValue = PlayerPrefs.GetInt(SelectedPositionPrefsKey, (int)defaultValue);
        return System.Enum.IsDefined(typeof(POSITION), savedValue) ? (POSITION)savedValue : defaultValue;
    }

    public static void SaveSelectedPosition(POSITION position)
    {
        PlayerPrefs.SetInt(SelectedPositionPrefsKey, (int)position);
        PlayerPrefs.Save();
    }

    public static void LoadPlayerStats(PlayerData playerData)
    {
        if (playerData == null || playerData.Stats == null)
        {
            return;
        }

        PlayerStats stats = playerData.Stats;
        stats.Power = LoadStat(playerData, PlayerStatType.Power, stats.Power);
        stats.Jump = LoadStat(playerData, PlayerStatType.Jump, stats.Jump);
        stats.Receive = LoadStat(playerData, PlayerStatType.Receive, stats.Receive);
        stats.Set = LoadStat(playerData, PlayerStatType.Set, stats.Set);
        stats.Block = LoadStat(playerData, PlayerStatType.Block, stats.Block);
        stats.Serve = LoadStat(playerData, PlayerStatType.Serve, stats.Serve);
        stats.Speed = LoadStat(playerData, PlayerStatType.Speed, stats.Speed);
        stats.Stamina = LoadStat(playerData, PlayerStatType.Stamina, stats.Stamina);
        stats.Mental = LoadStat(playerData, PlayerStatType.Mental, stats.Mental);
    }

    public static void SavePlayerStats(PlayerData playerData)
    {
        if (playerData == null || playerData.Stats == null)
        {
            return;
        }

        PlayerStats stats = playerData.Stats;
        SaveStat(playerData, PlayerStatType.Power, stats.Power);
        SaveStat(playerData, PlayerStatType.Jump, stats.Jump);
        SaveStat(playerData, PlayerStatType.Receive, stats.Receive);
        SaveStat(playerData, PlayerStatType.Set, stats.Set);
        SaveStat(playerData, PlayerStatType.Block, stats.Block);
        SaveStat(playerData, PlayerStatType.Serve, stats.Serve);
        SaveStat(playerData, PlayerStatType.Speed, stats.Speed);
        SaveStat(playerData, PlayerStatType.Stamina, stats.Stamina);
        SaveStat(playerData, PlayerStatType.Mental, stats.Mental);
        PlayerPrefs.Save();
    }

    public static void SavePlayerStats(string playerSaveId, PlayerStats stats)
    {
        if (string.IsNullOrWhiteSpace(playerSaveId) || stats == null)
        {
            return;
        }

        SaveStat(playerSaveId, PlayerStatType.Power, stats.Power);
        SaveStat(playerSaveId, PlayerStatType.Jump, stats.Jump);
        SaveStat(playerSaveId, PlayerStatType.Receive, stats.Receive);
        SaveStat(playerSaveId, PlayerStatType.Set, stats.Set);
        SaveStat(playerSaveId, PlayerStatType.Block, stats.Block);
        SaveStat(playerSaveId, PlayerStatType.Serve, stats.Serve);
        SaveStat(playerSaveId, PlayerStatType.Speed, stats.Speed);
        SaveStat(playerSaveId, PlayerStatType.Stamina, stats.Stamina);
        SaveStat(playerSaveId, PlayerStatType.Mental, stats.Mental);
        PlayerPrefs.Save();
    }

    public static void ResetCareerSave(int startingYear, int startingMonth)
    {
        SaveCareerDate(startingYear, startingMonth);
    }

    public static void ResetMoneySave(int startingMoney)
    {
        SaveMoney(startingMoney);
    }

    public static void ResetSeasonCoachEvaluationScore()
    {
        SaveSeasonCoachEvaluationScore(0);
    }

    public static void ResetSelectedPosition()
    {
        PlayerPrefs.DeleteKey(SelectedPositionPrefsKey);
        PlayerPrefs.Save();
    }

    public static void ResetPlayerStats(PlayerData playerData)
    {
        if (playerData == null)
        {
            return;
        }

        DeleteStat(playerData, PlayerStatType.Power);
        DeleteStat(playerData, PlayerStatType.Jump);
        DeleteStat(playerData, PlayerStatType.Receive);
        DeleteStat(playerData, PlayerStatType.Set);
        DeleteStat(playerData, PlayerStatType.Block);
        DeleteStat(playerData, PlayerStatType.Serve);
        DeleteStat(playerData, PlayerStatType.Speed);
        DeleteStat(playerData, PlayerStatType.Stamina);
        DeleteStat(playerData, PlayerStatType.Mental);
        PlayerPrefs.Save();
    }

    private static int LoadStat(PlayerData playerData, PlayerStatType statType, int defaultValue)
    {
        return Mathf.Max(0, PlayerPrefs.GetInt(GetPlayerStatPrefsKey(playerData, statType), defaultValue));
    }

    private static void SaveStat(PlayerData playerData, PlayerStatType statType, int value)
    {
        SaveStat(GetPlayerSaveId(playerData), statType, value);
    }

    private static void SaveStat(string playerSaveId, PlayerStatType statType, int value)
    {
        PlayerPrefs.SetInt(GetPlayerStatPrefsKey(playerSaveId, statType), Mathf.Max(0, value));
    }

    private static void DeleteStat(PlayerData playerData, PlayerStatType statType)
    {
        PlayerPrefs.DeleteKey(GetPlayerStatPrefsKey(playerData, statType));
    }

    private static string GetPlayerStatPrefsKey(PlayerData playerData, PlayerStatType statType)
    {
        return GetPlayerStatPrefsKey(GetPlayerSaveId(playerData), statType);
    }

    private static string GetPlayerStatPrefsKey(string playerSaveId, PlayerStatType statType)
    {
        return $"{PlayerStatsPrefsKeyPrefix}.{playerSaveId}.{statType}";
    }

    private static string GetPlayerSaveId(PlayerData playerData)
    {
        if (!string.IsNullOrWhiteSpace(playerData.PlayerName))
        {
            return playerData.PlayerName;
        }

        return $"{playerData.Position}.{playerData.Number}";
    }
}
