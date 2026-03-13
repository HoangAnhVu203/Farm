using System.Collections.Generic;
using UnityEngine;

public class PlacedFarmItem : MonoBehaviour
{
    public FarmItemData itemData { get; private set; }
    public Vector3Int originCell { get; private set; }
    public List<Vector3Int> occupiedCells { get; private set; } = new();

    public bool isPreplaced = false;
    public string uniqueId;

    public void Init(FarmItemData data, Vector3Int origin, List<Vector3Int> cells)
    {
        itemData = data;
        originCell = origin;
        occupiedCells = cells ?? new List<Vector3Int>();

        if (FarmPlacedItemRegistry.Instance != null)
            FarmPlacedItemRegistry.Instance.Register(this);
    }

    private void OnDestroy()
    {
        if (FarmPlacedItemRegistry.Instance != null)
            FarmPlacedItemRegistry.Instance.Unregister(this);
    }
}