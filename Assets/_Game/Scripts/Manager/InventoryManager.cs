using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    private const string SAVE_KEY = "FARM_INVENTORY_DATA";

    [Header("Database")]
    [SerializeField] private List<FarmInventoryItemData> allItems = new();

    private Dictionary<string, FarmInventoryItemData> itemLookup = new();
    private Dictionary<string, int> itemAmounts = new();

    public System.Action OnInventoryChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        BuildLookup();
        LoadInventory();
    }

    private void BuildLookup()
    {
        itemLookup.Clear();

        for (int i = 0; i < allItems.Count; i++)
        {
            FarmInventoryItemData item = allItems[i];
            if (item == null || string.IsNullOrWhiteSpace(item.id)) continue;

            if (!itemLookup.ContainsKey(item.id))
                itemLookup.Add(item.id, item);
            else
                Debug.LogWarning($"[Inventory] Trùng item id: {item.id}");
        }
    }

    public FarmInventoryItemData GetItemData(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId)) return null;
        itemLookup.TryGetValue(itemId, out var item);
        return item;
    }

    public int GetAmount(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId)) return 0;
        return itemAmounts.TryGetValue(itemId, out int amount) ? amount : 0;
    }

    public int GetAmount(FarmInventoryItemData itemData)
    {
        if (itemData == null) return 0;
        return GetAmount(itemData.id);
    }

    public bool HasEnough(FarmInventoryItemData itemData, int amount)
    {
        if (itemData == null) return false;
        return GetAmount(itemData.id) >= amount;
    }

    public void AddItem(FarmInventoryItemData itemData, int amount)
    {
        if (itemData == null) return;
        if (amount <= 0) return;
        if (string.IsNullOrWhiteSpace(itemData.id)) return;

        if (!itemAmounts.ContainsKey(itemData.id))
            itemAmounts[itemData.id] = 0;

        itemAmounts[itemData.id] += amount;

        SaveInventory();
        OnInventoryChanged?.Invoke();

        Debug.Log($"[Inventory] Add {amount} {itemData.itemName} | Total = {itemAmounts[itemData.id]}");
    }

    public bool RemoveItem(FarmInventoryItemData itemData, int amount)
    {
        if (itemData == null) return false;
        if (amount <= 0) return false;
        if (string.IsNullOrWhiteSpace(itemData.id)) return false;

        if (!itemAmounts.ContainsKey(itemData.id)) return false;
        if (itemAmounts[itemData.id] < amount) return false;

        itemAmounts[itemData.id] -= amount;

        if (itemAmounts[itemData.id] <= 0)
            itemAmounts.Remove(itemData.id);

        SaveInventory();
        OnInventoryChanged?.Invoke();

        Debug.Log($"[Inventory] Remove {amount} {itemData.itemName}");
        return true;
    }

    public Dictionary<string, int> GetAllAmounts()
    {
        return new Dictionary<string, int>(itemAmounts);
    }

    private void SaveInventory()
    {
        InventorySaveData saveData = new InventorySaveData();

        foreach (var kvp in itemAmounts)
        {
            saveData.entries.Add(new InventoryEntrySaveData
            {
                itemId = kvp.Key,
                amount = kvp.Value
            });
        }

        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    private void LoadInventory()
    {
        itemAmounts.Clear();

        if (!PlayerPrefs.HasKey(SAVE_KEY))
            return;

        string json = PlayerPrefs.GetString(SAVE_KEY, "");
        if (string.IsNullOrEmpty(json))
            return;

        InventorySaveData saveData = JsonUtility.FromJson<InventorySaveData>(json);
        if (saveData == null || saveData.entries == null) return;

        for (int i = 0; i < saveData.entries.Count; i++)
        {
            var entry = saveData.entries[i];
            if (entry == null) continue;
            if (string.IsNullOrWhiteSpace(entry.itemId)) continue;
            if (entry.amount <= 0) continue;

            itemAmounts[entry.itemId] = entry.amount;
        }
    }

    public void ClearInventory()
    {
        itemAmounts.Clear();
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.Save();
        OnInventoryChanged?.Invoke();
    }

}