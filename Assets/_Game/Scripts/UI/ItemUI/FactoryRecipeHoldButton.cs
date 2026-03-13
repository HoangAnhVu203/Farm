using UnityEngine;
using UnityEngine.EventSystems;

public class FactoryRecipeHoldButton : MonoBehaviour,
    IPointerDownHandler,
    IBeginDragHandler,
    IDragHandler
{
    [SerializeField] private PanelFoodFactory ownerPanel;
    [SerializeField] private FoodRecipeData recipeData;

    // private bool dragStarted;

    public void Setup(PanelFoodFactory panel, FoodRecipeData recipe)
    {
        ownerPanel = panel;
        recipeData = recipe;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (ownerPanel == null || recipeData == null) return;

        // dragStarted = false;
        ownerPanel.ShowIngredientTable(recipeData);

        Debug.Log("[FactoryButton] PointerDown -> show ingredient");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (ownerPanel == null || recipeData == null) return;

        if (!ownerPanel.CanCraft(recipeData))
        {
            Debug.Log("[FactoryButton] Không đủ nguyên liệu, không cho drag");
            return;
        }

        // dragStarted = true;

        FactoryCraftDragController.Instance?.BeginDrag(recipeData, ownerPanel.GetCurrentMachine());

        // Chỉ ẩn phần nhìn, vì controller global sẽ tự xử lý lúc thả
        ownerPanel.HideVisualOnly();

        Debug.Log("[FactoryButton] BeginDrag");
    }

    public void OnDrag(PointerEventData eventData)
    {
        // để trống, chỉ cần implement để event drag hoạt động ổn định
    }
}