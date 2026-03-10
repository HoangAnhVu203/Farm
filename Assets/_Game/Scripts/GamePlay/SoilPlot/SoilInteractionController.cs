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

        Vector3 world = GetPrimaryWorldPosition();
        Collider2D hit = Physics2D.OverlapPoint(world);
        if (hit == null) return;

        SoilPlot soil = hit.GetComponentInParent<SoilPlot>();
        if (soil == null) return;

        selectedSoil = soil;

        // Soil trống: mở PanelSow và chặn move
        if (!soil.IsPlanted)
        {
            IsHandlingSoilClick = true;
            UIManager.Instance.OpenUI<PanelSow>();
            return;
        }

        // Soil đã trồng:
        // không set IsHandlingSoilClick
        // để FarmPlacementController tiếp tục xử lý move
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
}