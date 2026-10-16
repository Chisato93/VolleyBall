using System.Collections.Generic;
using UnityEngine;

public class InventoryPanel : TabContentPanel
{
    private const int SlotColumnCount = 5;
    private const int SlotRowCount = 4;
    private const int SlotCount = SlotColumnCount * SlotRowCount;
    private static readonly Vector2 TooltipOffset = new Vector2(16f, -16f);

    [SerializeField] private Transform slotsParent;
    [SerializeField] private Transform equipmentSlotsParent;
    [SerializeField] private InventoryItemUI itemPrefab;
    [SerializeField] private RectTransform tooltipPanel;
    [SerializeField] private ItemTooltipUI tooltipView;
    [SerializeField] private RectTransform dragPreviewRoot;
    [SerializeField] private List<ItemData> ownedItems = new List<ItemData>();
    [SerializeField] private ItemData[] purchaseCatalog;

    private readonly List<InventorySlotUI> slotList = new List<InventorySlotUI>();
    private readonly List<InventorySlotUI> equipmentSlots = new List<InventorySlotUI>();
    private readonly List<ItemData> displayedItems = new List<ItemData>();
    private Canvas rootCanvas;
    private InventorySlotUI tooltipOwner;
    private InventorySlotUI dragSourceSlot;
    private GameObject draggedItemObject;
    private Transform draggedItemOriginalParent;
    private int draggedItemOriginalSiblingIndex;
    private bool dragCompleted;
    private int currentTabIndex;
    private bool hasInitializedOwnedItems;
    private readonly Vector3[] canvasCorners = new Vector3[4];

    public static InventoryPanel ActivePanel { get; private set; }

    private void Awake()
    {
        ActivePanel = this;
        InitializeEquipmentSlots();
        EnsureSlots();
    }

    private void OnDestroy()
    {
        if (ActivePanel == this)
        {
            ActivePanel = null;
        }
    }

    private void Start()
    {
        InitializeOwnedItems();
        RenderCurrentTab();
    }

    private void InitializeOwnedItems()
    {
        if (hasInitializedOwnedItems)
        {
            return;
        }

        hasInitializedOwnedItems = true;
        PlayerSaveManager.OwnedItemRecord[] records = PlayerSaveManager.LoadOwnedItemRecords();
        if (records != null)
        {
            ownedItems.Clear();
            foreach (PlayerSaveManager.OwnedItemRecord record in records)
            {
                ItemData source = FindCatalogItem(record.itemId);
                if (source != null)
                {
                    ItemData instance = source.CreateOwnedInstance(record.enhancementLevel, record.enhancementAttempted);
                    if (record.hasRolledOptions)
                    {
                        if (record.optionRulesVersion < 2) instance.MigrateLegacyOptions(record.additionalStatBonuses);
                        else
                        {
                            instance.RestoreAdditionalStatBonuses(record.additionalStatBonuses);
                            instance.RestoreAdditionalProbabilityBonuses(record.additionalProbabilityBonuses);
                        }
                    }
                    ownedItems.Add(instance);
                }
            }
        }
        else
        {
            string[] savedItems = PlayerSaveManager.LoadOwnedItemIds();
            if (savedItems != null) ownedItems.Clear();
            foreach (string id in savedItems ?? PlayerSaveManager.LoadPurchasedItemIds())
            {
                ItemData source = FindCatalogItem(id);
                if (source != null) ownedItems.Add(source);
            }
            for (int i = 0; i < ownedItems.Count; i++)
            {
                if (ownedItems[i] != null) ownedItems[i] = ownedItems[i].CreateOwnedInstance();
            }
        }

        ResetDisplayedItems();
        SaveOwnership();
    }

    private ItemData FindCatalogItem(string id)
    {
        if (purchaseCatalog == null) return null;
        foreach (ItemData item in purchaseCatalog)
        {
            if (item != null && item.name == id) return item;
        }
        return null;
    }

