using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ShopProduct
{
    public ItemData item;
    [Min(1)] public int price = 50;
    public bool isRandom;
    public Sprite randomIcon;
    public ItemData[] randomItems = Array.Empty<ItemData>();

    public string DisplayName => isRandom ? "랜덤 아이템" : item != null ? item.ItemName : string.Empty;
    public Sprite Icon => isRandom ? randomIcon : item != null ? item.Icon : null;
}

public class ShopItemCardUI : MonoBehaviour
{
    [SerializeField] private Button selectButton;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text priceText;

    [SerializeField] private ShopUpgradePanel upgradeOwner;
    [SerializeField] private ShopSalePanel saleOwner;
    [SerializeField] private int saleSlotIndex;
    private ShopProduct product;
    [SerializeField] private ShopPanel ownerPanel;

    public void Bind(ShopProduct value, bool selected)
    {
        product = value;
        iconImage.sprite = value.Icon;
        iconImage.enabled = value.Icon != null;
        nameText.text = value.DisplayName;
        priceText.text = $"{value.price:N0}원";
        backgroundImage.color = selected ? new Color(1f, 0.87f, 0.48f) : Color.white;
    }

    public void BindUpgrade(ItemData item, bool selected)
    {
        Bind(new ShopProduct { item = item }, selected);
        priceText.text = item.EnhancementAttempted ? $"강화 완료 +{item.EnhancementLevel}" : "강화 가능";
    }

    public void Select()
    {
        if (upgradeOwner != null)
        {
            upgradeOwner.SelectSlot(saleSlotIndex);
            return;
        }
        if (saleOwner != null)
        {
            saleOwner.SelectSlot(saleSlotIndex);
            return;
        }
        if (ownerPanel != null && product != null)
        {
            ownerPanel.SelectProduct(product);
        }
    }

}
