using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FarmSaveLoader : MonoBehaviour
{
    [SerializeField] private Tilemap groundTilemap;

    private void Start()
    {
        LoadPlacedItems();
    }

    public void LoadPlacedItems()
    {
        if (FarmSaveManager.Instance == null) return;
        if (FarmItemZoneSystem.Instance == null) return;
        if (groundTilemap == null) return;

        FarmSaveData saveData = FarmSaveManager.Instance.Load();
        if (saveData == null || saveData.placedItems == null) return;

        for (int i = 0; i < saveData.placedItems.Count; i++)
        {
            PlacedItemSaveData saved = saveData.placedItems[i];
            if (saved == null) continue;

            FarmItemData itemData = FarmSaveManager.Instance.GetItemDataById(saved.itemId);
            if (itemData == null || itemData.prefab == null)
            {
                Debug.LogWarning("Không tìm thấy itemData cho id: " + saved.itemId);
                continue;
            }

            Vector3Int originCell = new Vector3Int(saved.originX, saved.originY, saved.originZ);

            GameObject obj = Instantiate(itemData.prefab);

            PlaceableObject placeable = obj.GetComponent<PlaceableObject>() ?? obj.GetComponentInChildren<PlaceableObject>();
            Vector2Int footprint = placeable != null ? placeable.footprintSize : itemData.size;

            Vector3 rootWorldPos = GetRootWorldFromOriginCell(obj, groundTilemap, originCell);
            obj.transform.position = rootWorldPos;

            List<Vector3Int> occupiedCells = FarmItemZoneSystem.Instance.GetOccupiedCells(originCell, footprint);

            PlacedFarmItem placed = obj.GetComponent<PlacedFarmItem>();
            if (placed == null) placed = obj.AddComponent<PlacedFarmItem>();

            placed.Init(itemData, originCell, occupiedCells);

            if (FarmGridOccupancy.Instance != null)
            {
                FarmGridOccupancy.Instance.OccupyCells(occupiedCells, placed);
            }
        }
    }

    private Vector3 GetRootWorldFromOriginCell(GameObject obj, Tilemap tilemap, Vector3Int originCell)
    {
        PlaceableObject placeable = obj.GetComponent<PlaceableObject>() ?? obj.GetComponentInChildren<PlaceableObject>();

        Vector3 footOffset = Vector3.zero;
        if (placeable != null && placeable.footAnchor != null)
            footOffset = placeable.footAnchor.localPosition;

        Vector3 footCellCenter = tilemap.GetCellCenterWorld(originCell);
        return footCellCenter - footOffset;
    }
}