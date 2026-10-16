using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemStatBonus
{
    public PlayerStatType statType;
    public int amount;
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
    [SerializeField] private Sprite icon;
    [SerializeField] private ItemStatBonus[] statBonuses = Array.Empty<ItemStatBonus>();
    [SerializeField, TextArea] private string[] additionalOptions = Array.Empty<string>();
    [SerializeField] private ItemSetEffect[] setEffects = Array.Empty<ItemSetEffect>();

    public string ItemName => itemName;
    public ItemCategory Category => category;
    public Sprite Icon => icon;
    public IReadOnlyList<ItemStatBonus> StatBonuses => statBonuses ?? Array.Empty<ItemStatBonus>();
    public IReadOnlyList<string> AdditionalOptions => additionalOptions ?? Array.Empty<string>();
    public IReadOnlyList<ItemSetEffect> SetEffects => setEffects ?? Array.Empty<ItemSetEffect>();
}
