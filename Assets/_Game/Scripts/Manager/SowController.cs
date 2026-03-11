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

    [Header("Drag Sampling")]
    [SerializeField] private float sampleSpacing = 0.12f; 
    [SerializeField] private float overlapRadius = 0.05f; 

    private bool isSowMode;
    private CropSeedData currentSeed;

    private readonly HashSet<SoilPlot> plantedThisHold = new();

    private bool hasLastPointerWorld;
    private Vector3 lastPointerWorld;

    public bool IsSowMode => isSowMode;
    public CropSeedData CurrentSeed => currentSeed;

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

    public void BeginSowHold(CropSeedData seed)
    {
        if (seed == null) return;

        currentSeed = seed;
        isSowMode = true;
        plantedThisHold.Clear();

        hasLastPointerWorld = false;

        SeedCursorUI.Instance?.Show(seed);
    }

    public void EndSowHold()
    {
        isSowMode = false;
        currentSeed = null;
        plantedThisHold.Clear();

        hasLastPointerWorld = false;

        SeedCursorUI.Instance?.Hide();
    }

    private void Update()
    {
        if (!isSowMode || currentSeed == null) return;

        if (!IsPrimaryPressed())
        {
            EndSowHold();
            return;
        }

        // Khi đang giữ hạt, không chặn bởi UI nữa vì panel đã ẩn
        Vector3 currentWorld = GetPrimaryWorldPosition();

        if (!hasLastPointerWorld)
        {
            hasLastPointerWorld = true;
            lastPointerWorld = currentWorld;
            TryPlantAtWorld(currentWorld);
            return;
        }

        TryPlantAlongLine(lastPointerWorld, currentWorld);
        lastPointerWorld = currentWorld;
    }

    private void TryPlantAlongLine(Vector3 fromWorld, Vector3 toWorld)
    {
        float distance = Vector3.Distance(fromWorld, toWorld);

        if (distance <= sampleSpacing)
        {
            TryPlantAtWorld(toWorld);
            return;
        }

        int steps = Mathf.CeilToInt(distance / sampleSpacing);

        for (int i = 0; i <= steps; i++)
        {
            float t = steps == 0 ? 1f : (float)i / steps;
            Vector3 sample = Vector3.Lerp(fromWorld, toWorld, t);
            TryPlantAtWorld(sample);
        }
    }

    private void TryPlantAtWorld(Vector3 world)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(world, overlapRadius);
        if (hits == null || hits.Length == 0) return;

        for (int i = 0; i < hits.Length; i++)
        {
            SoilPlot soil = hits[i].GetComponentInParent<SoilPlot>();
            if (soil == null) continue;
            if (plantedThisHold.Contains(soil)) continue;

            if (soil.CanPlant())
            {
                soil.Plant(currentSeed);
                plantedThisHold.Add(soil);
            }
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

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!hasLastPointerWorld) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(lastPointerWorld, overlapRadius);
    }
#endif
}