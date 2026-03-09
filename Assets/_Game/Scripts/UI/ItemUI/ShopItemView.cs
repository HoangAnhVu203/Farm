using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Text priceText;
    [SerializeField] private Button buyButton;

    private FarmItemData itemData;
    private PanelShop ownerPanel;

    public void Setup(FarmItemData data, PanelShop panel)
    {
        itemData = data;
        ownerPanel = panel;

        if (iconImage != null)
            iconImage.sprite = data.icon;

        if (priceText != null)
            priceText.text = data.price.ToString();
    }

    private void Awake()
    {
        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnClickBuy);
        }
    }

    void OnClickBuy()
    {
        if (ownerPanel == null || itemData == null) return;
        ownerPanel.BuyItem(itemData);
    }
}