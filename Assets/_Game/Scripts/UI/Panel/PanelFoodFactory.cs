using System.Collections.Generic;
using UnityEngine;

public class PanelFoodFactory : UICanvas
{
    public static PanelFoodFactory Instance { get; private set; }

    [Header("Recipe Buttons")]
    [SerializeField] private FactoryRecipeHoldButton chickenButton;
    [SerializeField] private FactoryRecipeHoldButton cowButton;
    [SerializeField] private FactoryRecipeHoldButton pigButton;

    [Header("Recipe Data")]
    [SerializeField] private FoodRecipeData chickenRecipe;
    [SerializeField] private FoodRecipeData cowRecipe;
    [SerializeField] private FoodRecipeData pigRecipe;

    [Header("Ingredient Table")]
    [SerializeField] private GameObject ingredientTableRoot;
    [SerializeField] private Transform ingredientContentRoot;
    [SerializeField] private IngredientRowUI ingredientRowPrefab;

    private FactoryMachine currentMachine;

    private void Awake()
    {
        Instance = this;

        if (chickenButton != null) chickenButton.Setup(this, chickenRecipe);
        if (cowButton != null) cowButton.Setup(this, cowRecipe);
        if (pigButton != null) pigButton.Setup(this, pigRecipe);

        HideIngredientTable();
    }

    public void OpenFor(FactoryMachine machine)
    {
        currentMachine = machine;
        HideIngredientTable();
        UIManager.Instance.OpenUI<PanelFoodFactory>();
    }

    public FactoryMachine GetCurrentMachine()
    {
        return currentMachine;
    }

    public bool CanCraft(FoodRecipeData recipe)
    {
        if (currentMachine == null || recipe == null) return false;
        return currentMachine.CanCraft(recipe);
    }

    public void ShowIngredientTable(FoodRecipeData recipe)
    {
        if (ingredientTableRoot == null || ingredientContentRoot == null || ingredientRowPrefab == null)
            return;

        ingredientTableRoot.SetActive(true);
        ClearIngredientRows();

        if (recipe == null || InventoryManager.Instance == null) return;

        for (int i = 0; i < recipe.ingredients.Count; i++)
        {
            RecipeIngredient ing = recipe.ingredients[i];
            if (ing == null || ing.item == null) continue;

            int have = InventoryManager.Instance.GetAmount(ing.item);

            IngredientRowUI row = Instantiate(ingredientRowPrefab, ingredientContentRoot);
            row.Setup(ing.item, have, ing.amount);
        }
    }

    public void HideIngredientTable()
    {
        if (ingredientTableRoot != null)
            ingredientTableRoot.SetActive(false);

        ClearIngredientRows();
    }

    private void ClearIngredientRows()
    {
        if (ingredientContentRoot == null) return;

        for (int i = ingredientContentRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(ingredientContentRoot.GetChild(i).gameObject);
        }
    }

    public void HidePanelOnly()
    {
        gameObject.SetActive(false);
    }

    public void CloseUI()
    {
        HideIngredientTable();
        UIManager.Instance.CloseUIDirectly<PanelFoodFactory>();
    }
}