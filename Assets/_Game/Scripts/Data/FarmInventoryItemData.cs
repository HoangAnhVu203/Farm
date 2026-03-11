using UnityEngine;

public enum FarmInventoryItemType
{
    None = 0,
    Seed = 1,
    Crop = 2,
    Material = 3,
    Product = 4,
    Other = 5
}

[CreateAssetMenu(menuName = "Farm/Inventory Item Data")]
public class FarmInventoryItemData : ScriptableObject
{
    public string id;
    public string itemName;
    public FarmInventoryItemType itemType;
    public Sprite icon;
}