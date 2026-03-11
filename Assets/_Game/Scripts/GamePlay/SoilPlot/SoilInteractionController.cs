using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class SoilInteractionController : MonoBehaviour
{
    public static SoilInteractionController Instance { get; private set; }

    [SerializeField] private Camera mainCamera;

    private SoilPlot selectedSoil;
    public bool IsHandlingSoilClick { get; private set; }

    public SoilPlot GetSelectedSoil() => selectedSoil;

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
        IsHandlingSoilClick = false;

        if (!WasPrimaryPressStarted()) return;
        if (IsPointerOverUI()) return;

        // đang harvest mode thì không mở panel nữa
        if (HarvestController.Instance != null && HarvestController.Instance.IsHarvestMode)
            return;

        if (SowController.Instance != null && SowController.Instance.IsSowMode)
        return;

        Vector3 world = GetPrimaryWorldPosition();
        Collider2D hit = Physics2D.OverlapPoint(world);
        if (hit == null) return;

        SoilPlot soil = hit.GetComponentInParent<SoilPlot>();
        if (soil == null) return;

        selectedSoil = soil;
        FocusCameraToSoil(soil, hit);

        // Đất chín -> mở panel harvest
        if (soil.IsReadyToHarvest)
        {
            IsHandlingSoilClick = true;
            UIManager.Instance.OpenUI<PanelHarvest>();
            return;
        }

        // Đất trống -> mở panel sow
        if (!soil.IsPlanted)
        {
            IsHandlingSoilClick = true;
            UIManager.Instance.OpenUI<PanelSow>();
            return;
        }

        // Đang trồng nhưng chưa chín -> không xử lý ở đây
        // để FarmPlacementController quyết định move nếu muốn
    }

    private Vector3 GetPrimaryWorldPosition()
    {
        Vector2 screenPos = GetPrimaryScreenPosition();
        Vector3 screen = new Vector3(screenPos.x, screenPos.y, Mathf.Abs(mainCamera.transform.position.z));
        Vector3 world = mainCamera.ScreenToWorldPoint(screen);
        world.z = 0f;
        return world;
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

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        if (Touch.activeTouches.Count > 0)
            return EventSystem.current.IsPointerOverGameObject(Touch.activeTouches[0].touchId);

        return EventSystem.current.IsPointerOverGameObject();
    }

    private void FocusCameraToSoil(SoilPlot soil, Collider2D hitCollider)
    {
        if (CameraFocusController.Instance == null || soil == null)
            return;

        // ưu tiên focus theo collider đang click
        if (hitCollider != null)
        {
            CameraFocusController.Instance.FocusToPosition(hitCollider.bounds.center);
            return;
        }

        // fallback: lấy collider của soil
        Collider2D col = soil.GetComponentInChildren<Collider2D>();
        if (col != null)
        {
            CameraFocusController.Instance.FocusToPosition(col.bounds.center);
            return;
        }

        // fallback cuối
        CameraFocusController.Instance.FocusTo(soil.transform);
    }
}