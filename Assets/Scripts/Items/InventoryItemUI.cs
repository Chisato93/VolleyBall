using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemUI : MonoBehaviour
{
    [SerializeField] private ItemData itemData;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;

    public ItemData ItemData => itemData;

    private void Awake()
    {
        ApplyItem();
    }

    public void Initialize(ItemData item)
    {
        itemData = item;
        ApplyItem();
    }

    private void ApplyItem()
    {
        if (itemData != null)
        {
            gameObject.name = $"InventoryItem_{itemData.ItemName}";
        }

        if (iconImage != null)
        {
            iconImage.enabled = itemData != null && itemData.Icon != null;
            iconImage.sprite = itemData != null ? itemData.Icon : null;
        }

        if (nameText != null)
        {
            nameText.text = itemData != null ? itemData.ItemName : string.Empty;
        }
    }
}
