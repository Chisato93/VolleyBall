using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [SerializeField] private Transform itemRoot;
    [SerializeField] private bool restrictCategory;
    [SerializeField] private ItemCategory allowedCategory;

    private InventoryPanel ownerPanel;
    private InventoryItemUI currentItem;
    private ItemData currentItemData;

    public ItemData CurrentItemData => currentItemData;
    public bool HasItem => currentItemData != null;

    public void Initialize(InventoryPanel panel)
    {
        ownerPanel = panel;
    }

    public bool CanAccept(ItemData item)
    {
        if (item == null)
        {
            return true;
        }

        return !restrictCategory || item.Category == allowedCategory;
    }

    public void SetAllowedCategory(ItemCategory category)
    {
        restrictCategory = true;
        allowedCategory = category;
    }

    public void SetItem(ItemData item, InventoryItemUI itemPrefab)
    {
        Clear();

        if (item == null)
        {
            return;
        }

        currentItemData = item;
        currentItem = ItemFactory.CreateInventoryItem(item, itemPrefab, ResolveItemRoot());
    }

    public void Clear()
    {
        ownerPanel?.HideTooltip(this);

        Transform root = ResolveItemRoot();
        for (int i = root.childCount - 1; i >= 0; i--)
        {
            Destroy(root.GetChild(i).gameObject);
        }

        currentItem = null;
        currentItemData = null;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (HasItem)
        {
            ResolveOwnerPanel()?.ShowTooltip(this, eventData.position);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResolveOwnerPanel()?.HideTooltip(this);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (HasItem)
        {
            ResolveOwnerPanel()?.MoveTooltip(eventData.position);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!HasItem || currentItem == null)
        {
            return;
        }

        InventoryPanel panel = ResolveOwnerPanel();
        panel?.HideTooltip(this);
        panel?.BeginDrag(this, currentItem.gameObject, eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        ResolveOwnerPanel()?.Drag(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        ResolveOwnerPanel()?.EndDrag();
    }

    public void OnDrop(PointerEventData eventData)
    {
        InventorySlotUI sourceSlot = eventData.pointerDrag != null
            ? eventData.pointerDrag.GetComponent<InventorySlotUI>()
            : null;

        ResolveOwnerPanel()?.MoveItem(sourceSlot, this);
    }

    private Transform ResolveItemRoot()
    {
        if (itemRoot != null)
        {
            return itemRoot;
        }

        Transform foundRoot = transform.Find("ItemRoot");
        itemRoot = foundRoot != null ? foundRoot : transform;
        return itemRoot;
    }

    private InventoryPanel ResolveOwnerPanel()
    {
        if (ownerPanel != null)
        {
            return ownerPanel;
        }

        ownerPanel = InventoryPanel.ActivePanel;
        if (ownerPanel == null)
        {
            ownerPanel = FindObjectOfType<InventoryPanel>();
        }

        return ownerPanel;
    }
}