    public bool CanAddPurchasedItem(ItemCategory category)
    {
        InitializeOwnedItems();
        int count = 0;
        foreach (ItemData item in displayedItems)
        {
            if (item != null && item.Category == category)
            {
                count++;
            }
        }

        return count < SlotCount;
    }

    public ItemData AddPurchasedItem(ItemData item)
    {
        InitializeOwnedItems();
        return AddItem(item);
    }

    private void OnDisable()
    {
        HideTooltip(null);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            AddRandomItemToCurrentTab();
        }
    }

    public override void SetActiveTab(int index)
    {
        currentTabIndex = index;
        RenderCurrentTab();
    }

    private void RenderCurrentTab()
    {
        HideTooltip(null);
        EnsureSlots();
        ApplyCurrentTabCategoryToSlots();
        ClearSlots();

        int slotIndex = 0;
        for (int i = 0; i < displayedItems.Count; i++)
        {
            ItemData item = displayedItems[i];
            if (item == null || !IsItemInCurrentTab(item))
            {
                continue;
            }

            if (slotIndex >= slotList.Count)
            {
                return;
            }

            slotList[slotIndex].SetItem(item, itemPrefab);
            slotIndex++;
        }
    }

    private void AddRandomItemToCurrentTab()
    {
        EnsureSlots();

        if (GetVisibleItemCount() >= slotList.Count)
        {
            Debug.LogWarning("InventoryPanel cannot create more items because every slot is full.");
            return;
        }

        ItemData item = GetRandomCandidate();
        if (item == null)
        {
            Debug.LogWarning("InventoryPanel cannot create a random item because there is no matching item.");
            return;
        }

        AddItem(item);
    }

    private int GetVisibleItemCount()
    {
        int count = 0;
        for (int i = 0; i < displayedItems.Count; i++)
        {
            ItemData item = displayedItems[i];
            if (item != null && IsItemInCurrentTab(item))
            {
                count++;
            }
        }

        return count;
    }

    private ItemData GetRandomCandidate()
    {
        List<ItemData> candidates = new List<ItemData>();
        for (int i = 0; i < ownedItems.Count; i++)
        {
            ItemData item = ownedItems[i];
            if (item != null && IsItemInCurrentTab(item))
            {
                candidates.Add(item);
            }
        }

        if (candidates.Count == 0)
        {
            return null;
        }

        return candidates[Random.Range(0, candidates.Count)];
    }

    private bool IsItemInCurrentTab(ItemData item)
    {
        return item.Category == GetCategory(currentTabIndex);
    }

    private ItemCategory GetCategory(int tabIndex)
    {
        switch (tabIndex)
        {
            case 0:
                return ItemCategory.Head;
            case 1:
                return ItemCategory.Body;
            case 2:
                return ItemCategory.Shoes;
            case 3:
                return ItemCategory.Accessory;
            default:
                return ItemCategory.Head;
        }
    }

    private void ClearSlots()
    {
        for (int i = 0; i < slotList.Count; i++)
        {
            if (slotList[i] != null)
            {
                slotList[i].Clear();
            }
        }
    }

    private void EnsureSlots()
    {
        ResolveSlotsParent();
        if (slotsParent == null)
        {
            return;
        }

        CacheExistingSlots();

        if (slotList.Count < SlotCount)
        {
            Debug.LogWarning($"InventoryPanel needs {SlotCount} slot objects in the hierarchy, but found {slotList.Count}.");
        }

        for (int i = 0; i < slotList.Count; i++)
        {
            if (slotList[i] != null)
            {
                slotList[i].Initialize(this);
                slotList[i].gameObject.SetActive(i < SlotCount);
            }
        }
    }

    private void ResolveSlotsParent()
    {
        if (slotsParent != null)
        {
            return;
        }

        Transform foundSlots = transform.Find("Slots");
        if (foundSlots != null)
        {
            slotsParent = foundSlots;
        }
    }

    private void InitializeEquipmentSlots()
    {
        equipmentSlots.Clear();
        if (equipmentSlotsParent == null)
        {
            return;
        }

        equipmentSlots.AddRange(equipmentSlotsParent.GetComponentsInChildren<InventorySlotUI>(true));
        foreach (InventorySlotUI slot in equipmentSlots)
        {
            slot.Initialize(this, true);
        }
    }

    private int GetEquippedSetPieceCount(string setName)
    {
        if (string.IsNullOrWhiteSpace(setName))
        {
            return 0;
        }

        int count = 0;
        foreach (InventorySlotUI slot in equipmentSlots)
        {
            if (slot == null || !slot.HasItem)
            {
                continue;
            }

            foreach (ItemSetEffect effect in slot.CurrentItemData.SetEffects)
            {
                if (effect != null && effect.setName == setName)
                {
                    count++;
                    break;
                }
            }
        }

        return count;
    }

    private void CacheExistingSlots()
    {
        slotList.Clear();

        for (int i = 0; i < slotsParent.childCount; i++)
        {
            InventorySlotUI slot = slotsParent.GetChild(i).GetComponent<InventorySlotUI>();
            if (slot != null)
            {
                slotList.Add(slot);
            }
        }
    }

    private void ApplyCurrentTabCategoryToSlots()
    {
        ItemCategory category = GetCategory(currentTabIndex);
        for (int i = 0; i < slotList.Count && i < SlotCount; i++)
        {
            if (slotList[i] != null)
            {
                slotList[i].SetAllowedCategory(category);
            }
        }
    }

    public void ShowTooltip(InventorySlotUI slot, Vector2 screenPosition)
    {
        if (slot == null || !slot.HasItem || dragSourceSlot != null)
        {
            return;
        }

        if (tooltipPanel == null)
        {
            return;
        }

        ResolveRootCanvas();
        if (tooltipView == null)
        {
            tooltipView = tooltipPanel.GetComponent<ItemTooltipUI>();
        }

        if (tooltipView == null || !TryGetTooltipBounds(out Rect bounds))
        {
            return;
        }

        tooltipOwner = slot;
        RectTransform parent = (RectTransform)tooltipPanel.parent;
        tooltipPanel.anchorMin = parent.pivot;
        tooltipPanel.anchorMax = parent.pivot;
        tooltipPanel.pivot = new Vector2(0f, 1f);
        tooltipPanel.SetAsLastSibling();
        tooltipPanel.gameObject.SetActive(true);
        tooltipView.SetItem(slot.CurrentItemData,
            new Vector2(Mathf.Max(1f, bounds.width - 16f), Mathf.Max(1f, bounds.height - 16f)),
            slot.IsEquipmentSlot, GetEquippedSetPieceCount);
        MoveTooltip(screenPosition);
    }

    public void MoveTooltip(Vector2 screenPosition)
    {
        if (tooltipPanel == null || !tooltipPanel.gameObject.activeSelf || !TryGetTooltipBounds(out Rect bounds))
        {
            return;
        }

        Vector2 localPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)tooltipPanel.parent,
            screenPosition + TooltipOffset,
            rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera,
            out localPosition);

        Vector2 size = tooltipPanel.rect.size;
        localPosition.x = Mathf.Clamp(localPosition.x, bounds.xMin + 8f, bounds.xMax - size.x - 8f);
        localPosition.y = Mathf.Clamp(localPosition.y, bounds.yMin + size.y + 8f, bounds.yMax - 8f);
        tooltipPanel.anchoredPosition = localPosition;
    }

    private bool TryGetTooltipBounds(out Rect bounds)
    {
        bounds = default;
        if (tooltipPanel == null || !(tooltipPanel.parent is RectTransform parent)
            || !(ResolveRootCanvas()?.transform is RectTransform canvas))
        {
            return false;
        }

        canvas.GetWorldCorners(canvasCorners);
        Vector2 minimum = parent.InverseTransformPoint(canvasCorners[0]);
        Vector2 maximum = minimum;
        for (int i = 1; i < canvasCorners.Length; i++)
        {
            Vector2 point = parent.InverseTransformPoint(canvasCorners[i]);
            minimum = Vector2.Min(minimum, point);
            maximum = Vector2.Max(maximum, point);
        }

        bounds = Rect.MinMaxRect(minimum.x, minimum.y, maximum.x, maximum.y);
        return true;
    }

    public void HideTooltip(InventorySlotUI slot)
    {
        if (slot != null && tooltipOwner != null && tooltipOwner != slot)
        {
            return;
        }

        tooltipOwner = null;
        if (tooltipPanel != null)
        {
            tooltipPanel.gameObject.SetActive(false);
        }
    }

    public void BeginDrag(InventorySlotUI sourceSlot, GameObject itemObject, Vector2 screenPosition)
    {
        if (sourceSlot == null || itemObject == null || dragPreviewRoot == null)
        {
            return;
        }

        HideTooltip(sourceSlot);
        ResolveRootCanvas();

        dragSourceSlot = sourceSlot;
        draggedItemObject = itemObject;
        draggedItemOriginalParent = itemObject.transform.parent;
        draggedItemOriginalSiblingIndex = itemObject.transform.GetSiblingIndex();
        dragCompleted = false;

        itemObject.transform.SetParent(dragPreviewRoot, true);
        MoveDraggedItem(screenPosition);
    }

    public void Drag(Vector2 screenPosition)
    {
        MoveDraggedItem(screenPosition);
    }

    public void EndDrag()
    {
        if (draggedItemObject == null)
        {
            ClearDragState();
            return;
        }

        if (dragCompleted)
        {
            Destroy(draggedItemObject);
        }
        else if (draggedItemOriginalParent != null)
        {
            draggedItemObject.transform.SetParent(draggedItemOriginalParent, false);
            draggedItemObject.transform.SetSiblingIndex(draggedItemOriginalSiblingIndex);

            RectTransform itemTransform = draggedItemObject.transform as RectTransform;
            if (itemTransform != null)
            {
                itemTransform.anchorMin = Vector2.zero;
                itemTransform.anchorMax = Vector2.one;
                itemTransform.anchoredPosition = Vector2.zero;
                itemTransform.sizeDelta = Vector2.zero;
                itemTransform.localScale = Vector3.one;
            }
        }

        ClearDragState();
    }

    public void MoveItem(InventorySlotUI sourceSlot, InventorySlotUI targetSlot)
    {
        if (sourceSlot == null || targetSlot == null || sourceSlot == targetSlot || !sourceSlot.HasItem)
        {
            return;
        }

        ItemData sourceItem = sourceSlot.CurrentItemData;
        ItemData targetItem = targetSlot.CurrentItemData;

        if (!targetSlot.CanAccept(sourceItem) || !sourceSlot.CanAccept(targetItem))
        {
            return;
        }

        targetSlot.SetItem(sourceItem, itemPrefab);
        if (targetItem != null)
        {
            sourceSlot.SetItem(targetItem, itemPrefab);
        }
        else
        {
            sourceSlot.Clear();
        }

        SyncDisplayedItemsFromSlots();
        SaveOwnership();
        dragCompleted = sourceSlot == dragSourceSlot;
    }

    private void MoveDraggedItem(Vector2 screenPosition)
    {
        if (draggedItemObject == null || dragPreviewRoot == null)
        {
            return;
        }

        RectTransform rootTransform = dragPreviewRoot;
        RectTransform itemTransform = draggedItemObject.transform as RectTransform;
        if (itemTransform == null)
        {
            return;
        }

        Vector2 localPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootTransform,
            screenPosition,
            rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay ? rootCanvas.worldCamera : null,
            out localPosition);

        itemTransform.anchorMin = new Vector2(0.5f, 0.5f);
        itemTransform.anchorMax = new Vector2(0.5f, 0.5f);
        itemTransform.pivot = new Vector2(0.5f, 0.5f);
        itemTransform.anchoredPosition = localPosition;
        itemTransform.localScale = Vector3.one;
    }

    private void ClearDragState()
    {
        dragSourceSlot = null;
        draggedItemObject = null;
        draggedItemOriginalParent = null;
        draggedItemOriginalSiblingIndex = 0;
        dragCompleted = false;
    }

    private Canvas ResolveRootCanvas()
    {
        if (rootCanvas != null)
        {
            return rootCanvas;
        }

        rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas != null && !rootCanvas.isRootCanvas)
        {
            rootCanvas = rootCanvas.rootCanvas;
        }

        return rootCanvas;
    }

    private void SyncDisplayedItemsFromSlots()
    {
        List<ItemData> nextItems = new List<ItemData>();
        for (int i = 0; i < displayedItems.Count; i++)
        {
            ItemData item = displayedItems[i];
            if (item != null && !IsItemInCurrentTab(item))
            {
                nextItems.Add(item);
            }
        }

        for (int i = 0; i < slotList.Count && i < SlotCount; i++)
        {
            InventorySlotUI slot = slotList[i];
            if (slot != null && slot.HasItem)
            {
                nextItems.Add(slot.CurrentItemData);
            }
        }

        displayedItems.Clear();
        displayedItems.AddRange(nextItems);
    }

    public void SetOwnedItems(List<ItemData> items)
    {
        hasInitializedOwnedItems = true;
        ownedItems = new List<ItemData>();
        if (items != null)
        {
            foreach (ItemData item in items)
                if (item != null) ownedItems.Add(item.CreateOwnedInstance(item.EnhancementLevel, item.EnhancementAttempted));
        }
        ResetDisplayedItems();
        RenderCurrentTab();
    }

    public ItemData AddItem(ItemData item)
    {
        if (item == null) return null;

        InitializeOwnedItems();
        item = item.CreateOwnedInstance(item.EnhancementLevel, item.EnhancementAttempted);
        ownedItems.Add(item);
        displayedItems.Add(item);
        SaveOwnership();
        RenderCurrentTab();
        return item;
    }

    public List<ItemData> GetSaleItems()
    {
        InitializeOwnedItems();
        return new List<ItemData>(displayedItems);
    }

    public bool TryRemoveSaleItem(ItemData item)
    {
        InitializeOwnedItems();
        if (item == null || !displayedItems.Remove(item)) return false;
        SaveOwnership();
        RenderCurrentTab();
        return true;
    }

    public bool TryEnhanceItem(ItemData item, int successes)
    {
        InitializeOwnedItems();
        if (item == null || !displayedItems.Contains(item) || !item.TryApplyEnhancement(successes)) return false;
        SaveOwnership();
        RenderCurrentTab();
        return true;
    }

    private void SaveOwnership()
    {
        // 장착한 장비도 소유 목록에 포함하되 판매 목록에서는 제외합니다.
        InitializeEquipmentSlots();
        ownedItems.Clear();
        ownedItems.AddRange(displayedItems);
        foreach (InventorySlotUI slot in equipmentSlots)
        {
            if (slot != null && slot.CurrentItemData != null) ownedItems.Add(slot.CurrentItemData);
        }
        var records = new List<PlayerSaveManager.OwnedItemRecord>();
        foreach (ItemData item in ownedItems)
        {
            if (item != null) records.Add(new PlayerSaveManager.OwnedItemRecord
            {
                itemId = item.name,
                enhancementLevel = item.EnhancementLevel,
                enhancementAttempted = item.EnhancementAttempted,
                hasRolledOptions = true,
                additionalStatBonuses = item.CopyAdditionalStatBonuses(),
                additionalProbabilityBonuses = item.CopyAdditionalProbabilityBonuses(),
                optionRulesVersion = 2
            });
        }
        PlayerSaveManager.SaveOwnedItemRecords(records.ToArray());
    }

    private void ResetDisplayedItems()
    {
        displayedItems.Clear();
        for (int i = 0; i < ownedItems.Count; i++)
        {
            if (ownedItems[i] != null)
            {
                displayedItems.Add(ownedItems[i]);
            }
        }
    }
}
