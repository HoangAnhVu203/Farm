using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class HarvestController : MonoBehaviour
{
    public static HarvestController Instance { get; private set; }

    [SerializeField] private Camera mainCamera;

    private bool isHarvestMode;
    private readonly HashSet<SoilPlot> harvestedThisHold = new();

    public bool IsHarvestMode => isHarvestMode;

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

    public void BeginHarvestHold()
    {
        isHarvestMode = true;
        harvestedThisHold.Clear();
        HarvestCursorUI.Instance?.Show();
    }

    public void EndHarvestHold()
    {
        isHarvestMode = false;
        harvestedThisHold.Clear();
        HarvestCursorUI.Instance?.Hide();
    }

    private void Update()
    {
        if (!isHarvestMode) return;

        // Khi đang giữ tool, cứ quét theo con trỏ/touch
        TryHarvestAtPointer();

        // backup: nếu vì lý do nào đó pointer đã thả thì cũng tự tắt
        if (!IsPrimaryPressed())
        {
            EndHarvestHold();
        }
    }

    private void TryHarvestAtPointer()
    {
        Vector3 world = GetPrimaryWorldPosition();
        Collider2D hit = Physics2D.OverlapPoint(world);
        if (hit == null) return;

        SoilPlot soil = hit.GetComponentInParent<SoilPlot>();
        if (soil == null) return;
        if (harvestedThisHold.Contains(soil)) return;

        if (soil.IsReadyToHarvest)
        {
            soil.Harvest();
            harvestedThisHold.Add(soil);
        }
    }

    private Vector3 GetPrimaryWorldPosition()
    {
        Vector2 screenPos = GetPrimaryScreenPosition();
        Vector3 screen = new Vector3(
            screenPos.x,
            screenPos.y,
            Mathf.Abs(mainCamera.transform.position.z)
        );

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
}