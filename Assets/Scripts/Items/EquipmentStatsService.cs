using System;
using System.Collections.Generic;
using UnityEngine;

public static class EquipmentStatsService
{
    private static EquipmentItemCatalog catalog;
    private static int cachedVersion = -1;
    private static PlayerStats bonuses = new PlayerStats();
    private static readonly Dictionary<ItemProbabilityType, int> probabilityBonuses = new Dictionary<ItemProbabilityType, int>();
    private static readonly List<string> activeSets = new List<string>();

    public static ItemData FindItem(string id)
    {
        if (catalog == null) catalog = Resources.Load<EquipmentItemCatalog>("EquipmentItemCatalog");
        return catalog != null ? catalog.FindItem(id) : null;
    }

    private static void Refresh()
    {
        if (cachedVersion == PlayerSaveManager.StateVersion) return;
        bonuses = new PlayerStats();
        probabilityBonuses.Clear();
        activeSets.Clear();
        var equipped = new List<ItemData>();
        var categories = new HashSet<ItemCategory>();
        foreach (PlayerSaveManager.OwnedItemRecord record in PlayerSaveManager.LoadOwnedItemRecords()
            ?? Array.Empty<PlayerSaveManager.OwnedItemRecord>())
        {
            if (record == null || !record.isEquipped) continue;
            ItemData source = FindItem(record.itemId);
            if (source == null || !categories.Add(source.Category)) continue;
            equipped.Add(source);
            foreach (ItemStatBonus bonus in source.StatBonuses)
                if (bonus != null) AddStat(bonus.statType, bonus.amount + Mathf.Clamp(record.enhancementLevel, 0, 5));
            if (source.Rarity == ItemRarity.Rare)
            {
                foreach (ItemStatBonus bonus in record.additionalStatBonuses ?? Array.Empty<ItemStatBonus>())
                    if (bonus != null && bonus.amount > 0) AddStat(bonus.statType, bonus.amount);
            }
            if (source.Rarity == ItemRarity.Legendary)
            {
                foreach (ItemProbabilityBonus bonus in record.additionalProbabilityBonuses ?? Array.Empty<ItemProbabilityBonus>())
                    if (bonus != null) AddProbability(bonus.effectType, bonus.percent);
            }
        }

        var pieceCounts = new Dictionary<string, int>();
        foreach (ItemData item in equipped)
        {
            var names = new HashSet<string>();
            foreach (ItemSetEffect effect in item.SetEffects)
            {
                if (effect == null || string.IsNullOrWhiteSpace(effect.setName) || !names.Add(effect.setName)) continue;
                pieceCounts.TryGetValue(effect.setName, out int count);
                pieceCounts[effect.setName] = count + 1;
            }
        }
        var applied = new HashSet<string>();
        foreach (ItemData item in equipped)
        {
            foreach (ItemSetEffect effect in item.SetEffects)
            {
                if (effect == null || string.IsNullOrWhiteSpace(effect.setName)) continue;
                string key = effect.setName + ":" + effect.requiredPieces + ":" + (int)effect.effectType;
                if (!pieceCounts.TryGetValue(effect.setName, out int count) || count < Mathf.Max(1, effect.requiredPieces)
                    || !applied.Add(key)) continue;
                AddProbability(effect.effectType, effect.percent);
                activeSets.Add($"{effect.setName} ({effect.requiredPieces}개): {effect.Description}");
            }
        }
        cachedVersion = PlayerSaveManager.StateVersion;
    }

    private static void AddStat(PlayerStatType type, int amount)
    {
        bonuses.SetValue(type, (int)Math.Min(1000000L, Math.Max(0L, (long)bonuses.GetValue(type) + amount)));
    }

    private static void AddProbability(ItemProbabilityType type, int percent)
    {
        if (percent <= 0 || !Enum.IsDefined(typeof(ItemProbabilityType), type)) return;
        probabilityBonuses.TryGetValue(type, out int current);
        probabilityBonuses[type] = current + Mathf.Clamp(percent, 0, 100);
    }

    public static IReadOnlyList<string> GetActiveSetEffects()
    {
        Refresh();
        return activeSets.ToArray();
    }

    public static PlayerStats GetBonuses()
    {
        Refresh();
        return bonuses.Clone();
    }

    public static PlayerStats GetEffectiveStats(PlayerStats baseStats)
    {
        Refresh();
        PlayerStats result = baseStats.Clone();
        foreach (PlayerStatType type in Enum.GetValues(typeof(PlayerStatType)))
            result.SetValue(type, (int)Math.Min((long)PlayerStats.MaxValue, Math.Max(0L, (long)baseStats.GetValue(type) + bonuses.GetValue(type))));
        return result;
    }

    public static float GetProbabilityMultiplier(ItemProbabilityType type)
    {
        Refresh();
        return 1f + (probabilityBonuses.TryGetValue(type, out int percent) ? percent * 0.01f : 0f);
    }

    public static float ApplyProbabilityBonus(float baseChance, ItemProbabilityType type)
    {
        return Mathf.Clamp(Mathf.Clamp(baseChance, 0.18f, 0.92f) * GetProbabilityMultiplier(type), 0.18f, 0.92f);
    }

    public static string BuildStatsText(PlayerStats baseStats)
    {
        PlayerStats stats = GetEffectiveStats(baseStats);
        var text = new System.Text.StringBuilder($"능력치 (기본 + 장비, 최대 {PlayerStats.MaxValue})\n");
        foreach (PlayerStatType type in Enum.GetValues(typeof(PlayerStatType)))
            text.AppendLine($"{TrainingCatalog.GetStatName(type)} {stats.GetValue(type)} ({baseStats.GetValue(type)} + {bonuses.GetValue(type)})");
        text.AppendLine();
        text.AppendLine("활성화된 확률 효과 (기존 성공률에 곱연산)");
        bool hasEffect = false;
        foreach (ItemProbabilityType type in Enum.GetValues(typeof(ItemProbabilityType)))
        {
            if (!probabilityBonuses.TryGetValue(type, out int percent) || percent == 0) continue;
            text.AppendLine($"{ItemProbabilityBonus.GetEffectName(type)} +{percent}%");
            hasEffect = true;
        }
        if (!hasEffect) text.AppendLine("없음");
        foreach (string effect in activeSets) text.AppendLine(effect);
        return text.ToString().TrimEnd();
    }
}
