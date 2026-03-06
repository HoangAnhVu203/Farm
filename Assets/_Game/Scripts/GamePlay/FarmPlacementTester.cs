using UnityEngine;
using UnityEngine.Tilemaps;

public class FarmPlacementTester : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Tilemap groundTilemap;

    [Header("Current Item")]
    [SerializeField] private FarmItemData currentItemData;

    private void Update()
    {
        if (currentItemData == null) return;
        if (mainCamera == null || groundTilemap == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            TryPlace();
        }
    }

    void TryPlace()
    {
        Vector3 mouseWorld = GetMouseWorldPosition();
        Vector3Int originCell = groundTilemap.WorldToCell(mouseWorld);

        bool canPlace = FarmItemZoneSystem.Instance.CanPlaceItemOnTilemap(
            groundTilemap,
            originCell,
            currentItemData.size,
            currentItemData.itemType
        );

        Debug.Log($"Try Place {currentItemData.itemName} at cell {originCell} => {canPlace}");

        if (!canPlace) return;

        Vector3 placePos = GetPlacementWorldPosition(originCell, currentItemData.size);

        Instantiate(currentItemData.prefab, placePos, Quaternion.identity);
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(mainCamera.transform.position.z);
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
        worldPos.z = 0f;
        return worldPos;
    }

    Vector3 GetPlacementWorldPosition(Vector3Int originCell, Vector2Int size)
    {
        Vector3Int lastCell = new Vector3Int(
            originCell.x + size.x - 1,
            originCell.y + size.y - 1,
            originCell.z
        );

        Vector3 start = groundTilemap.GetCellCenterWorld(originCell);
        Vector3 end = groundTilemap.GetCellCenterWorld(lastCell);

        return (start + end) * 0.5f;
    }
}