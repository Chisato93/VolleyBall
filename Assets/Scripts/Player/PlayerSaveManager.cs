using UnityEngine;

public static class PlayerSaveManager
{
    public static int StateVersion { get; private set; }

    [System.Serializable]
    private class PurchasedItemsData
    {
        public string[] itemIds = System.Array.Empty<string>();
    }

    private const string PurchasedItemsPrefsKey = "PurchasedInventoryItems";

    public static string[] LoadPurchasedItemIds()
    {
        string json = PlayerPrefs.GetString(PurchasedItemsPrefsKey, string.Empty);
        if (string.IsNullOrEmpty(json))
        {
            return System.Array.Empty<string>();
        }

        return JsonUtility.FromJson<PurchasedItemsData>(json)?.itemIds ?? System.Array.Empty<string>();
    }

    public static void AddPurchasedItemId(string itemId)
    {
        var ids = new System.Collections.Generic.List<string>(LoadPurchasedItemIds());
        ids.Add(itemId);
        PlayerPrefs.SetString(PurchasedItemsPrefsKey, JsonUtility.ToJson(new PurchasedItemsData { itemIds = ids.ToArray() }));
        PlayerPrefs.Save();
    }

    private const string OwnedItemsPrefsKey = "OwnedInventoryItems";

    public static string[] LoadOwnedItemIds()
    {
        if (!PlayerPrefs.HasKey(OwnedItemsPrefsKey)) return null;
        return JsonUtility.FromJson<PurchasedItemsData>(PlayerPrefs.GetString(OwnedItemsPrefsKey))?.itemIds
            ?? System.Array.Empty<string>();
    }

    public static void SaveOwnedItemIds(string[] ids)
    {
        PlayerPrefs.SetString(OwnedItemsPrefsKey, JsonUtility.ToJson(new PurchasedItemsData { itemIds = ids }));
        PlayerPrefs.Save();
    }

    [System.Serializable]
    public class OwnedItemRecord
    {
        public string itemId;
        public bool isEquipped;
        public int enhancementLevel;
        public bool enhancementAttempted;
        public bool hasRolledOptions;
        public ItemStatBonus[] additionalStatBonuses;
        public ItemProbabilityBonus[] additionalProbabilityBonuses;
        public int optionRulesVersion;
    }

    [System.Serializable]
    private class OwnedItemRecords
    {
        public OwnedItemRecord[] items;
    }

    private const string OwnedItemRecordsKey = "OwnedInventoryItemRecords";

    public static OwnedItemRecord[] LoadOwnedItemRecords()
    {
        if (!PlayerPrefs.HasKey(OwnedItemRecordsKey)) return null;
        return JsonUtility.FromJson<OwnedItemRecords>(PlayerPrefs.GetString(OwnedItemRecordsKey))?.items
            ?? System.Array.Empty<OwnedItemRecord>();
    }

    public static void SaveOwnedItemRecords(OwnedItemRecord[] items)
    {
        PlayerPrefs.SetString(OwnedItemRecordsKey, JsonUtility.ToJson(new OwnedItemRecords { items = items }));
        PlayerPrefs.Save();
        StateVersion++;
    }

    public static void ResetPurchasedItems()
    {
        PlayerPrefs.DeleteKey(PurchasedItemsPrefsKey);
        PlayerPrefs.DeleteKey(OwnedItemsPrefsKey);
        PlayerPrefs.DeleteKey(OwnedItemRecordsKey);
        PlayerPrefs.Save();
        StateVersion++;
    }

    [System.Serializable]
    private class EnergyData
    {
        public int year;
        public int month;
        public int remaining;
    }

    private const string EnergyPrefsKey = "PlayerRemainingEnergy";

    public static int LoadEnergy(int maximum, int year, int month)
    {
        string json = PlayerPrefs.GetString(EnergyPrefsKey, string.Empty);
        if (string.IsNullOrEmpty(json)) return Mathf.Max(0, maximum);
        EnergyData data = JsonUtility.FromJson<EnergyData>(json);
        if (data == null || data.year != year || data.month != month) return Mathf.Max(0, maximum);
        return Mathf.Clamp(data.remaining, 0, Mathf.Max(0, maximum));
    }

    public static void SaveEnergy(int remaining, int year, int month)
    {
        PlayerPrefs.SetString(EnergyPrefsKey, JsonUtility.ToJson(new EnergyData
        {
            year = year, month = month, remaining = Mathf.Max(0, remaining)
        }));
        PlayerPrefs.Save();
    }

    public static void ResetEnergy()
    {
        PlayerPrefs.DeleteKey(EnergyPrefsKey);
        PlayerPrefs.Save();
    }

    public static void SaveCheckpoint()
    {
        // 각 기능이 갱신한 능력치·보유 돈·장비·경기 기록을 디스크에 확정합니다.
        PlayerPrefs.Save();
    }

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

    public static void LoadPlayerStats(string playerSaveId, PlayerStats stats)
    {
        if (string.IsNullOrWhiteSpace(playerSaveId) || stats == null)
        {
            return;
        }

        stats.Power = LoadStat(playerSaveId, PlayerStatType.Power, stats.Power);
        stats.Jump = LoadStat(playerSaveId, PlayerStatType.Jump, stats.Jump);
        stats.Receive = LoadStat(playerSaveId, PlayerStatType.Receive, stats.Receive);
        stats.Set = LoadStat(playerSaveId, PlayerStatType.Set, stats.Set);
        stats.Block = LoadStat(playerSaveId, PlayerStatType.Block, stats.Block);
        stats.Serve = LoadStat(playerSaveId, PlayerStatType.Serve, stats.Serve);
        stats.Speed = LoadStat(playerSaveId, PlayerStatType.Speed, stats.Speed);
        stats.Stamina = LoadStat(playerSaveId, PlayerStatType.Stamina, stats.Stamina);
        stats.Mental = LoadStat(playerSaveId, PlayerStatType.Mental, stats.Mental);
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
        StateVersion++;
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
        StateVersion++;
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
        StateVersion++;
    }

    private static int LoadStat(PlayerData playerData, PlayerStatType statType, int defaultValue)
    {
        return Mathf.Max(0, PlayerPrefs.GetInt(GetPlayerStatPrefsKey(playerData, statType), defaultValue));
    }

    private static int LoadStat(string playerSaveId, PlayerStatType statType, int defaultValue)
    {
        return Mathf.Max(0, PlayerPrefs.GetInt(GetPlayerStatPrefsKey(playerSaveId, statType), defaultValue));
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
