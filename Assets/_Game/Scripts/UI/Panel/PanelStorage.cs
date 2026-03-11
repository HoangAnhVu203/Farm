using System.Collections.Generic;
using UnityEngine;

public class PanelStorage : UICanvas
{
    [Header("UI")]
    [SerializeField] private Transform contentRoot;
    [SerializeField] private StorageItemUI storageItemPrefab;

    [Header("Optional")]
    [SerializeField] private bool hideItemsWithZeroAmount = true;

    private void OnEnable()
    {
        RefreshUI();

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged += RefreshUI;
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= RefreshUI;
    }

    public void RefreshUI()
    {
        if (contentRoot == null || storageItemPrefab == null)
        {
            Debug.LogError("[PanelStorage] Thiếu contentRoot hoặc storageItemPrefab");
            return;
        }

        ClearContent();

        if (InventoryManager.Instance == null) return;

        Dictionary<string, int> allAmounts = InventoryManager.Instance.GetAllAmounts();

        foreach (var kvp in allAmounts)
        {
            string itemId = kvp.Key;
            int amount = kvp.Value;

            if (hideItemsWithZeroAmount && amount <= 0)
                continue;

            FarmInventoryItemData itemData = InventoryManager.Instance.GetItemData(itemId);
            if (itemData == null) continue;

            StorageItemUI ui = Instantiate(storageItemPrefab, contentRoot);
            ui.Setup(itemData, amount);
        }
    }

    private void ClearContent()
    {
        for (int i = contentRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(contentRoot.GetChild(i).gameObject);
        }
    }

    public void CloseUI()
    {
        UIManager.Instance.CloseUIDirectly<PanelStorage>();
    }
}