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

    private void Start()
    {
        RegisterAsPlacedItem();
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

        // 1. Xác định originCell: ưu tiên vị trí đã lưu, fallback về vị trí scene
        Vector3Int originCell = LoadSavedOriginCell(placeable);

        // 2. Dịch transform về vị trí ứng với cell đó
        ApplyPositionFromCell(originCell, placeable);

        // 3. Tính occupied cells theo footprint
        List<Vector3Int> cells = FarmItemZoneSystem.Instance != null
            ? FarmItemZoneSystem.Instance.GetOccupiedCells(originCell, placeable.footprintSize)
            : BuildCellsFallback(originCell, placeable.footprintSize);

        // 4. Giải phóng cell cũ (nếu đã chiếm trước đó)
        if (placed.occupiedCells != null && placed.occupiedCells.Count > 0)
            FarmGridOccupancy.Instance?.FreeCells(placed.occupiedCells);

        // 5. Init và đăng ký
        placed.Init(itemData, originCell, cells);
        placed.isPreplaced = true;
        placed.uniqueId = uniqueId;

        FarmGridOccupancy.Instance?.OccupyCells(cells, placed);
        FarmPlacedItemRegistry.Instance?.Register(placed);

        Debug.Log($"[PreplacedFactoryMachineBootstrap] Registered {name} | id={uniqueId} | cell={originCell}");
    }

    /// <summary>
    /// Đọc originCell từ save data nếu có, ngược lại tính từ vị trí scene.
    /// </summary>
    private Vector3Int LoadSavedOriginCell(PlaceableObject placeable)
    {
        if (!string.IsNullOrWhiteSpace(uniqueId) && FarmSaveManager.Instance != null)
        {
            FarmSaveData saveData = FarmSaveManager.Instance.Load();
            if (saveData?.preplacedItems != null)
            {
                for (int i = 0; i < saveData.preplacedItems.Count; i++)
                {
                    PreplacedItemSaveData saved = saveData.preplacedItems[i];
                    if (saved != null && saved.uniqueId == uniqueId)
                    {
                        Debug.Log($"[PreplacedFactoryMachineBootstrap] Found saved position for '{uniqueId}': ({saved.originX},{saved.originY},{saved.originZ})");
                        return new Vector3Int(saved.originX, saved.originY, saved.originZ);
                    }
                }
            }
        }

        // Fallback: tính từ vị trí scene hiện tại
        Vector3 footWorld = transform.position;
        if (placeable.footAnchor != null)
            footWorld = placeable.footAnchor.position;

        Vector3Int sceneCell = groundTilemap.WorldToCell(footWorld);
        Debug.Log($"[PreplacedFactoryMachineBootstrap] No save found for '{uniqueId}', using scene cell: {sceneCell}");
        return sceneCell;
    }

    /// <summary>
    /// Dịch chuyển transform về vị trí thế giới tương ứng với originCell trên tilemap.
    /// </summary>
    private void ApplyPositionFromCell(Vector3Int originCell, PlaceableObject placeable)
    {
        Vector3 footCenter = groundTilemap.GetCellCenterWorld(originCell);
        Vector3 rootWorld = footCenter;

        if (placeable.footAnchor != null)
            rootWorld = footCenter - placeable.footAnchor.localPosition;

        rootWorld.z = 0f;
        transform.position = rootWorld;
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