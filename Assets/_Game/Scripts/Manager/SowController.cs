using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class SowController : MonoBehaviour
{
    public static SowController Instance { get; private set; }

    [SerializeField] private Camera mainCamera;

    private CropSeedData selectedSeed;
    private bool isSowing;
    private bool isDragging;

    private readonly HashSet<SoilPlot> plantedThisDrag = new();

    public bool IsSowing => isSowing;
    public CropSeedData SelectedSeed => selectedSeed;

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

    public void BeginSow(CropSeedData seed)
    {
        selectedSeed = seed;
        isSowing = seed != null;
        isDragging = false;
        plantedThisDrag.Clear();
    }

    public void CancelSow()
    {
        selectedSeed = null;
        isSowing = false;
        isDragging = false;
        plantedThisDrag.Clear();
    }

    private void Update()
    {
        if (!isSowing || selectedSeed == null) return;

        if (WasPrimaryPressStarted())
        {
            if (IsPointerOverUI()) return;

            isDragging = true;
            plantedThisDrag.Clear();
            TryPlantAtPointer();
        }
        else if (isDragging && IsPrimaryPressed())
        {
            TryPlantAtPointer();
        }
        else if (isDragging && WasPrimaryPressReleased())
        {
            isDragging = false;
            plantedThisDrag.Clear();
        }

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CancelSow();
        }
    }

    private void TryPlantAtPointer()
    {
        Vector3 world = GetPrimaryWorldPosition();
        Collider2D hit = Physics2D.OverlapPoint(world);
        if (hit == null) return;

        SoilPlot soil = hit.GetComponentInParent<SoilPlot>();
        if (soil == null) return;

        if (plantedThisDrag.Contains(soil)) return;

        if (soil.CanPlant())
        {
            soil.Plant(selectedSeed);
            plantedThisDrag.Add(soil);
        }
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

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        if (Touch.activeTouches.Count > 0)
            return EventSystem.current.IsPointerOverGameObject(Touch.activeTouches[0].touchId);

        return EventSystem.current.IsPointerOverGameObject();
    }
}