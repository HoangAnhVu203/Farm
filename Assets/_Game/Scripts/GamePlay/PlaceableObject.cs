using UnityEngine;

public class PlaceableObject : MonoBehaviour
{
    [Header("Info")]
    public FarmItemType itemType;

    [Header("Grid Size")]
    public Vector2Int size = new Vector2Int(1, 1);

    [Header("Placement")]
    public Vector3 placementOffset;

    [Header("Rule")]
    public bool canPlaceOnGround = true;
    public bool canPlaceOnSoil = false;
}