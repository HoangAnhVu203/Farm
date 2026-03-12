using System;
using UnityEngine;

public class FactoryMachine : MonoBehaviour
{
    [SerializeField] private Collider2D targetCollider;

    private FoodRecipeData currentRecipe;
    private long craftStartUnixMs;
    private bool isCrafting;

    public bool IsCrafting => isCrafting;
    public FoodRecipeData CurrentRecipe => currentRecipe;

    private void Awake()
    {
        if (targetCollider == null)
            targetCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (!isCrafting || currentRecipe == null) return;

        if (GetRemainingSeconds() <= 0f)
        {
            CompleteCraft();
        }
    }

    public bool CanCraft(FoodRecipeData recipe)
    {
        if (recipe == null) return false;
        if (isCrafting) return false;
        if (InventoryManager.Instance == null) return false;

        for (int i = 0; i < recipe.ingredients.Count; i++)
        {
            RecipeIngredient ing = recipe.ingredients[i];
            if (ing == null || ing.item == null) return false;

            if (!InventoryManager.Instance.HasEnough(ing.item, ing.amount))
                return false;
        }

        return true;
    }

    public bool StartCraft(FoodRecipeData recipe)
    {
        if (!CanCraft(recipe)) return false;

        for (int i = 0; i < recipe.ingredients.Count; i++)
        {
            RecipeIngredient ing = recipe.ingredients[i];
            if (ing != null && ing.item != null)
            {
                InventoryManager.Instance.RemoveItem(ing.item, ing.amount);
            }
        }

        currentRecipe = recipe;
        craftStartUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        isCrafting = true;

        return true;
    }

    public float GetProgress01()
    {
        if (!isCrafting || currentRecipe == null || currentRecipe.craftDurationSeconds <= 0f)
            return 0f;

        double nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        double elapsed = (nowMs - craftStartUnixMs) / 1000.0;
        return Mathf.Clamp01((float)(elapsed / currentRecipe.craftDurationSeconds));
    }

    public float GetRemainingSeconds()
    {
        if (!isCrafting || currentRecipe == null) return 0f;

        double nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        double elapsed = (nowMs - craftStartUnixMs) / 1000.0;
        float remain = currentRecipe.craftDurationSeconds - (float)elapsed;
        return Mathf.Max(0f, remain);
    }

    public void CompleteCraft()
    {
        if (!isCrafting || currentRecipe == null) return;

        if (InventoryManager.Instance != null &&
            currentRecipe.outputItem != null)
        {
            InventoryManager.Instance.AddItem(currentRecipe.outputItem, currentRecipe.outputAmount);
        }

        currentRecipe = null;
        craftStartUnixMs = 0;
        isCrafting = false;
    }

    public Collider2D GetTargetCollider()
    {
        return targetCollider;
    }
}