using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class CameraDragController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Camera cam;

    [Header("Zoom")]
    [SerializeField] private float zoomMin = 3f;
    [SerializeField] private float zoomMax = 12f;
    [SerializeField] private float zoomSpeed = 0.15f;

    private Vector3 lastWorldPoint;
    private bool isDraggingMouse;
    private bool isDraggingTouch;

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

        if (FarmPlacementController.Instance != null &&
            FarmPlacementController.Instance.IsPlacing)
        {
            isDraggingMouse = false;
            isDraggingTouch = false;
            HandleMouseZoom();
            return;
        }

        HandleTouchDrag();
        HandleMouseDrag();
        HandleMouseZoom();
    }

    private void HandleTouchDrag()
    {
        if (Touch.activeTouches.Count == 0)
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
                return;
            }

            isDraggingTouch = true;
            lastWorldPoint = GetWorldPoint(touch.screenPosition);
        }
        else if (isDraggingTouch &&
                 (touch.phase == UnityEngine.InputSystem.TouchPhase.Moved ||
                  touch.phase == UnityEngine.InputSystem.TouchPhase.Stationary))
        {
            Vector3 currentWorldPoint = GetWorldPoint(touch.screenPosition);
            Vector3 delta = lastWorldPoint - currentWorldPoint;
            cam.transform.position += delta;
            lastWorldPoint = GetWorldPoint(touch.screenPosition);
        }
        else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended ||
                 touch.phase == UnityEngine.InputSystem.TouchPhase.Canceled)
        {
            isDraggingTouch = false;
        }
    }

    private void HandleMouseDrag()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            isDraggingMouse = true;
            lastWorldPoint = GetWorldPoint(Mouse.current.position.ReadValue());
        }
        else if (isDraggingMouse && Mouse.current.rightButton.isPressed)
        {
            Vector3 currentWorldPoint = GetWorldPoint(Mouse.current.position.ReadValue());
            Vector3 delta = lastWorldPoint - currentWorldPoint;
            cam.transform.position += delta;
            lastWorldPoint = GetWorldPoint(Mouse.current.position.ReadValue());
        }
        else if (Mouse.current.rightButton.wasReleasedThisFrame)
        {
            isDraggingMouse = false;
        }
    }

    private void HandleMouseZoom()
    {
        if (Mouse.current == null) return;

        float scroll = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) < 0.01f) return;

        if (cam.orthographic)
        {
            cam.orthographicSize -= scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, zoomMin, zoomMax);
        }
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