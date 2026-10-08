using System.Collections.Generic;
using UnityEngine;

public class InventoryPanel : TabContentPanel
{
    private const int SlotColumnCount = 5;
    private const int SlotRowCount = 4;
    private const int SlotCount = SlotColumnCount * SlotRowCount;
    private static readonly Vector2 TooltipOffset = new Vector2(16f, -16f);

    [SerializeField] private Transform slotsParent;
    [SerializeField] private InventoryItemUI itemPrefab;
    [SerializeField] private RectTransform tooltipPanel;
    [SerializeField] private RectTransform dragPreviewRoot;
    [SerializeField] private List<ItemData> ownedItems = new List<ItemData>();

    private readonly List<InventorySlotUI> slotList = new List<InventorySlotUI>();
    private readonly List<ItemData> displayedItems = new List<ItemData>();
    private Canvas rootCanvas;
    private InventorySlotUI tooltipOwner;
    private InventorySlotUI dragSourceSlot;
    private GameObject draggedItemObject;
    private Transform draggedItemOriginalParent;
    private int draggedItemOriginalSiblingIndex;
    private bool dragCompleted;
    private int currentTabIndex;

    public static InventoryPanel ActivePanel { get; private set; }

    private void Awake()
    {
        ActivePanel = this;
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
        ResetDisplayedItems();
        RenderCurrentTab();
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

        displayedItems.Add(item);
        RenderCurrentTab();
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
        if (slot == null || !slot.HasItem)
        {
            return;
        }

        if (tooltipPanel == null)
        {
            return;
        }

        tooltipOwner = slot;
        ResolveRootCanvas();
        tooltipPanel.gameObject.SetActive(true);
        MoveTooltip(screenPosition);
    }

    public void MoveTooltip(Vector2 screenPosition)
    {
        if (tooltipPanel == null || rootCanvas == null)
        {
            return;
        }

        RectTransform canvasTransform = rootCanvas.transform as RectTransform;
        if (canvasTransform == null)
        {
            tooltipPanel.position = screenPosition + TooltipOffset;
            return;
        }

        Vector2 localPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasTransform,
            screenPosition + TooltipOffset,
            rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera,
            out localPosition);

        tooltipPanel.anchoredPosition = localPosition;
    }

    public void HideTooltip(InventorySlotUI slot)
    {
        if (tooltipOwner != null && tooltipOwner != slot)
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
        ownedItems = items ?? new List<ItemData>();
        ResetDisplayedItems();
        RenderCurrentTab();
    }

    public void AddItem(ItemData item)
    {
        if (item == null)
        {
            return;
        }

        ownedItems.Add(item);
        displayedItems.Add(item);
        RenderCurrentTab();
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
