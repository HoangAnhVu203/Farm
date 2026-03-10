using UnityEngine;

public class PlaceableObject : MonoBehaviour
{
    [Header("Footprint")]
    public Vector2Int footprintSize = Vector2Int.one;

    [Header("Placement Rule")]
    public bool canPlaceOnGrass = true;
    public bool canPlaceOnSoil = false;

    [Header("Refs")]
    public Transform visualRoot;
    public Transform footAnchor;
}