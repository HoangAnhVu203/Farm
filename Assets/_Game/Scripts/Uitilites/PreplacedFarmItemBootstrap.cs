using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(FactoryMachine))]
[RequireComponent(typeof(PlaceableObject))]
public class PreplacedFactoryMachineBootstrap : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private FarmItemData itemData;
    [SerializeField] private Tilemap groundTilemap;

    [Header("Preplaced ID")]
    [SerializeField] private string uniqueId = "factory_machine_01";

    [Header("Optional")]
    [SerializeField] private bool registerOnStart = true;


    private void Awake()
    {
        RegisterAsPlacedItem();
    }
    private void Start()
    {
        Debug.Log($"[BOOTSTRAP] {name} start at {transform.position}");
        if (!registerOnStart) return;
        // RegisterAsPlacedItem();
    }

    [ContextMenu("Register As Placed Item")]
    public void RegisterAsPlacedItem()
    {
        if (itemData == null)
        {
            Debug.LogError($"[PreplacedFactoryMachineBootstrap] {name} thiếu itemData");
            return;
        }

        if (groundTilemap == null)
        {
            Debug.LogError($"[PreplacedFactoryMachineBootstrap] {name} thiếu groundTilemap");
            return;
        }

        PlaceableObject placeable = GetComponent<PlaceableObject>();
        if (placeable == null)
        {
            Debug.LogError($"[PreplacedFactoryMachineBootstrap] {name} thiếu PlaceableObject");
            return;
        }

        PlacedFarmItem placed = GetComponent<PlacedFarmItem>();
        if (placed == null)
            placed = gameObject.AddComponent<PlacedFarmItem>();

        Vector3 footWorld = transform.position;
        if (placeable.footAnchor != null)
            footWorld = placeable.footAnchor.position;

        Vector3Int originCell = groundTilemap.WorldToCell(footWorld);

        List<Vector3Int> cells = FarmItemZoneSystem.Instance != null
            ? FarmItemZoneSystem.Instance.GetOccupiedCells(originCell, placeable.footprintSize)
            : BuildCellsFallback(originCell, placeable.footprintSize);

        placed.Init(itemData, originCell, cells);
        placed.isPreplaced = true;
        placed.uniqueId = uniqueId;

        FarmGridOccupancy.Instance?.OccupyCells(cells, placed);
        FarmPlacedItemRegistry.Instance?.Register(placed);

        Debug.Log($"[PreplacedFactoryMachineBootstrap] Registered {name} | id = {uniqueId} | cell = {originCell}");
        Debug.Log($"[PREPLACED CHECK] {name} | isPreplaced = {placed.isPreplaced} | uniqueId = {placed.uniqueId}");
    }

    private List<Vector3Int> BuildCellsFallback(Vector3Int originCell, Vector2Int size)
    {
        List<Vector3Int> cells = new();
        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                cells.Add(new Vector3Int(originCell.x + x, originCell.y + y, originCell.z));
            }
        }
        return cells;
    }
}