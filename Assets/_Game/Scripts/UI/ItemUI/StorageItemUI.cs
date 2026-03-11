using UnityEngine;
using UnityEngine.UI;

public class StorageItemUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Text amountText;

    private FarmInventoryItemData itemData;

    public void Setup(FarmInventoryItemData data, int amount)
    {
        itemData = data;

        if (iconImage != null)
            iconImage.sprite = data != null ? data.icon : null;

        if (amountText != null)
            amountText.text = amount.ToString();
    }
}