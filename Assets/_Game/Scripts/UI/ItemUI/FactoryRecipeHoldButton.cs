using UnityEngine;
using UnityEngine.EventSystems;

public class FactoryRecipeHoldButton : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [SerializeField] private PanelFoodFactory ownerPanel;
    [SerializeField] private FoodRecipeData recipeData;

    private bool isHolding;
    private bool dragStarted;

    public void Setup(PanelFoodFactory panel, FoodRecipeData recipe)
    {
        ownerPanel = panel;
        recipeData = recipe;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (ownerPanel == null || recipeData == null) return;

        isHolding = true;
        dragStarted = false;

        ownerPanel.ShowIngredientTable(recipeData);
        Debug.Log("[FactoryButton] PointerDown -> show ingredient");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isHolding) return;
        if (ownerPanel == null || recipeData == null) return;

        if (!ownerPanel.CanCraft(recipeData))
        {
            Debug.Log("[FactoryButton] Không đủ nguyên liệu, không cho drag");
            return;
        }

        dragStarted = true;

        FactoryCraftDragController.Instance?.BeginDrag(recipeData, ownerPanel.GetCurrentMachine());

        // Ẩn phần nút recipe, nhưng không tắt cả panel script
        ownerPanel.HideVisualOnly();

        Debug.Log("[FactoryButton] BeginDrag");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragStarted) return;

        // Không cần làm gì ở đây nếu icon kéo đã tự follow chuột trong CursorUI
        // Giữ lại để Unity xác nhận đây là drag thật
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("[FactoryButton] EndDrag");
        FinishHoldAndDrop();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Trường hợp chỉ bấm rồi thả, hoặc một số device không gọi EndDrag như mong đợi
        Debug.Log("[FactoryButton] PointerUp");
        FinishHoldAndDrop();
    }

    private void FinishHoldAndDrop()
    {
        if (!isHolding) return;

        isHolding = false;

        if (dragStarted)
        {
            FactoryCraftDragController.Instance?.EndDrag();
            ownerPanel?.CloseUI();
        }
        else
        {
            ownerPanel?.HideIngredientTable();
        }

        dragStarted = false;
    }
}