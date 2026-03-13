using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class FactoryCraftDragController : MonoBehaviour
{
    public static FactoryCraftDragController Instance { get; private set; }

    [SerializeField] private Camera mainCamera;

    private bool isDragging;
    private FoodRecipeData currentRecipe;
    private FactoryMachine targetMachine;

    public bool IsDragging => isDragging;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void Update()
    {
        if (!isDragging) return;

        // Khi đang kéo, chỉ cần phát hiện lúc người chơi nhả chuột/tay
        if (WasPrimaryReleased())
        {
            Vector2 releaseScreenPos = GetPrimaryScreenPosition();
            FinishDrag(releaseScreenPos);
        }
    }

    public void BeginDrag(FoodRecipeData recipe, FactoryMachine machine)
    {
        if (recipe == null || machine == null)
        {
            Debug.LogWarning("[CraftDrag] BeginDrag fail: recipe hoặc machine null");
            return;
        }

        currentRecipe = recipe;
        targetMachine = machine;
        isDragging = true;

        FactoryRecipeCursorUI.Instance?.Show(recipe);

        Debug.Log($"[CraftDrag] BeginDrag recipe = {recipe.recipeName}, machine = {machine.name}");
    }

    public void CancelDrag()
    {
        if (!isDragging) return;

        Debug.Log("[CraftDrag] CancelDrag");

        isDragging = false;
        currentRecipe = null;
        targetMachine = null;

        FactoryRecipeCursorUI.Instance?.Hide();
    }

    private void FinishDrag(Vector2 releaseScreenPos)
    {
        if (!isDragging) return;

        bool crafted = TryDropOnMachine(releaseScreenPos);
        Debug.Log("[CraftDrag] FinishDrag -> crafted = " + crafted);

        isDragging = false;
        currentRecipe = null;
        targetMachine = null;

        FactoryRecipeCursorUI.Instance?.Hide();

        PanelFoodFactory panel = Object.FindFirstObjectByType<PanelFoodFactory>(FindObjectsInactive.Include);
        if (panel != null)
            panel.CloseUI();
    }

    private bool TryDropOnMachine(Vector2 screenPos)
    {
        if (currentRecipe == null || targetMachine == null)
            return false;

        Vector3 world = ScreenToWorld(screenPos);

        Collider2D[] hits = Physics2D.OverlapPointAll(world);
        if (hits == null || hits.Length == 0)
        {
            Debug.LogWarning("[CraftDrag] Không hit collider nào khi thả");
            return false;
        }

        for (int i = 0; i < hits.Length; i++)
        {
            Debug.Log("[CraftDrag] Drop hit = " + hits[i].name);

            FactoryMachine hitMachine = hits[i].GetComponentInParent<FactoryMachine>();
            if (hitMachine == null) continue;

            if (hitMachine != targetMachine)
            {
                Debug.LogWarning("[CraftDrag] Thả trúng machine khác");
                continue;
            }

            bool ok = targetMachine.StartCraft(currentRecipe);
            Debug.Log("[CraftDrag] StartCraft = " + ok);
            return ok;
        }

        Debug.LogWarning("[CraftDrag] Không thả trúng đúng máy cần craft");
        return false;
    }

    private Vector3 ScreenToWorld(Vector2 screenPos)
    {
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

    private bool WasPrimaryReleased()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
            return true;

        if (Touch.activeTouches.Count > 0)
        {
            var phase = Touch.activeTouches[0].phase;
            return phase == UnityEngine.InputSystem.TouchPhase.Ended ||
                   phase == UnityEngine.InputSystem.TouchPhase.Canceled;
        }

        return false;
    }
}