using System;
using System.Collections.Generic;
using UnityEngine;

public enum ItemRarity { Normal, Rare, Legendary, Set }

[Serializable]
public class ItemStatBonus
{
    public PlayerStatType statType;
    public int amount;
}

public enum ItemProbabilityType { AttackSuccess, ServeSuccess, ReceiveSuccess, BlockSuccess }

[Serializable]
public class ItemProbabilityBonus
{
    public ItemProbabilityType effectType;
    public int percent;
}

[Serializable]
public class ItemSetEffect
{
    public string setName;
    [Min(1)] public int requiredPieces = 2;
    [TextArea] public string description;
}

[CreateAssetMenu(fileName = "ItemData", menuName = "Volleyball/Item Data")]
public class ItemData : ScriptableObject
{
    [SerializeField] private string itemName;
    [SerializeField] private ItemCategory category;
    [SerializeField] private ItemRarity rarity;
    [SerializeField, Min(1)] private int minimumOptionCount = 1;
    [SerializeField, Min(1)] private int maximumOptionCount = 2;
    [SerializeField, Min(1)] private int minimumOptionAmount = 1;
    [SerializeField, Min(1)] private int maximumOptionAmount = 3;
    [SerializeField] private Sprite icon;
    [SerializeField] private ItemStatBonus[] statBonuses = Array.Empty<ItemStatBonus>();
    [SerializeField] private ItemSetEffect[] setEffects = Array.Empty<ItemSetEffect>();

    [SerializeField, Min(1)] private int sellPrice = 25;

    public int SellPrice => Mathf.Max(1, sellPrice);
    [NonSerialized] private bool isOwnedInstance;
    [NonSerialized] private bool enhancementAttempted;
    [NonSerialized] private int enhancementLevel;

    [NonSerialized] private ItemStatBonus[] rolledOptions = Array.Empty<ItemStatBonus>();

    [NonSerialized] private ItemProbabilityBonus[] rolledProbabilityOptions = Array.Empty<ItemProbabilityBonus>();

    public IReadOnlyList<ItemProbabilityBonus> AdditionalProbabilityBonuses => rolledProbabilityOptions;
    public ItemRarity Rarity => rarity;
    public string RarityName => rarity == ItemRarity.Rare ? "레어" : rarity == ItemRarity.Legendary ? "레전드"
        : rarity == ItemRarity.Set ? "세트" : "일반";
    public IReadOnlyList<ItemStatBonus> AdditionalStatBonuses => rolledOptions;

    public bool EnhancementAttempted => enhancementAttempted;
    public int EnhancementLevel => enhancementLevel;
    public string ItemName => enhancementLevel > 0 ? $"{itemName} +{enhancementLevel}" : itemName;

    public ItemData CreateOwnedInstance(int level = 0, bool attempted = false)
    {
        ItemData instance = Instantiate(this);
        instance.name = name;
        instance.isOwnedInstance = true;
        instance.enhancementLevel = Mathf.Clamp(level, 0, 5);
        instance.enhancementAttempted = attempted;
        instance.rolledOptions = new List<ItemStatBonus>(rolledOptions ?? Array.Empty<ItemStatBonus>()).ToArray();
        instance.rolledProbabilityOptions = CopyAdditionalProbabilityBonuses();
        if (!isOwnedInstance) instance.RollAdditionalOptions();
        return instance;
    }

    private void RollAdditionalOptions()
    {
        rolledOptions = Array.Empty<ItemStatBonus>();
        rolledProbabilityOptions = Array.Empty<ItemProbabilityBonus>();
        if (rarity == ItemRarity.Legendary)
        {
            RollProbabilityOptions();
            return;
        }
        if (rarity != ItemRarity.Rare) return;
        var pool = new List<PlayerStatType>((PlayerStatType[])Enum.GetValues(typeof(PlayerStatType)));
        int maxCount = Mathf.Clamp(maximumOptionCount, 1, pool.Count);
        int minCount = Mathf.Clamp(minimumOptionCount, 1, maxCount);
        int count = UnityEngine.Random.Range(minCount, maxCount + 1);
        int minAmount = Mathf.Clamp(minimumOptionAmount, 1, 1000);
        int maxAmount = Mathf.Clamp(maximumOptionAmount, minAmount, 1000);
        rolledOptions = new ItemStatBonus[count];
        for (int i = 0; i < count; i++)
        {
            int index = UnityEngine.Random.Range(0, pool.Count);
            rolledOptions[i] = new ItemStatBonus
            {
                statType = pool[index],
                amount = UnityEngine.Random.Range(minAmount, maxAmount + 1)
            };
            pool.RemoveAt(index);
        }
    }

