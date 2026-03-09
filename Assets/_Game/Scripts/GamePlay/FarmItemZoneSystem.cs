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

    public List<Vector3Int> GetOccupiedCells(Vector3Int originCell, Vector2Int size)
    {
        List<Vector3Int> cells = new();

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                cells.Add(new Vector3Int(originCell.x + x, originCell.y + y, originCell.z));
            }
        }

        return cells;
    }

    public bool CanPlaceItemOnTilemap(
        Tilemap tilemap,
        Vector3Int originCell,
        Vector2Int size,
        FarmItemType type,
        PlacedFarmItem ignoreItem = null
    )
    {
        if (tilemap == null) return false;
        if (type == FarmItemType.None) return false;

        List<Vector2> polygon = GetZonePoints(type);
        if (polygon == null || polygon.Count < 3) return false;

        List<Vector3Int> cells = GetOccupiedCells(originCell, size);

        if (FarmGridOccupancy.Instance != null &&
            FarmGridOccupancy.Instance.AreCellsOccupied(cells, ignoreItem))
        {
            return false;
        }

        for (int i = 0; i < cells.Count; i++)
        {
            Vector3Int cell = cells[i];

            if (!tilemap.HasTile(cell))
                return false;

            Vector3 world = tilemap.GetCellCenterWorld(cell);
            Vector2 point = new Vector2(world.x, world.y);

            if (!PolygonService.IsPointInPolygon(point, polygon))
                return false;
        }

        return true;
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
                default:
                    Gizmos.color = Color.magenta;
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