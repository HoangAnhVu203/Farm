using System.Collections.Generic;
using UnityEngine;

public class PanelShop : UICanvas
{
    [Header("Data")]
    [SerializeField] private List<FarmItemData> shopItems = new();

    [Header("UI")]
    [SerializeField] private Transform contentRoot;
    [SerializeField] private ShopItemView shopItemPrefab;

    [Header("Optional")]
    [SerializeField] private bool clearOldChildrenOnBuild = true;

    private void Start()
    {
        BuildShop();
    }

    public void BuildShop()
    {
        if (contentRoot == null || shopItemPrefab == null)
        {
            Debug.LogError("PanelShop thiếu contentRoot hoặc shopItemPrefab");
            return;
        }

        if (clearOldChildrenOnBuild)
        {
            ClearContent();
        }

        for (int i = 0; i < shopItems.Count; i++)
        {
            FarmItemData data = shopItems[i];
            if (data == null) continue;

            ShopItemView itemView = Instantiate(shopItemPrefab, contentRoot);
            itemView.Setup(data, this);
        }
    }

    public void BuyItem(FarmItemData itemData)
    {
        if (itemData == null) return;
        if (FarmPlacementController.Instance == null) return;

        FarmPlacementController.Instance.StartPlacingNewItem(itemData);

        // Nếu UICanvas của bạn có Hide() thì dùng Hide().
        // Nếu chưa có thì cứ tắt object luôn.
        gameObject.SetActive(false);
    }

    void ClearContent()
    {
        for (int i = contentRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(contentRoot.GetChild(i).gameObject);
        }
    }
    
    public void CloseUI()
    {
        UIManager.Instance.CloseUIDirectly<PanelShop>();
    }
}