using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUpgradePanel : MonoBehaviour
{
    [SerializeField] private InventoryPanel inventoryPanel;
    [SerializeField] private ShopItemCardUI[] cards;
    [SerializeField] private TMP_Text detailsText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text pageText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button enhanceButton;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;

    [SerializeField] private EnhancementRevealPanel revealPanel;
    [SerializeField, Range(0, 100)] private int levelOneChance = 70;
    [SerializeField, Range(0, 100)] private int levelTwoChance = 50;
    [SerializeField, Range(0, 100)] private int levelThreeChance = 20;
    [SerializeField, Range(0, 100)] private int levelFourChance = 10;
    [SerializeField, Range(0, 100)] private int levelFiveChance = 1;
    [SerializeField, Min(1)] private int normalEnhancementCost = 100;
    [SerializeField, Min(1)] private int rareEnhancementCost = 500;
    [SerializeField, Min(1)] private int legendaryEnhancementCost = 2000;
    [SerializeField, Min(1)] private int setEnhancementCost = 1000;

    private List<ItemData> items = new List<ItemData>();
    private int selectedIndex = -1;
    private int page;

    private int GetEnhancementCost(ItemData item)
    {
        switch (item.Rarity)
        {
            case ItemRarity.Rare: return Mathf.Max(1, rareEnhancementCost);
            case ItemRarity.Legendary: return Mathf.Max(1, legendaryEnhancementCost);
            case ItemRarity.Set: return Mathf.Max(1, setEnhancementCost);
            default: return Mathf.Max(1, normalEnhancementCost);
        }
    }

    private int GetSuccessChance(int currentLevel)
    {
        switch (currentLevel)
        {
            case 0: return Mathf.Clamp(levelOneChance, 0, 100);
            case 1: return Mathf.Clamp(levelTwoChance, 0, 100);
            case 2: return Mathf.Clamp(levelThreeChance, 0, 100);
            case 3: return Mathf.Clamp(levelFourChance, 0, 100);
            case 4: return Mathf.Clamp(levelFiveChance, 0, 100);
            default: return 0;
        }
    }

    private void OnEnable()
    {
        page = 0;
        selectedIndex = 0;
        messageText.text = "아이템마다 한 번만, 5회 연속 강화합니다.";
        Refresh();
    }

    private void Refresh()
    {
        items = inventoryPanel.GetSaleItems();
        int pageCount = Mathf.Max(1, (items.Count + cards.Length - 1) / cards.Length);
        page = Mathf.Clamp(page, 0, pageCount - 1);
        selectedIndex = items.Count == 0 ? -1 : Mathf.Clamp(selectedIndex, 0, items.Count - 1);
        for (int i = 0; i < cards.Length; i++)
        {
            int index = page * cards.Length + i;
            cards[i].gameObject.SetActive(index < items.Count);
            if (index < items.Count)
            {
                ItemData item = items[index];
                cards[i].BindUpgrade(item, index == selectedIndex);
            }
        }

        ItemData selected = selectedIndex >= 0 ? items[selectedIndex] : null;
        int enhancementCost = selected != null ? GetEnhancementCost(selected) : 0;
        detailsText.text = selected != null ? ItemTooltipUI.BuildText(selected) : "강화할 아이템이 없습니다.";
        priceText.text = selected == null ? string.Empty : selected.EnhancementAttempted
            ? $"강화 완료 +{selected.EnhancementLevel} · 재강화 불가"
            : $"5회 강화 총비용: {enhancementCost:N0}원\n+1 {levelOneChance}% / +2 {levelTwoChance}% / +3 {levelThreeChance}% / +4 {levelFourChance}% / +5 {levelFiveChance}%\n실패 시 단계 유지 · 총 5회 시도";
        WalletManager wallet = WalletManager.Instance;
        moneyText.text = $"보유 돈: {(wallet != null ? wallet.CurrentMoney : 0):N0}원";
        enhanceButton.interactable = selected != null && !selected.EnhancementAttempted
            && wallet != null && wallet.CanSpend(enhancementCost) && !revealPanel.IsOpen;
        previousButton.interactable = page > 0;
        nextButton.interactable = page < pageCount - 1;
        pageText.text = $"{page + 1} / {pageCount}";
    }

    public void SelectSlot(int slot)
    {
        int index = page * cards.Length + slot;
        if (slot < 0 || slot >= cards.Length || index >= items.Count) return;
        selectedIndex = index;
        messageText.text = string.Empty;
        Refresh();
    }

    public void PreviousPage()
    {
        page = Mathf.Max(0, page - 1);
        selectedIndex = page * cards.Length;
        Refresh();
    }

    public void NextPage()
    {
        if ((page + 1) * cards.Length >= items.Count) return;
        page++;
        selectedIndex = page * cards.Length;
        Refresh();
    }

    public void EnhanceSelectedItem()
    {
        WalletManager wallet = WalletManager.Instance;
        if (wallet == null || revealPanel.IsOpen || !revealPanel.IsConfigured
            || selectedIndex < 0 || selectedIndex >= items.Count) return;
        ItemData selected = items[selectedIndex];
        if (selected.EnhancementAttempted || !inventoryPanel.GetSaleItems().Contains(selected))
        {
            Refresh();
            return;
        }
        int enhancementCost = GetEnhancementCost(selected);
        if (!wallet.TrySpendMoney(enhancementCost))
        {
            messageText.text = "보유 돈이 부족합니다.";
            Refresh();
            return;
        }
        bool[] results = new bool[5];
        int successes = 0;
        for (int i = 0; i < results.Length; i++)
        {
            results[i] = Random.Range(0, 100) < GetSuccessChance(successes);
            if (results[i]) successes++;
        }
        if (!inventoryPanel.TryEnhanceItem(selected, successes))
        {
            wallet.AddMoney(enhancementCost);
            Refresh();
            return;
        }
        // 결과와 재강화 금지를 먼저 저장합니다. 스킵·창 닫기로 다시 뽑을 수 없습니다.
        messageText.text = "강화 완료! 이 아이템은 다시 강화할 수 없습니다.";
        revealPanel.Open(selected, results);
        Refresh();
    }
}
