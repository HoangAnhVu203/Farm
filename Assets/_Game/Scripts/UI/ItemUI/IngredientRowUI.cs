using UnityEngine;
using UnityEngine.UI;

public class IngredientRowUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Text amountText;

    [Header("Color")]
    [SerializeField] private Color enoughColor = Color.green;
    [SerializeField] private Color notEnoughColor = Color.red;

    public void Setup(FarmInventoryItemData itemData, int haveAmount, int needAmount)
    {
        if (iconImage != null)
            iconImage.sprite = itemData != null ? itemData.icon : null;

        if (amountText != null)
        {
            amountText.text = $"{haveAmount}/{needAmount}";
            amountText.color = haveAmount >= needAmount ? enoughColor : notEnoughColor;
        }
    }
}