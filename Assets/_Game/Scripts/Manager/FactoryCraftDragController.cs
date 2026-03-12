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

    public void EndDrag()
    {
        if (!isDragging)
            return;

        bool crafted = TryDropOnMachine();

        Debug.Log("[CraftDrag] EndDrag -> crafted = " + crafted);

        isDragging = false;
        currentRecipe = null;
        targetMachine = null;

        FactoryRecipeCursorUI.Instance?.Hide();
    }

    private bool TryDropOnMachine()
    {
        if (currentRecipe == null || targetMachine == null)
            return false;

        Vector3 world = GetPrimaryWorldPosition();
        Collider2D hit = Physics2D.OverlapPoint(world);

        if (hit == null)
        {
            Debug.LogWarning("[CraftDrag] Không hit collider nào khi thả");
            return false;
        }

        Debug.Log("[CraftDrag] Drop hit = " + hit.name);

        FactoryMachine hitMachine = hit.GetComponentInParent<FactoryMachine>();
        if (hitMachine == null)
        {
            Debug.LogWarning("[CraftDrag] Hit không phải FactoryMachine");
            return false;
        }

        if (hitMachine != targetMachine)
        {
            Debug.LogWarning("[CraftDrag] Thả nhầm máy");
            return false;
        }

        bool ok = targetMachine.StartCraft(currentRecipe);
        Debug.Log("[CraftDrag] StartCraft = " + ok);
        return ok;
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
}