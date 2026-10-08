using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Volleyball/Item Data")]
public class ItemData : ScriptableObject
{
    [SerializeField] private string itemName;
    [SerializeField] private ItemCategory category;
    [SerializeField] private Sprite icon;

    public string ItemName => itemName;
    public ItemCategory Category => category;
    public Sprite Icon => icon;
}
