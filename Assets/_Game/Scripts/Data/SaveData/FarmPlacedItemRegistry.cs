using System.Collections.Generic;
using UnityEngine;

public class FarmPlacedItemRegistry : MonoBehaviour
{
    public static FarmPlacedItemRegistry Instance { get; private set; }

    private readonly List<PlacedFarmItem> placedItems = new();

    public IReadOnlyList<PlacedFarmItem> PlacedItems => placedItems;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Register(PlacedFarmItem item)
    {
        if (item == null) return;
        if (!placedItems.Contains(item))
            placedItems.Add(item);
    }

    public void Unregister(PlacedFarmItem item)
    {
        if (item == null) return;
        placedItems.Remove(item);
    }

    public List<PlacedFarmItem> GetAll()
    {
        return new List<PlacedFarmItem>(placedItems);
    }
}