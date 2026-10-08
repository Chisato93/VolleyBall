using UnityEngine;

public static class ItemFactory
{
    public static WorldItem Create(
        ItemData itemData,
        WorldItem prefab,
        Vector3 position,
        Quaternion rotation,
        Transform parent = null)
    {
        if (itemData == null)
        {
            Debug.LogWarning("ItemFactory.Create failed because itemData is null.");
            return null;
        }

        if (prefab == null)
        {
            Debug.LogWarning("ItemFactory.Create failed because prefab is null.");
            return null;
        }

        WorldItem worldItem = Object.Instantiate(prefab, position, rotation, parent);
        worldItem.Initialize(itemData);
        return worldItem;
    }

    public static InventoryItemUI CreateInventoryItem(
        ItemData itemData,
        InventoryItemUI prefab,
        Transform parent)
    {
        if (itemData == null)
        {
            Debug.LogWarning("ItemFactory.CreateInventoryItem failed because itemData is null.");
            return null;
        }

        if (prefab == null)
        {
            Debug.LogWarning("ItemFactory.CreateInventoryItem failed because prefab is null.");
            return null;
        }

        InventoryItemUI inventoryItem = Object.Instantiate(prefab, parent);
        inventoryItem.Initialize(itemData);
        return inventoryItem;
    }
}
