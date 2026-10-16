using System;
using System.Text;
using TMPro;
using UnityEngine;

public class ItemTooltipUI : MonoBehaviour
{
    [SerializeField] private TMP_Text contentText;
    [SerializeField] private float preferredWidth = 360f;
    [SerializeField] private Vector2 contentPadding = new Vector2(36f, 28f);

    public void SetItem(ItemData item, Vector2 availableSize, bool isEquipped = false,
        Func<string, int> getSetPieceCount = null)
    {
        RectTransform panel = (RectTransform)transform;
        float width = Mathf.Min(preferredWidth, availableSize.x);
        contentText.text = BuildText(item, isEquipped, getSetPieceCount);
        contentText.enableAutoSizing = false;
        contentText.fontSize = contentText.fontSizeMax;
        Vector2 preferred = contentText.GetPreferredValues(contentText.text,
            Mathf.Max(1f, width - contentPadding.x), float.PositiveInfinity);
        float height = Mathf.Min(preferred.y + contentPadding.y, availableSize.y);
        panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        contentText.enableAutoSizing = preferred.y + contentPadding.y > availableSize.y;
    }

    public static string BuildText(ItemData item, bool isEquipped = false,
        Func<string, int> getSetPieceCount = null)
    {
        if (item == null)
        {
            return string.Empty;
        }

        StringBuilder builder = new StringBuilder();
        builder.AppendLine($"<b>{Escape(item.ItemName)}</b>");
        builder.AppendLine(GetCategoryName(item.Category));
        bool hasOptions = false;
        bool hasSection = false;

        foreach (ItemStatBonus bonus in item.StatBonuses)
        {
            if (bonus == null || bonus.amount == 0)
            {
                continue;
            }

            if (!hasSection)
            {
                AppendHeading(builder, "능력치");
                hasSection = true;
            }

            builder.AppendLine(HighlightActive($"{GetStatName(bonus.statType)} {bonus.amount:+0;-0;0}", isEquipped));
            hasOptions = true;
        }

        hasSection = false;
        foreach (string option in item.AdditionalOptions)
        {
            if (string.IsNullOrWhiteSpace(option))
            {
                continue;
            }

            if (!hasSection)
            {
                AppendHeading(builder, "추가 옵션");
                hasSection = true;
            }

            builder.AppendLine(HighlightActive(Escape(option), isEquipped));
            hasOptions = true;
        }

        hasSection = false;
        foreach (ItemSetEffect effect in item.SetEffects)
        {
            if (effect == null || string.IsNullOrWhiteSpace(effect.description))
            {
                continue;
            }

            if (!hasSection)
            {
                AppendHeading(builder, "세트 효과");
                hasSection = true;
            }

            int requiredPieces = Mathf.Max(1, effect.requiredPieces);
            int equippedPieces = getSetPieceCount != null ? getSetPieceCount(effect.setName) : 0;
            bool isActive = isEquipped && equippedPieces >= requiredPieces;
            builder.AppendLine(HighlightActive($"{Escape(effect.setName)} ({equippedPieces}/{requiredPieces}개)", isActive));
            builder.AppendLine(HighlightActive(Escape(effect.description), isActive));
            hasOptions = true;
        }

        if (!hasOptions)
        {
            builder.AppendLine();
            builder.AppendLine("옵션 없음");
        }

        return builder.ToString().TrimEnd();
    }

    private static void AppendHeading(StringBuilder builder, string heading)
    {
        builder.AppendLine();
        builder.AppendLine($"<color=#FFD940><b>{heading}</b></color>");
    }

    private static string HighlightActive(string text, bool isActive)
    {
        return isActive ? $"<color=#65DC8A>{text}</color>" : text;
    }

    private static string Escape(string text)
    {
        return (text ?? string.Empty).Replace("<", "＜").Replace(">", "＞");
    }

    private static string GetCategoryName(ItemCategory category)
    {
        switch (category)
        {
            case ItemCategory.Head: return "머리 장비";
            case ItemCategory.Body: return "상의";
            case ItemCategory.Shoes: return "신발";
            case ItemCategory.Accessory: return "장신구";
            default: return "장비";
        }
    }

    private static string GetStatName(PlayerStatType statType)
    {
        switch (statType)
        {
            case PlayerStatType.Power: return "파워";
            case PlayerStatType.Jump: return "점프";
            case PlayerStatType.Receive: return "리시브";
            case PlayerStatType.Set: return "토스";
            case PlayerStatType.Block: return "블록";
            case PlayerStatType.Serve: return "서브";
            case PlayerStatType.Speed: return "속도";
            case PlayerStatType.Stamina: return "체력";
            case PlayerStatType.Mental: return "멘탈";
            default: return statType.ToString();
        }
    }
}
