using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.Tilemaps;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class FarmPlacementController : MonoBehaviour
{
    public static FarmPlacementController Instance { get; private set; }

    [Header("Refs")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private PanelBuyUI buyUI;

    [Header("Preview Color")]
    [SerializeField] private Color validColor = new Color(0f, 1f, 0f, 0.7f);
    [SerializeField] private Color invalidColor = new Color(1f, 0f, 0f, 0.7f);

    [Header("Spawn Preview")]
    [SerializeField] private bool spawnAtScreenCenter = true;
    [SerializeField] private int nearestValidSearchRadius = 8;

    [Header("Drag Preview")]
    [SerializeField] private bool allowDragPreview = true;
    [SerializeField] private float previewFollowSpeed = 20f;
    [SerializeField] private float dragDeadZone = 0.01f;

    private readonly Dictionary<SpriteRenderer, Color> originalSpriteColors = new();

    private FarmItemData currentItemData;
    private GameObject previewObject;
    private Vector3Int currentPreviewCell;
    private Vector3Int bestPreviewCell;

    private bool isPlacing;
    private bool hasValidCell;

    private PlacedFarmItem editingItem;

    private bool isDraggingPreview;
    private Vector2 lastPointerScreenPos;
    private Vector3 previewTargetWorldPos;

    public bool IsPlacing => isPlacing;
    public bool IsDraggingPreview => isDraggingPreview;
    public GameObject PreviewObject => previewObject;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (mainCamera == null) mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    private void Update()
    {
        if (mainCamera == null || groundTilemap == null) return;

        if (isPlacing)
        {
            HandlePreviewInput();
            SmoothMovePreview();

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                CancelPlacement();
            }
        }
        else
        {
            if (WasPrimaryPressStarted() && !IsPointerOverUI())
            {
                TryPickPlacedItemForMove();
            }
        }
    }

    public void StartPlacingNewItem(FarmItemData itemData)
    {
        if (itemData == null || itemData.prefab == null) return;

        ForceClearCurrentPreview();

        currentItemData = itemData;
        isPlacing = true;
        editingItem = null;
        isDraggingPreview = false;
        hasValidCell = false;

        previewObject = Instantiate(itemData.prefab);
        CacheOriginalColors(previewObject);
        SetPreviewVisual(previewObject, true);

        if (spawnAtScreenCenter)
            SpawnPreviewNearCameraCenter();
        else
            MovePreviewToFirstValidZone();

        if (buyUI != null)
        {
            buyUI.Setup(mainCamera, previewObject.transform, ConfirmPlacement, CancelPlacement);
        }
    }

    public void StartMovingPlacedItem(PlacedFarmItem placedItem)
    {
        if (placedItem == null || placedItem.itemData == null) return;

        ForceClearCurrentPreview();

        currentItemData = placedItem.itemData;
        isPlacing = true;
        editingItem = placedItem;
        isDraggingPreview = false;
        hasValidCell = false;

        if (FarmGridOccupancy.Instance != null)
        {
            FarmGridOccupancy.Instance.FreeCells(placedItem.occupiedCells);
        }

        previewObject = placedItem.gameObject;
        CacheOriginalColors(previewObject);
        SetPreviewVisual(previewObject, true);

        currentPreviewCell = placedItem.originCell;
        bestPreviewCell = currentPreviewCell;
        hasValidCell = true;

        Vector3 pos = GetPlacementWorldPosition(currentPreviewCell, currentItemData.size);
        previewObject.transform.position = pos;
        previewTargetWorldPos = pos;

        RefreshPreviewValidation();

        if (buyUI != null)
        {
            buyUI.Setup(mainCamera, previewObject.transform, ConfirmPlacement, CancelPlacement);
        }
    }

    private void HandlePreviewInput()
    {
        if (!allowDragPreview) return;
        if (previewObject == null || currentItemData == null) return;

        if (!isDraggingPreview && WasPrimaryPressStarted())
        {
            if (IsPointerOverUI())
                return;

            Vector2 screenPos = GetPrimaryScreenPosition();

            // Chỉ kéo item nếu bấm đúng vào preview
            if (!IsScreenPointOnPreview(screenPos))
                return;

            isDraggingPreview = true;
            lastPointerScreenPos = screenPos;
            previewTargetWorldPos = previewObject.transform.position;
            return;
        }

        if (isDraggingPreview && IsPrimaryPressed())
        {
            Vector2 currentScreenPos = GetPrimaryScreenPosition();

            Vector3 prevWorld = GetWorldPositionFromScreen(lastPointerScreenPos);
            Vector3 currWorld = GetWorldPositionFromScreen(currentScreenPos);

            Vector3 worldDelta = currWorld - prevWorld;
            worldDelta.z = 0f;

            if (worldDelta.sqrMagnitude >= dragDeadZone * dragDeadZone)
            {
                previewTargetWorldPos += worldDelta;
                previewTargetWorldPos.z = 0f;
                RefreshPreviewValidation();
            }

            lastPointerScreenPos = currentScreenPos;
            return;
        }

        if (isDraggingPreview && WasPrimaryPressReleased())
        {
            isDraggingPreview = false;
        }
    }

    private void SmoothMovePreview()
    {
        if (previewObject == null) return;

        previewObject.transform.position = Vector3.Lerp(
            previewObject.transform.position,
            previewTargetWorldPos,
            previewFollowSpeed * Time.deltaTime
        );
    }

    private void RefreshPreviewValidation()
    {
        if (previewObject == null || currentItemData == null) return;

        Vector3Int pointerCell = groundTilemap.WorldToCell(previewTargetWorldPos);
        currentPreviewCell = pointerCell;

        bool canPlaceAtPointer = FarmItemZoneSystem.Instance != null &&
                                 FarmItemZoneSystem.Instance.CanPlaceItemOnTilemap(
                                     groundTilemap,
                                     pointerCell,
                                     currentItemData.size,
                                     currentItemData.itemType,
                                     editingItem
                                 );

        if (canPlaceAtPointer)
        {
            bestPreviewCell = pointerCell;
            hasValidCell = true;
            SetPreviewColor(previewObject, validColor);
            return;
        }

        hasValidCell = false;
        SetPreviewColor(previewObject, invalidColor);
    }

    public void ConfirmPlacement()
    {
        if (!isPlacing) return;
        if (currentItemData == null || previewObject == null) return;
        if (FarmItemZoneSystem.Instance == null) return;
        if (!hasValidCell) return;

        currentPreviewCell = bestPreviewCell;

        bool canPlace = FarmItemZoneSystem.Instance.CanPlaceItemOnTilemap(
            groundTilemap,
            currentPreviewCell,
            currentItemData.size,
            currentItemData.itemType,
            editingItem
        );

        if (!canPlace) return;

        Vector3 snappedPos = GetPlacementWorldPosition(currentPreviewCell, currentItemData.size);
        previewObject.transform.position = snappedPos;
        previewTargetWorldPos = snappedPos;

        List<Vector3Int> cells = FarmItemZoneSystem.Instance.GetOccupiedCells(
            currentPreviewCell,
            currentItemData.size
        );

        if (editingItem == null)
        {
            GameObject placedObj = previewObject;

            RestoreOriginalColors(placedObj);
            SetPreviewVisual(placedObj, false);

            var placed = placedObj.GetComponent<PlacedFarmItem>();
            if (placed == null) placed = placedObj.AddComponent<PlacedFarmItem>();

            placed.Init(currentItemData, currentPreviewCell, cells);

            if (FarmGridOccupancy.Instance != null)
            {
                FarmGridOccupancy.Instance.OccupyCells(cells, placed);
            }
        }
        else
        {
            RestoreOriginalColors(editingItem.gameObject);
            SetPreviewVisual(editingItem.gameObject, false);

            editingItem.Init(currentItemData, currentPreviewCell, cells);

            if (FarmGridOccupancy.Instance != null)
            {
                FarmGridOccupancy.Instance.OccupyCells(cells, editingItem);
            }
        }

        previewObject = null;
        currentItemData = null;
        editingItem = null;
        isPlacing = false;
        isDraggingPreview = false;
        hasValidCell = false;

        if (buyUI != null)
            buyUI.Hide();
    }

    public void CancelPlacement()
    {
        if (!isPlacing) return;

        if (editingItem == null)
        {
            if (previewObject != null)
                Destroy(previewObject);
        }
        else
        {
            if (FarmGridOccupancy.Instance != null)
            {
                FarmGridOccupancy.Instance.OccupyCells(editingItem.occupiedCells, editingItem);
            }

            Vector3 oldPos = GetPlacementWorldPosition(editingItem.originCell, editingItem.itemData.size);
            editingItem.transform.position = oldPos;

            RestoreOriginalColors(editingItem.gameObject);
            SetPreviewVisual(editingItem.gameObject, false);
        }

        previewObject = null;
        currentItemData = null;
        editingItem = null;
        isPlacing = false;
        isDraggingPreview = false;
        hasValidCell = false;

        if (buyUI != null)
            buyUI.Hide();
    }

    private void ForceClearCurrentPreview()
    {
        if (!isPlacing) return;

        if (editingItem == null)
        {
            if (previewObject != null)
                Destroy(previewObject);
        }
        else
        {
            if (FarmGridOccupancy.Instance != null)
            {
                FarmGridOccupancy.Instance.OccupyCells(editingItem.occupiedCells, editingItem);
            }

            Vector3 oldPos = GetPlacementWorldPosition(editingItem.originCell, editingItem.itemData.size);
            editingItem.transform.position = oldPos;

            RestoreOriginalColors(editingItem.gameObject);
            SetPreviewVisual(editingItem.gameObject, false);
        }

        previewObject = null;
        currentItemData = null;
        editingItem = null;
        isPlacing = false;
        isDraggingPreview = false;
        hasValidCell = false;

        if (buyUI != null)
            buyUI.Hide();
    }

    private void TryPickPlacedItemForMove()
    {
        Vector3 pressWorld = GetPrimaryWorldPosition();

        Collider2D hit = Physics2D.OverlapPoint(pressWorld);
        if (hit != null)
        {
            PlacedFarmItem placed = hit.GetComponentInParent<PlacedFarmItem>();
            if (placed != null)
            {
                StartMovingPlacedItem(placed);
                return;
            }
        }

        Vector3Int cell = groundTilemap.WorldToCell(pressWorld);

        if (FarmGridOccupancy.Instance != null &&
            FarmGridOccupancy.Instance.TryGetPlacedItemAtCell(cell, out var placedByCell))
        {
            StartMovingPlacedItem(placedByCell);
        }
    }

    private void SpawnPreviewNearCameraCenter()
    {
        if (currentItemData == null || mainCamera == null) return;

        Vector3 centerWorld = mainCamera.ViewportToWorldPoint(
            new Vector3(0.5f, 0.5f, Mathf.Abs(mainCamera.transform.position.z))
        );
        centerWorld.z = 0f;

        Vector3Int centerCell = groundTilemap.WorldToCell(centerWorld);

        Vector3Int bestCell = FindNearestValidCell(
            centerCell,
            currentItemData.size,
            currentItemData.itemType,
            nearestValidSearchRadius,
            null
        );

        currentPreviewCell = bestCell;
        bestPreviewCell = bestCell;
        hasValidCell = true;

        Vector3 pos = GetPlacementWorldPosition(bestCell, currentItemData.size);
        previewObject.transform.position = pos;
        previewTargetWorldPos = pos;

        RefreshPreviewValidation();
    }

    private Vector3Int FindNearestValidCell(
        Vector3Int centerCell,
        Vector2Int size,
        FarmItemType type,
        int radius,
        PlacedFarmItem ignoreItem = null)
    {
        if (FarmItemZoneSystem.Instance != null &&
            FarmItemZoneSystem.Instance.CanPlaceItemOnTilemap(
                groundTilemap, centerCell, size, type, ignoreItem))
        {
            return centerCell;
        }

        for (int r = 1; r <= radius; r++)
        {
            for (int y = -r; y <= r; y++)
            {
                for (int x = -r; x <= r; x++)
                {
                    Vector3Int cell = new Vector3Int(centerCell.x + x, centerCell.y + y, centerCell.z);

                    bool canPlace = FarmItemZoneSystem.Instance != null &&
                                    FarmItemZoneSystem.Instance.CanPlaceItemOnTilemap(
                                        groundTilemap, cell, size, type, ignoreItem);

                    if (canPlace)
                        return cell;
                }
            }
        }

        return centerCell;
    }

    private void MovePreviewToFirstValidZone()
    {
        if (currentItemData == null) return;

        BoundsInt bounds = groundTilemap.cellBounds;

        for (int y = bounds.yMin; y < bounds.yMax; y++)
        {
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);

                bool canPlace = FarmItemZoneSystem.Instance != null &&
                                FarmItemZoneSystem.Instance.CanPlaceItemOnTilemap(
                                    groundTilemap,
                                    cell,
                                    currentItemData.size,
                                    currentItemData.itemType
                                );

                if (canPlace)
                {
                    currentPreviewCell = cell;
                    bestPreviewCell = cell;
                    hasValidCell = true;

                    Vector3 pos = GetPlacementWorldPosition(cell, currentItemData.size);
                    previewObject.transform.position = pos;
                    previewTargetWorldPos = pos;

                    RefreshPreviewValidation();
                    return;
                }
            }
        }

        currentPreviewCell = Vector3Int.zero;
        bestPreviewCell = Vector3Int.zero;
        hasValidCell = false;

        Vector3 zeroPos = GetPlacementWorldPosition(Vector3Int.zero, currentItemData.size);
        previewObject.transform.position = zeroPos;
        previewTargetWorldPos = zeroPos;

        RefreshPreviewValidation();
    }

    public bool IsScreenPointOnPreview(Vector2 screenPos)
    {
        if (previewObject == null || mainCamera == null) return false;

        Vector3 world = GetWorldPositionFromScreen(screenPos);
        Collider2D hit = Physics2D.OverlapPoint(world);

        if (hit == null) return false;

        return hit.transform == previewObject.transform || hit.transform.IsChildOf(previewObject.transform);
    }

    private Vector3 GetPrimaryWorldPosition()
    {
        Vector2 screenPos = GetPrimaryScreenPosition();
        return GetWorldPositionFromScreen(screenPos);
    }

    private Vector2 GetPrimaryScreenPosition()
    {
        if (Touch.activeTouches.Count > 0)
            return Touch.activeTouches[0].screenPosition;

        if (Mouse.current != null)
            return Mouse.current.position.ReadValue();

        return Vector2.zero;
    }

    private bool WasPrimaryPressStarted()
    {
        if (Touch.activeTouches.Count > 0)
            return Touch.activeTouches[0].phase == UnityEngine.InputSystem.TouchPhase.Began;

        if (Mouse.current != null)
            return Mouse.current.leftButton.wasPressedThisFrame;

        return false;
    }

    private bool IsPrimaryPressed()
    {
        if (Touch.activeTouches.Count > 0)
        {
            var phase = Touch.activeTouches[0].phase;
            return phase == UnityEngine.InputSystem.TouchPhase.Began
                || phase == UnityEngine.InputSystem.TouchPhase.Moved
                || phase == UnityEngine.InputSystem.TouchPhase.Stationary;
        }

        if (Mouse.current != null)
            return Mouse.current.leftButton.isPressed;

        return false;
    }

    private bool WasPrimaryPressReleased()
    {
        if (Touch.activeTouches.Count > 0)
        {
            var phase = Touch.activeTouches[0].phase;
            return phase == UnityEngine.InputSystem.TouchPhase.Ended
                || phase == UnityEngine.InputSystem.TouchPhase.Canceled;
        }

        if (Mouse.current != null)
            return Mouse.current.leftButton.wasReleasedThisFrame;

        return false;
    }

    private Vector3 GetWorldPositionFromScreen(Vector2 screenPos2D)
    {
        Vector3 screenPos = new Vector3(
            screenPos2D.x,
            screenPos2D.y,
            Mathf.Abs(mainCamera.transform.position.z)
        );

        Vector3 world = mainCamera.ScreenToWorldPoint(screenPos);
        world.z = 0f;
        return world;
    }

    private Vector3 GetPlacementWorldPosition(Vector3Int originCell, Vector2Int size)
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

    private void SetPreviewVisual(GameObject obj, bool isPreview)
    {
        if (obj == null) return;

        // để collider vẫn bật khi preview,
        // nhờ vậy có thể click lại vào item để kéo tiếp
        var colliders = obj.GetComponentsInChildren<Collider2D>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = true;
        }
    }

    private void SetPreviewColor(GameObject obj, Color color)
    {
        if (obj == null) return;

        var renderers = obj.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].color = color;
        }
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        if (Touch.activeTouches.Count > 0)
            return EventSystem.current.IsPointerOverGameObject(Touch.activeTouches[0].touchId);

        return EventSystem.current.IsPointerOverGameObject();
    }

    private void CacheOriginalColors(GameObject obj)
    {
        originalSpriteColors.Clear();

        if (obj == null) return;

        var renderers = obj.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (!originalSpriteColors.ContainsKey(renderers[i]))
            {
                originalSpriteColors.Add(renderers[i], renderers[i].color);
            }
        }
    }

    private void RestoreOriginalColors(GameObject obj)
    {
        if (obj == null) return;

        var renderers = obj.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (originalSpriteColors.TryGetValue(renderers[i], out var original))
            {
                renderers[i].color = original;
            }
        }
    }
}