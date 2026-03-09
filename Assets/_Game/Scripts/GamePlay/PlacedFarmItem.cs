using System.Collections.Generic;
using UnityEngine;

public class PlacedFarmItem : MonoBehaviour
{
    public FarmItemData itemData;
    public Vector3Int originCell;
    public List<Vector3Int> occupiedCells = new();

    public void Init(FarmItemData data, Vector3Int cell, List<Vector3Int> cells)
    {
        itemData = data;
        originCell = cell;
        occupiedCells = cells;
    }
}