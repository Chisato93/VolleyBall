using UnityEngine;

[CreateAssetMenu(fileName = "EquipmentItemCatalog", menuName = "Volleyball/Equipment Item Catalog")]
public class EquipmentItemCatalog : ScriptableObject
{
    [SerializeField] private ItemData[] items;

    public ItemData FindItem(string id)
    {
        if (items == null) return null;
        foreach (ItemData item in items)
            if (item != null && item.name == id) return item;
        return null;
    }
}
