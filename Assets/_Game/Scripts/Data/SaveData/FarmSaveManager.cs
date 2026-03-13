using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FarmSaveManager : MonoBehaviour
{
    public static FarmSaveManager Instance { get; private set; }

    private const string SAVE_KEY = "FARM_SAVE_DATA";

    [Header("Item Database")]
    [SerializeField] private List<FarmItemData> allItems = new();

    private Dictionary<string, FarmItemData> itemLookup;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        BuildLookup();
    }

    private void BuildLookup()
    {
        itemLookup = new Dictionary<string, FarmItemData>();

        for (int i = 0; i < allItems.Count; i++)
        {
            FarmItemData item = allItems[i];
            if (item == null || string.IsNullOrWhiteSpace(item.itemId)) continue;

            if (!itemLookup.ContainsKey(item.itemId))
                itemLookup.Add(item.itemId, item);
            else
                Debug.LogWarning($"Trùng FarmItemData id: {item.itemId}");
        }
    }

    public FarmItemData GetItemDataById(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        if (itemLookup == null) BuildLookup();

        itemLookup.TryGetValue(id, out var data);
        return data;
    }

    public void SavePlacedItems(List<PlacedFarmItem> placedItems)
    {
        FarmSaveData saveData = new FarmSaveData();

        for (int i = 0; i < placedItems.Count; i++)
        {
            PlacedFarmItem placed = placedItems[i];
            if (placed == null || placed.itemData == null) continue;

            bool isActuallyPreplaced =
                placed.isPreplaced || !string.IsNullOrWhiteSpace(placed.uniqueId);

            Debug.Log($"[SAVE CHECK] {placed.name} | isPreplaced={placed.isPreplaced} | uniqueId={placed.uniqueId} | actualPreplaced={isActuallyPreplaced}");

            if (isActuallyPreplaced)
            {
                if (string.IsNullOrWhiteSpace(placed.uniqueId))
                {
                    Debug.LogWarning($"[SAVE PREPLACED] {placed.name} bị thiếu uniqueId");
                    continue;
                }

                saveData.preplacedItems.Add(new PreplacedItemSaveData
                {
                    uniqueId = placed.uniqueId,
                    originX = placed.originCell.x,
                    originY = placed.originCell.y,
                    originZ = placed.originCell.z
                });

                Debug.Log($"[SAVE PREPLACED] {placed.name} -> id={placed.uniqueId} cell=({placed.originCell.x},{placed.originCell.y},{placed.originCell.z})");
                continue;
            }

            if (string.IsNullOrWhiteSpace(placed.itemData.itemId)) continue;

            saveData.placedItems.Add(new PlacedItemSaveData
            {
                itemId = placed.itemData.itemId,
                originX = placed.originCell.x,
                originY = placed.originCell.y,
                originZ = placed.originCell.z
            });

            Debug.Log($"[SAVE DYNAMIC] {placed.name} -> itemId={placed.itemData.itemId} cell=({placed.originCell.x},{placed.originCell.y},{placed.originCell.z})");
        }

        string json = JsonUtility.ToJson(saveData);
        Debug.Log("[SAVE JSON] " + json);

        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();

        Debug.Log("Saved placed items: " + json);
    }
    public FarmSaveData Load()
    {
        if (!PlayerPrefs.HasKey(SAVE_KEY))
            return new FarmSaveData();

        string json = PlayerPrefs.GetString(SAVE_KEY, "");
        if (string.IsNullOrEmpty(json))
            return new FarmSaveData();

        FarmSaveData data = JsonUtility.FromJson<FarmSaveData>(json);
        return data ?? new FarmSaveData();
    }

    public void ClearSave()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.Save();
    }

    // =========================
    // THÊM MỚI: Restore preplaced items
    // =========================
    public void RestorePreplacedItems(Tilemap groundTilemap)
    {
        Debug.Log("[RESTORE] RestorePreplacedItems called");
        if (groundTilemap == null)
        {
            Debug.LogWarning("RestorePreplacedItems: groundTilemap is null");
            return;
        }

        FarmSaveData saveData = Load();
        if (saveData == null || saveData.preplacedItems == null || saveData.preplacedItems.Count == 0)
            return;

        if (PreplacedFarmItemRegistry.Instance == null)
        {
            Debug.LogWarning("RestorePreplacedItems: PreplacedFarmItemRegistry.Instance is null");
            return;
        }

        for (int i = 0; i < saveData.preplacedItems.Count; i++)
        {
            
            PreplacedItemSaveData state = saveData.preplacedItems[i];
            if (state == null || string.IsNullOrWhiteSpace(state.uniqueId)) continue;

            if (!PreplacedFarmItemRegistry.Instance.TryGet(state.uniqueId, out var placed))
            {
                Debug.LogWarning($"Không tìm thấy preplaced item có id = {state.uniqueId}");
                continue;
            }

            PlaceableObject placeable = placed.GetComponent<PlaceableObject>();
            if (placeable == null)
            {
                Debug.LogWarning($"{placed.name} thiếu PlaceableObject");
                continue;
            }

            Vector3Int originCell = new Vector3Int(state.originX, state.originY, state.originZ);

            // free cell cũ trước
            if (FarmGridOccupancy.Instance != null &&
                placed.occupiedCells != null &&
                placed.occupiedCells.Count > 0)
            {
                FarmGridOccupancy.Instance.FreeCells(placed.occupiedCells);
            }

            List<Vector3Int> cells = FarmItemZoneSystem.Instance != null
                ? FarmItemZoneSystem.Instance.GetOccupiedCells(originCell, placeable.footprintSize)
                : BuildCellsFallback(originCell, placeable.footprintSize);

            Vector3 footCenter = groundTilemap.GetCellCenterWorld(originCell);
            Vector3 rootWorld = footCenter;

            if (placeable.footAnchor != null)
            {
                Vector3 footLocalOffset = placeable.footAnchor.localPosition;
                rootWorld = footCenter - footLocalOffset;
            }

            placed.transform.position = rootWorld;
            placed.Init(placed.itemData, originCell, cells);

            FarmGridOccupancy.Instance?.OccupyCells(cells, placed);

            Debug.Log($"Restored preplaced item: {state.uniqueId} -> {originCell}");
            Debug.Log($"[RESTORE] {state.uniqueId} -> ({state.originX},{state.originY},{state.originZ})");
        }
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