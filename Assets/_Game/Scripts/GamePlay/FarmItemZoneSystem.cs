using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FarmItemZoneSystem : MonoBehaviour
{
    public static FarmItemZoneSystem Instance { get; private set; }

    [System.Serializable]
    public class FarmItemZone
    {
        public FarmItemType type;
        public List<Vector2> points = new();
    }

    [SerializeField] private List<FarmItemZone> zones = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public List<Vector2> GetZonePoints(FarmItemType type)
    {
        var zone = zones.FirstOrDefault(z => z.type == type);
        if (zone == null) return null;
        return zone.points;
    }

    public List<Vector3Int> GetOccupiedCells(Vector3Int originCell, Vector2Int footprintSize)
    {
        List<Vector3Int> cells = new();

        for (int y = 0; y < footprintSize.y; y++)
        {
            for (int x = 0; x < footprintSize.x; x++)
            {
                cells.Add(new Vector3Int(originCell.x + x, originCell.y + y, originCell.z));
            }
        }

        return cells;
    }

    public bool CanPlaceItemOnTilemap(
        Tilemap tilemap,
        Vector3Int originCell,
        Vector2Int footprintSize,
        FarmItemType itemType,
        PlacedFarmItem ignoreItem = null)
    {
        List<Vector3Int> cells = GetOccupiedCells(originCell, footprintSize);

        for (int i = 0; i < cells.Count; i++)
        {
            Vector3Int cell = cells[i];

            // 1. Cell phải có tile
            if (!tilemap.HasTile(cell))
                return false;

            // 2. Cell phải nằm trong zone hợp lệ
            if (!IsCellValidForItemType(tilemap, cell, itemType))
                return false;

            // 3. Cell không bị vật khác chiếm
            if (FarmGridOccupancy.Instance != null &&
                FarmGridOccupancy.Instance.TryGetPlacedItemAtCell(cell, out var other))
            {
                if (other != null && other != ignoreItem)
                    return false;
            }
        }

        return true;
    }

    private bool IsCellValidForItemType(Tilemap tilemap, Vector3Int cell, FarmItemType itemType)
    {
        // Cell center trong world
        Vector3 world = tilemap.GetCellCenterWorld(cell);
        Vector2 p = new Vector2(world.x, world.y);

        // None thì không đặt
        if (itemType == FarmItemType.None)
            return false;

        // Other:
        // Nếu bạn muốn Other đặt được ở mọi nơi có tile -> return true;
        // Nếu muốn Other cũng phải có zone riêng -> giữ như dưới.
        if (itemType == FarmItemType.Other)
        {
            var zoneOther = zones.FirstOrDefault(z => z.type == FarmItemType.Other);
            if (zoneOther == null || zoneOther.points == null || zoneOther.points.Count < 3)
                return true;

            return IsPointInsidePolygon(p, zoneOther.points);
        }

        // Các loại còn lại phải nằm trong polygon zone đúng type
        var zone = zones.FirstOrDefault(z => z.type == itemType);
        if (zone == null || zone.points == null || zone.points.Count < 3)
            return false;

        return IsPointInsidePolygon(p, zone.points);
    }

    private bool IsPointInsidePolygon(Vector2 point, List<Vector2> polygon)
    {
        bool inside = false;
        int count = polygon.Count;

        for (int i = 0, j = count - 1; i < count; j = i++)
        {
            Vector2 pi = polygon[i];
            Vector2 pj = polygon[j];

            bool intersect =
                ((pi.y > point.y) != (pj.y > point.y)) &&
                (point.x < (pj.x - pi.x) * (point.y - pi.y) / ((pj.y - pi.y) + Mathf.Epsilon) + pi.x);

            if (intersect)
                inside = !inside;
        }

        return inside;
    }

    private void OnDrawGizmos()
    {
        if (zones == null) return;

        for (int i = 0; i < zones.Count; i++)
        {
            var zone = zones[i];
            if (zone == null || zone.points == null || zone.points.Count < 2) continue;

            switch (zone.type)
            {
                case FarmItemType.Soil:
                    Gizmos.color = Color.green;
                    break;
                case FarmItemType.Barn:
                    Gizmos.color = Color.yellow;
                    break;
                case FarmItemType.Factory:
                    Gizmos.color = Color.cyan;
                    break;
                case FarmItemType.Other:
                    Gizmos.color = Color.magenta;
                    break;
                default:
                    Gizmos.color = Color.white;
                    break;
            }

            for (int j = 0; j < zone.points.Count; j++)
            {
                Vector2 a = zone.points[j];
                Vector2 b = zone.points[(j + 1) % zone.points.Count];
                Gizmos.DrawLine(a, b);
            }
        }
    }
}