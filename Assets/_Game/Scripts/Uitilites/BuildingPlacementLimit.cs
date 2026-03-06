// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;
// using UnityEngine.Tilemaps;

// public class BuildingPlacementLimit : MonoBehaviour
// {
//     public static BuildingPlacementLimit instance;

//     private void Awake()
//     {
//         instance = this;
//     }

//     public List<BuildingBound> bounds = new();

//     public List<Vector2> GetPoints(FarmItemType type)
//     {
//         var bound = bounds.FirstOrDefault(b => b.type == type);
//         if (bound == null)
//         {
//             bound = new BuildingBound { type = type };
//             bounds.Add(bound);
//         }
//         return bound.points;
//     }

//     public bool CanPlaceObjectAt(Vector2 worldPos, FarmItemType buildingType)
//     {
//         if (buildingType == FarmItemType.None)
//         {
//             Debug.LogError("Chưa có building type cho building ở vị trí: " + worldPos);
//             return false;
//         }

//         return PolygonService.IsPointInPolygon(worldPos, GetPoints(buildingType));
//     }

//     public bool CanPlaceObjectAtCells(Tilemap tilemap, List<Vector3Int> occupiedCells, FarmItemType buildingType)
//     {
//         if (buildingType == FarmItemType.None)
//         {
//             Debug.LogError("Chưa có building type.");
//             return false;
//         }

//         var polygon = GetPoints(buildingType);
//         if (polygon == null || polygon.Count < 3)
//         {
//             Debug.LogWarning($"BuildingType {buildingType} chưa có polygon hợp lệ.");
//             return false;
//         }

//         for (int i = 0; i < occupiedCells.Count; i++)
//         {
//             Vector3 world = tilemap.GetCellCenterWorld(occupiedCells[i]);
//             Vector2 point = new Vector2(world.x, world.y);

//             if (!PolygonService.IsPointInPolygon(point, polygon))
//             {
//                 return false;
//             }
//         }

//         return true;
//     }
// }

// [System.Serializable]
// public class BuildingBound
// {
//     public FarmItemType type;
//     public List<Vector2> points = new();

//     public BuildingBound() { }
// }