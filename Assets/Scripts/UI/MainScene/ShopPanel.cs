using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanel : MonoBehaviour
{
    private const int ProductCount = 5;

    [SerializeField] private GameFlowManager gameFlowManager;
    [SerializeField] private InventoryPanel inventoryPanel;
    [SerializeField] private List<ShopProduct> products = new List<ShopProduct>();
    [SerializeField] private ShopItemCardUI[] cards;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text detailsText;
    [SerializeField] private TMP_Text selectedPriceText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject purchasePanel;
    [SerializeField] private GameObject salePanel;
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private Button purchaseMenuButton;
    [SerializeField] private Button saleMenuButton;
    [SerializeField] private Button upgradeMenuButton;
    [SerializeField] private Button leaveShopButton;
    [SerializeField] private Button saleBackButton;
    [SerializeField] private Button upgradeBackButton;
    [SerializeField] private TMP_Text menuMoneyText;
    [SerializeField] private RandomItemRevealPanel randomItemRevealPanel;

    [SerializeField] private EnhancementRevealPanel enhancementRevealPanel;

    private readonly List<ShopProduct> visibleProducts = new List<ShopProduct>();
    private ShopProduct selectedProduct;
    private int lastMoney = -1;
    private bool hasReportedMissingReferences;

    private void OnEnable()
    {
        if (!HasSceneReferences()) return;
        ShowMenu();
    }

    public void Open()
    {
        if (!HasSceneReferences()) return;
        randomItemRevealPanel.Close();
        if (enhancementRevealPanel != null) enhancementRevealPanel.Close();
        gameObject.SetActive(true);
        selectedProduct = null;
        ShowMenu();
    }

    private void OnDisable()
    {
        if (enhancementRevealPanel != null) enhancementRevealPanel.Close();
        if (randomItemRevealPanel != null)
        {
            randomItemRevealPanel.Close();
        }
    }

    public void ShowMenu()
    {
        if (!HasSceneReferences()) return;
        SetScreen(menuPanel);
        RefreshMoney();
    }

    public void ShowPurchase()
    {
        SetScreen(purchasePanel);
        LoadProducts();
    }

    public void ShowSale()
    {
        SetScreen(salePanel);
    }

    public void ShowUpgrade()
    {
        SetScreen(upgradePanel);
    }

    private bool HasSceneReferences()
    {
        bool valid = menuPanel != null && purchasePanel != null && salePanel != null && upgradePanel != null
            && gameFlowManager != null && inventoryPanel != null && moneyText != null && menuMoneyText != null
            && detailsText != null && selectedPriceText != null && messageText != null
            && buyButton != null && closeButton != null
            && purchaseMenuButton != null && saleMenuButton != null && upgradeMenuButton != null
            && leaveShopButton != null && saleBackButton != null && upgradeBackButton != null
            && randomItemRevealPanel != null
            && cards != null && cards.Length == ProductCount;
        if (valid)
        {
            foreach (ShopItemCardUI card in cards) valid &= card != null;
        }

        if (!valid && !hasReportedMissingReferences)
        {
            Debug.LogError("ShopCanvas의 ShopPanel 참조가 비어 있습니다. MainScene의 저장된 UI 연결을 확인하세요.", this);
            hasReportedMissingReferences = true;
        }

        return valid;
    }

    private void SetScreen(GameObject selectedPanel)
    {
        menuPanel.SetActive(selectedPanel == menuPanel);
        purchasePanel.SetActive(selectedPanel == purchasePanel);
        salePanel.SetActive(selectedPanel == salePanel);
        upgradePanel.SetActive(selectedPanel == upgradePanel);
    }

    private void Update()
    {
        if (!HasSceneReferences()) return;
        int money = WalletManager.Instance != null ? WalletManager.Instance.CurrentMoney : 0;
        if (money != lastMoney)
        {
            RefreshMoney();
        }
    }

    private void LoadProducts()
    {
        visibleProducts.Clear();
        foreach (ShopProduct product in products)
        {
            if (product != null && (product.isRandom || product.item != null))
            {
                visibleProducts.Add(product);
            }
        }

        selectedProduct = visibleProducts.Count > 0 ? visibleProducts[0] : null;
        messageText.text = string.Empty;
        RefreshPage();
    }

    public void SelectProduct(ShopProduct product)
    {
        selectedProduct = product;
        messageText.text = string.Empty;
        RefreshPage();
    }

    private void RefreshPage()
    {
        for (int i = 0; i < cards.Length; i++)
        {
            int index = i;
            bool hasProduct = index < visibleProducts.Count;
            cards[i].gameObject.SetActive(hasProduct);
            if (hasProduct)
            {
                ShopProduct product = visibleProducts[index];
                cards[i].Bind(product, product == selectedProduct);
            }
        }

        detailsText.text = selectedProduct == null ? "상품이 없습니다."
            : selectedProduct.isRandom
                ? "<b>랜덤 아이템</b>\n\n일반 85% · 레어 12% · 세트 1% · 레전드 2%\n일반은 상점에서 판매하는 장비 중 하나가 나옵니다.\n레어는 능력치 +옵션, 레전드는 성공 확률 +% 옵션이 무작위로 붙습니다.\n\n구매 즉시 아이템이 지급됩니다.\n이미 가진 장비도 나올 수 있습니다.\n\n각 종류의 인벤토리에 빈칸이 필요합니다."
                : ItemTooltipUI.BuildText(selectedProduct.item);
        selectedPriceText.text = selectedProduct != null ? $"구매 가격: {selectedProduct.price:N0}원" : string.Empty;
        RefreshMoney();
    }

    private void RefreshMoney()
    {
        WalletManager wallet = WalletManager.Instance;
        lastMoney = wallet != null ? wallet.CurrentMoney : 0;
        moneyText.text = $"보유 돈: {lastMoney:N0}원";
        menuMoneyText.text = moneyText.text;
        bool hasProduct = selectedProduct != null && selectedProduct.price > 0;
        buyButton.interactable = hasProduct && wallet != null && inventoryPanel != null
            && wallet.CanSpend(selectedProduct.price);
        if (hasProduct && string.IsNullOrEmpty(messageText.text))
        {
            messageText.text = lastMoney < selectedProduct.price ? "보유 돈이 부족합니다." : "상품을 선택하고 구매 버튼을 눌러 주세요.";
        }
    }

    public void BuySelectedItem()
    {
        if (randomItemRevealPanel == null || randomItemRevealPanel.IsOpen)
        {
            return;
        }

        WalletManager wallet = WalletManager.Instance;
        if (selectedProduct == null || inventoryPanel == null || wallet == null)
        {
            return;
        }

        if (selectedProduct.isRandom && !randomItemRevealPanel.IsConfigured)
        {
            messageText.text = "랜덤 아이템 연출 화면이 연결되지 않았습니다.";
            Debug.LogError("RandomItemRevealPanel의 씬 UI 연결을 확인하세요.", this);
            return;
        }

        List<ItemData> candidates = new List<ItemData>();
        if (selectedProduct.isRandom)
        {
            if (selectedProduct.randomItems != null)
            {
                foreach (ItemData item in selectedProduct.randomItems)
                {
                    if (item != null && !candidates.Contains(item)) candidates.Add(item);
                }
            }
        }
        else if (selectedProduct.item != null)
        {
            candidates.Add(selectedProduct.item);
        }

        if (candidates.Count == 0)
        {
            messageText.text = "상품에 지급할 아이템이 연결되지 않았습니다.";
            return;
        }

        if (selectedProduct.isRandom)
        {
            foreach (ItemRarity rarity in System.Enum.GetValues(typeof(ItemRarity)))
            {
                if (!candidates.Exists(item => item.Rarity == rarity))
                {
                    messageText.text = "랜덤 상품의 등급별 아이템이 연결되지 않았습니다.";
                    return;
                }
            }
        }

        // 특정 종류가 가득 찼다고 다른 보상으로 다시 뽑지 않습니다.
        foreach (ItemData item in candidates)
        {
            if (!inventoryPanel.CanAddPurchasedItem(item.Category))
            {
                messageText.text = selectedProduct.isRandom
                    ? "랜덤 아이템 구매에는 각 종류의 인벤토리 빈칸이 필요합니다."
                    : "해당 종류의 인벤토리가 가득 찼습니다.";
                return;
            }
        }

        if (!wallet.TrySpendMoney(selectedProduct.price))
        {
            messageText.text = "보유 돈이 부족합니다.";
            RefreshMoney();
            return;
        }

        ItemData reward;
        if (selectedProduct.isRandom)
        {
            ItemRarity rarity = GetRandomRarity(Random.Range(0, 100));
            List<ItemData> gradeItems = candidates.FindAll(item => item.Rarity == rarity);
            reward = gradeItems[Random.Range(0, gradeItems.Count)];
        }
        else reward = candidates[0];
        reward = inventoryPanel.AddPurchasedItem(reward);
        PlayerSaveManager.SaveCheckpoint();
        messageText.text = selectedProduct.isRandom ? "랜덤 아이템 구매 완료!" : $"{reward.ItemName} 구매 완료!";
        RefreshMoney();
        if (selectedProduct.isRandom)
        {
            randomItemRevealPanel.Open(reward);
        }
    }

    private static ItemRarity GetRandomRarity(int roll)
    {
        // 정수 0~99: 일반 85칸, 레어 12칸, 세트 1칸, 레전드 2칸.
        if (roll < 85) return ItemRarity.Normal;
        if (roll < 97) return ItemRarity.Rare;
        if (roll < 98) return ItemRarity.Set;
        return ItemRarity.Legendary;
    }

    public void Close()
    {
        if (gameFlowManager != null)
        {
            gameFlowManager.CloseShop();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

}
