using System.Collections.Generic;
using UnityEngine;

public class FarmGridOccupancy : MonoBehaviour
{
    public static FarmGridOccupancy Instance { get; private set; }

    private readonly Dictionary<Vector3Int, PlacedFarmItem> occupiedMap = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public bool AreCellsOccupied(List<Vector3Int> cells, PlacedFarmItem ignoreItem = null)
    {
        for (int i = 0; i < cells.Count; i++)
        {
            if (occupiedMap.TryGetValue(cells[i], out var placed))
            {
                if (ignoreItem != null && placed == ignoreItem)
                    continue;

                return true;
            }
        }

        return false;
    }

    public void OccupyCells(List<Vector3Int> cells, PlacedFarmItem item)
    {
        for (int i = 0; i < cells.Count; i++)
        {
            occupiedMap[cells[i]] = item;
        }
    }

    public void FreeCells(List<Vector3Int> cells)
    {
        for (int i = 0; i < cells.Count; i++)
        {
            occupiedMap.Remove(cells[i]);
        }
    }

    public bool TryGetPlacedItemAtCell(Vector3Int cell, out PlacedFarmItem item)
    {
        return occupiedMap.TryGetValue(cell, out item);
    }
}