using UnityEngine;
using UnityEngine.EventSystems;

public class FactoryRecipeHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private PanelFoodFactory ownerPanel;
    [SerializeField] private FoodRecipeData recipeData;

    private bool isHolding;

    public void Setup(PanelFoodFactory panel, FoodRecipeData recipe)
    {
        ownerPanel = panel;
        recipeData = recipe;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (ownerPanel == null || recipeData == null) return;

        isHolding = true;
        ownerPanel.ShowIngredientTable(recipeData);

        if (ownerPanel.CanCraft(recipeData))
        {
            FactoryCraftDragController.Instance?.BeginDrag(recipeData, ownerPanel.GetCurrentMachine());
            ownerPanel.HidePanelOnly();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StopHold();
    }

    private void Update()
    {
        if (!isHolding) return;

        if (!IsPrimaryPressed())
        {
            StopHold();
        }
    }

    private void StopHold()
    {
        if (!isHolding) return;
        isHolding = false;

        FactoryCraftDragController.Instance?.EndDrag();

        if (ownerPanel != null)
            ownerPanel.CloseUI();
    }

    private bool IsPrimaryPressed()
    {
        var mouse = UnityEngine.InputSystem.Mouse.current;
        if (mouse != null)
            return mouse.leftButton.isPressed;

        if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count > 0)
        {
            var phase = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[0].phase;
            return phase == UnityEngine.InputSystem.TouchPhase.Began
                || phase == UnityEngine.InputSystem.TouchPhase.Moved
                || phase == UnityEngine.InputSystem.TouchPhase.Stationary;
        }

        return false;
    }
}