using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class WorldItem : MonoBehaviour
{
    [SerializeField] private ItemData itemData;
    [SerializeField] private SpriteRenderer spriteRenderer;

    public ItemData ItemData => itemData;

    private void Awake()
    {
        EnsureReferences();
        ApplyItem();
    }

    public void Initialize(ItemData item)
    {
        itemData = item;
        EnsureReferences();
        ApplyItem();
    }

    private void EnsureReferences()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private void ApplyItem()
    {
        if (itemData != null)
        {
            gameObject.name = $"WorldItem_{itemData.ItemName}";
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = itemData != null ? itemData.Icon : null;
            spriteRenderer.enabled = spriteRenderer.sprite != null;
        }
    }
}
