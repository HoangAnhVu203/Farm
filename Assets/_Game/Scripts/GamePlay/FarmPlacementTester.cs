// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Tilemaps;
// using UnityEngine.InputSystem;
// using UnityEngine.InputSystem.EnhancedTouch;
// using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

// public class FarmPlacementTester : MonoBehaviour
// {
//     [Header("Refs")]
//     [SerializeField] private Camera mainCamera;
//     [SerializeField] private Tilemap groundTilemap;

//     [Header("Current Item")]
//     [SerializeField] private FarmItemData currentItemData;

//     private void OnEnable()
//     {
//         EnhancedTouchSupport.Enable();
//     }

//     private void OnDisable()
//     {
//         EnhancedTouchSupport.Disable();
//     }

//     private void Update()
//     {
//         if (currentItemData == null) return;
//         if (mainCamera == null) return;
//         if (groundTilemap == null) return;

//         // Ưu tiên touch trước (Simulator / Mobile)
//         if (Touch.activeTouches.Count > 0)
//         {
//             var touch = Touch.activeTouches[0];

//             if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
//             {
//                 TryPlace(touch.screenPosition);
//             }

//             return;
//         }

//         // Fallback sang mouse (Game view trên editor)
//         if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
//         {
//             TryPlace(Mouse.current.position.ReadValue());
//         }
//     }

//     void TryPlace(Vector2 screenPosition)
//     {
//         Vector3 mouseWorld = GetWorldPositionFromScreen(screenPosition);
//         Vector3Int originCell = groundTilemap.WorldToCell(mouseWorld);

//         bool canPlace = FarmItemZoneSystem.Instance.CanPlaceItemOnTilemap(
//             groundTilemap,
//             originCell,
//             currentItemData.size,
//             currentItemData.itemType
//         );

//         Debug.Log($"Try Place {currentItemData.itemName} at cell {originCell} => {canPlace}");

//         if (!canPlace) return;
//         if (currentItemData.prefab == null) return;

//         Vector3 placePos = GetPlacementWorldPosition(originCell, currentItemData.size);
//         Instantiate(currentItemData.prefab, placePos, Quaternion.identity);

//         List<Vector3Int> cells =
//             FarmItemZoneSystem.Instance.GetOccupiedCells(originCell, currentItemData.size);

//         if (FarmGridOccupancy.Instance != null)
//         {
//             FarmGridOccupancy.Instance.OccupyCells(cells);
//         }
//     }

//     Vector3 GetWorldPositionFromScreen(Vector2 screenPosition)
//     {
//         Vector3 screenPos = new Vector3(
//             screenPosition.x,
//             screenPosition.y,
//             Mathf.Abs(mainCamera.transform.position.z)
//         );

//         Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
//         worldPos.z = 0f;
//         return worldPos;
//     }

//     Vector3 GetPlacementWorldPosition(Vector3Int originCell, Vector2Int size)
//     {
//         Vector3Int lastCell = new Vector3Int(
//             originCell.x + size.x - 1,
//             originCell.y + size.y - 1,
//             originCell.z
//         );

//         Vector3 start = groundTilemap.GetCellCenterWorld(originCell);
//         Vector3 end = groundTilemap.GetCellCenterWorld(lastCell);

//         return (start + end) * 0.5f;
//     }
// }