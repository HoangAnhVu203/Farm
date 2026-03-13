using System.Collections.Generic;
using UnityEngine;

public class PanelFoodFactory : UICanvas
{
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

    [Header("Visual Root")]
    [SerializeField] private GameObject visualRoot;

    private FactoryMachine currentMachine;

    private void Awake()
    {
       
    }

    private void OnEnable()
    {
        if (chickenButton != null) chickenButton.Setup(this, chickenRecipe);
        if (cowButton != null) cowButton.Setup(this, cowRecipe);
        if (pigButton != null) pigButton.Setup(this, pigRecipe);

        ShowVisual();
        HideIngredientTable();
    }

    public static void Open(FactoryMachine machine)
    {
        if (UIManager.Instance == null)
        {
            Debug.LogError("UIManager.Instance is NULL");
            return;
        }

        PanelFoodFactory panel = UIManager.Instance.OpenUI<PanelFoodFactory>();
        if (panel == null)
        {
            Debug.LogError("Không mở được PanelFoodFactory");
            return;
        }

        panel.BindMachine(machine);
    }

    public void BindMachine(FactoryMachine machine)
    {
        currentMachine = machine;
        ShowVisual();
        HideIngredientTable();
    }

    public void OpenFor(FactoryMachine machine)
    {
        currentMachine = machine;
        ShowVisual();
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
        ingredientTableRoot.transform.SetAsLastSibling();
        Debug.Log(">>> ShowIngredientTable called");

        if (ingredientTableRoot == null)
        {
            Debug.LogError("ingredientTableRoot NULL");
            return;
        }

        ingredientTableRoot.SetActive(true);
        Debug.Log(">>> Ingredient table ACTIVE = true");

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
        Debug.Log(">>> HideIngredientTable called");

        if (ingredientTableRoot != null)
        {
            ingredientTableRoot.SetActive(false);
            Debug.Log(">>> Ingredient table ACTIVE = false");
        }

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

    public void HideVisualOnly()
    {
        Debug.Log(">>> HideVisualOnly called");

        if (visualRoot != null)
            visualRoot.SetActive(false);
    }

    public void ShowVisual()
    {
        if (visualRoot != null)
            visualRoot.SetActive(true);
    }

    public void CloseUI()
    {
        ShowVisual();
        HideIngredientTable();
        UIManager.Instance.CloseUIDirectly<PanelFoodFactory>();
    }

    
}