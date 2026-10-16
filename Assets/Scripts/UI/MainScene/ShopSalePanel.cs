using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopSalePanel : MonoBehaviour
{
    [SerializeField] private InventoryPanel inventoryPanel;
    [SerializeField] private ShopItemCardUI[] cards;
    [SerializeField] private TMP_Text detailsText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text pageText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button sellButton;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;

    private List<ItemData> items = new List<ItemData>();
    private int selectedIndex = -1;
    private int page;

    private void OnEnable()
    {
        page = 0;
        selectedIndex = 0;
        messageText.text = "착용하지 않은 아이템을 판매할 수 있습니다.";
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
                cards[i].Bind(new ShopProduct { item = item, price = item.SellPrice }, index == selectedIndex);
            }
        }

        ItemData selected = selectedIndex >= 0 ? items[selectedIndex] : null;
        detailsText.text = selected != null ? ItemTooltipUI.BuildText(selected) : "판매할 아이템이 없습니다.";
        priceText.text = selected != null ? $"판매하면 받는 금액: {selected.SellPrice:N0}원" : string.Empty;
        WalletManager wallet = WalletManager.Instance;
        moneyText.text = $"보유 돈: {(wallet != null ? wallet.CurrentMoney : 0):N0}원";
        sellButton.interactable = selected != null && wallet != null;
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

    public void SellSelectedItem()
    {
        WalletManager wallet = WalletManager.Instance;
        if (wallet == null || selectedIndex < 0 || selectedIndex >= items.Count) return;
        ItemData selected = items[selectedIndex];
        if ((long)wallet.CurrentMoney + selected.SellPrice > int.MaxValue)
        {
            messageText.text = "보유할 수 있는 금액을 초과합니다.";
            return;
        }
        if (inventoryPanel.TryRemoveSaleItem(selected))
        {
            wallet.AddMoney(selected.SellPrice);
            PlayerSaveManager.SaveCheckpoint();
            messageText.text = $"{selected.ItemName} 판매 완료! +{selected.SellPrice:N0}원";
        }
        else messageText.text = "아이템 목록이 바뀌었습니다. 다시 선택해 주세요.";
        Refresh();
    }
}
