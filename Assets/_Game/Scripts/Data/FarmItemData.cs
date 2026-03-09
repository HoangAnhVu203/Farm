using UnityEngine;

[CreateAssetMenu(menuName = "Farm/Farm Item Data")]
public class FarmItemData : ScriptableObject
{
    public string itemId;
    public string itemName;
    public FarmItemType itemType = FarmItemType.None;
    public int price = 0;

    [Header("Visual")]
    public Sprite icon;

    [Header("Prefab")]
    public GameObject prefab;

    [Header("Grid Size")]
    public Vector2Int size = Vector2Int.one;
}