using System.Collections.Generic;
using UnityEngine;

public class PreplacedFarmItemRegistry : MonoBehaviour
{
    public static PreplacedFarmItemRegistry Instance { get; private set; }

    private readonly Dictionary<string, PlacedFarmItem> map = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Rebuild();
    }

    [ContextMenu("Rebuild")]
    public void Rebuild()
    {
        map.Clear();

        PlacedFarmItem[] all = Object.FindObjectsByType<PlacedFarmItem>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        for (int i = 0; i < all.Length; i++)
        {
            PlacedFarmItem placed = all[i];
            if (placed == null) continue;
            if (!placed.isPreplaced) continue;
            if (string.IsNullOrWhiteSpace(placed.uniqueId)) continue;

            if (!map.ContainsKey(placed.uniqueId))
                map.Add(placed.uniqueId, placed);
            else
                Debug.LogWarning($"Trùng preplaced uniqueId: {placed.uniqueId}");
        }
    }

    public bool TryGet(string uniqueId, out PlacedFarmItem placed)
    {
        return map.TryGetValue(uniqueId, out placed);
    }
}