    public ItemStatBonus[] CopyAdditionalStatBonuses()
    {
        var copy = new List<ItemStatBonus>();
        foreach (ItemStatBonus bonus in rolledOptions)
            copy.Add(new ItemStatBonus { statType = bonus.statType, amount = bonus.amount });
        return copy.ToArray();
    }

    public void RestoreAdditionalStatBonuses(ItemStatBonus[] options)
    {
        rolledOptions = Array.Empty<ItemStatBonus>();
        if (rarity != ItemRarity.Rare) return;
        var restored = new List<ItemStatBonus>();
        foreach (ItemStatBonus option in options ?? Array.Empty<ItemStatBonus>())
        {
            if (option == null || option.amount == 0 || !Enum.IsDefined(typeof(PlayerStatType), option.statType)) continue;
            restored.Add(new ItemStatBonus { statType = option.statType, amount = Mathf.Clamp(option.amount, 1, 1000) });
        }
        rolledOptions = restored.ToArray();
    }

    private void RollProbabilityOptions()
    {
        var pool = new List<ItemProbabilityType>((ItemProbabilityType[])Enum.GetValues(typeof(ItemProbabilityType)));
        int maxCount = Mathf.Clamp(maximumOptionCount, 1, pool.Count);
        int count = UnityEngine.Random.Range(Mathf.Clamp(minimumOptionCount, 1, maxCount), maxCount + 1);
        int minAmount = Mathf.Clamp(minimumOptionAmount, 1, 100);
        int maxAmount = Mathf.Clamp(maximumOptionAmount, minAmount, 100);
        rolledProbabilityOptions = new ItemProbabilityBonus[count];
        for (int i = 0; i < count; i++)
        {
            int index = UnityEngine.Random.Range(0, pool.Count);
            rolledProbabilityOptions[i] = new ItemProbabilityBonus
            {
                effectType = pool[index], percent = UnityEngine.Random.Range(minAmount, maxAmount + 1)
            };
            pool.RemoveAt(index);
        }
    }

    public ItemProbabilityBonus[] CopyAdditionalProbabilityBonuses()
    {
        var copy = new List<ItemProbabilityBonus>();
        foreach (ItemProbabilityBonus option in rolledProbabilityOptions)
            copy.Add(new ItemProbabilityBonus { effectType = option.effectType, percent = option.percent });
        return copy.ToArray();
    }

    public void RestoreAdditionalProbabilityBonuses(ItemProbabilityBonus[] options)
    {
        rolledProbabilityOptions = Array.Empty<ItemProbabilityBonus>();
        if (rarity != ItemRarity.Legendary) return;
        var restored = new List<ItemProbabilityBonus>();
        foreach (ItemProbabilityBonus option in options ?? Array.Empty<ItemProbabilityBonus>())
        {
            if (option == null || option.percent <= 0 || !Enum.IsDefined(typeof(ItemProbabilityType), option.effectType)) continue;
            restored.Add(new ItemProbabilityBonus { effectType = option.effectType, percent = Mathf.Clamp(option.percent, 1, 100) });
        }
        rolledProbabilityOptions = restored.ToArray();
    }

    public void MigrateLegacyOptions(ItemStatBonus[] options)
    {
        // 기존 레어의 감소 수치는 같은 종류·크기의 증가 수치로 교정합니다.
        if (rarity == ItemRarity.Rare)
        {
            var converted = new List<ItemStatBonus>();
            foreach (ItemStatBonus option in options ?? Array.Empty<ItemStatBonus>())
                if (option != null) converted.Add(new ItemStatBonus
                { statType = option.statType, amount = (int)Math.Min(1000L, Math.Abs((long)option.amount)) });
            RestoreAdditionalStatBonuses(converted.ToArray());
        }
        // 레전드는 새 규칙으로 생성된 확률 옵션을 저장해 다음 로드부터 유지합니다.
    }

    public bool TryApplyEnhancement(int successes)
    {
        if (!isOwnedInstance || enhancementAttempted) return false;
        enhancementAttempted = true;
        enhancementLevel = Mathf.Clamp(successes, 0, 5);
        return true;
    }
    public ItemCategory Category => category;
    public Sprite Icon => icon;
    public IReadOnlyList<ItemStatBonus> StatBonuses
    {
        get
        {
            if (enhancementLevel == 0) return statBonuses ?? Array.Empty<ItemStatBonus>();
            var bonuses = new List<ItemStatBonus>();
            foreach (ItemStatBonus bonus in statBonuses ?? Array.Empty<ItemStatBonus>())
            {
                if (bonus != null) bonuses.Add(new ItemStatBonus
                {
                    statType = bonus.statType,
                    amount = bonus.amount + enhancementLevel
                });
            }
            return bonuses;
        }
    }
    public IReadOnlyList<ItemSetEffect> SetEffects => rarity == ItemRarity.Set ? setEffects ?? Array.Empty<ItemSetEffect>() : Array.Empty<ItemSetEffect>();
}
