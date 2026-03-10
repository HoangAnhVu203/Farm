using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class CameraDragController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Camera cam;

    [Header("Pan")]
    [SerializeField] private float mousePanMultiplier = 1f;
    [SerializeField] private float touchPanMultiplier = 1f;

    [Header("Zoom")]
    [SerializeField] private float zoomMin = 3f;
    [SerializeField] private float zoomMax = 12f;
    [SerializeField] private float mouseZoomSpeed = 0.015f;
    [SerializeField] private float pinchZoomSpeed = 0.01f;

    private bool isDraggingMouse;
    private bool isDraggingTouch;
    private bool touchOwnedByPreview;

    private Vector3 lastWorldPoint;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
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
        if (cam == null) return;

        HandleMousePan();
        HandleMouseZoom();

        HandleTouchPan();
        HandleTouchZoom();
    }

    private void HandleMousePan()
    {
        if (Mouse.current == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                isDraggingMouse = false;
                return;
            }

            // Nếu đang placing và click trúng preview -> nhường chuột trái cho item
            if (FarmPlacementController.Instance != null &&
                FarmPlacementController.Instance.IsPlacing &&
                FarmPlacementController.Instance.IsScreenPointOnPreview(mousePos))
            {
                isDraggingMouse = false;
                return;
            }

            // Ngược lại: chuột trái kéo camera
            isDraggingMouse = true;
            lastWorldPoint = GetWorldPoint(mousePos);
            return;
        }

        if (isDraggingMouse && Mouse.current.leftButton.isPressed)
        {
            Vector3 currentWorldPoint = GetWorldPoint(mousePos);
            Vector3 delta = (lastWorldPoint - currentWorldPoint) * mousePanMultiplier;

            cam.transform.position += delta;
            lastWorldPoint = GetWorldPoint(mousePos);
            return;
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            isDraggingMouse = false;
        }
    }

    private void HandleTouchPan()
    {
        if (Touch.activeTouches.Count == 0)
        {
            isDraggingTouch = false;
            touchOwnedByPreview = false;
            return;
        }

        if (Touch.activeTouches.Count >= 2)
        {
            isDraggingTouch = false;
            return;
        }

        var touch = Touch.activeTouches[0];

        if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
        {
            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject(touch.touchId))
            {
                isDraggingTouch = false;
                touchOwnedByPreview = false;
                return;
            }

            touchOwnedByPreview = false;

            // Nếu đang placing và touch bắt đầu trên preview -> nhường cho item
            if (FarmPlacementController.Instance != null &&
                FarmPlacementController.Instance.IsPlacing &&
                FarmPlacementController.Instance.IsScreenPointOnPreview(touch.screenPosition))
            {
                isDraggingTouch = false;
                touchOwnedByPreview = true;
                return;
            }

            isDraggingTouch = true;
            lastWorldPoint = GetWorldPoint(touch.screenPosition);
        }
        else if (isDraggingTouch &&
                 !touchOwnedByPreview &&
                 (touch.phase == UnityEngine.InputSystem.TouchPhase.Moved ||
                  touch.phase == UnityEngine.InputSystem.TouchPhase.Stationary))
        {
            Vector3 currentWorldPoint = GetWorldPoint(touch.screenPosition);
            Vector3 delta = (lastWorldPoint - currentWorldPoint) * touchPanMultiplier;

            cam.transform.position += delta;
            lastWorldPoint = GetWorldPoint(touch.screenPosition);
        }
        else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended ||
                 touch.phase == UnityEngine.InputSystem.TouchPhase.Canceled)
        {
            isDraggingTouch = false;
            touchOwnedByPreview = false;
        }
    }

    private void HandleMouseZoom()
    {
        if (Mouse.current == null) return;
        if (!cam.orthographic) return;

        float scroll = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) < 0.01f) return;

        cam.orthographicSize -= scroll * mouseZoomSpeed;
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, zoomMin, zoomMax);
    }

    private void HandleTouchZoom()
    {
        if (!cam.orthographic) return;
        if (Touch.activeTouches.Count < 2) return;

        var t0 = Touch.activeTouches[0];
        var t1 = Touch.activeTouches[1];

        Vector2 prev0 = t0.screenPosition - t0.delta;
        Vector2 prev1 = t1.screenPosition - t1.delta;

        float prevDistance = Vector2.Distance(prev0, prev1);
        float currDistance = Vector2.Distance(t0.screenPosition, t1.screenPosition);

        float delta = currDistance - prevDistance;
        if (Mathf.Abs(delta) < 0.01f) return;

        cam.orthographicSize -= delta * pinchZoomSpeed;
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, zoomMin, zoomMax);
    }

    private Vector3 GetWorldPoint(Vector2 screenPos2D)
    {
        Vector3 screenPos = new Vector3(
            screenPos2D.x,
            screenPos2D.y,
            Mathf.Abs(cam.transform.position.z)
        );

        Vector3 world = cam.ScreenToWorldPoint(screenPos);
        world.z = 0f;
        return world;
    }
}