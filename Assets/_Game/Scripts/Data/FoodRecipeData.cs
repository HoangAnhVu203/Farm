using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RecipeIngredient
{
    public FarmInventoryItemData item;
    public int amount = 1;
}

[CreateAssetMenu(menuName = "Farm/Food Recipe Data")]
public class FoodRecipeData : ScriptableObject
{
    public string id;
    public string recipeName;
    public Sprite icon;

    [Header("Output")]
    public FarmInventoryItemData outputItem;
    public int outputAmount = 1;
    public float craftDurationSeconds = 10f;

    [Header("Ingredients")]
    public List<RecipeIngredient> ingredients = new();
}