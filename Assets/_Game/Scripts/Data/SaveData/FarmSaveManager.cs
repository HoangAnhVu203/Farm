using System.Collections.Generic;
using UnityEngine;

public class FarmSaveManager : MonoBehaviour
{
    public static FarmSaveManager Instance { get; private set; }

    private const string SAVE_KEY = "FARM_SAVE_DATA";

    [Header("Item Database")]
    [SerializeField] private List<FarmItemData> allItems = new();

    private Dictionary<string, FarmItemData> itemLookup;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        BuildLookup();
    }

    private void BuildLookup()
    {
        itemLookup = new Dictionary<string, FarmItemData>();

        for (int i = 0; i < allItems.Count; i++)
        {
            FarmItemData item = allItems[i];
            if (item == null || string.IsNullOrWhiteSpace(item.itemId)) continue;

            if (!itemLookup.ContainsKey(item.itemId))
                itemLookup.Add(item.itemId, item);
            else
                Debug.LogWarning($"Trùng FarmItemData id: {item.itemId}");
        }
    }

    public FarmItemData GetItemDataById(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        if (itemLookup == null) BuildLookup();

        itemLookup.TryGetValue(id, out var data);
        return data;
    }

    public void SavePlacedItems(List<PlacedFarmItem> placedItems)
    {
        FarmSaveData saveData = new FarmSaveData();

        for (int i = 0; i < placedItems.Count; i++)
        {
            PlacedFarmItem placed = placedItems[i];
            if (placed == null || placed.itemData == null) continue;
            if (string.IsNullOrWhiteSpace(placed.itemData.itemId)) continue;

            saveData.placedItems.Add(new PlacedItemSaveData
            {
                itemId = placed.itemData.itemId,
                originX = placed.originCell.x,
                originY = placed.originCell.y,
                originZ = placed.originCell.z
            });
        }

        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();

        Debug.Log("Saved placed items: " + json);
    }

    public FarmSaveData Load()
    {
        if (!PlayerPrefs.HasKey(SAVE_KEY))
            return new FarmSaveData();

        string json = PlayerPrefs.GetString(SAVE_KEY, "");
        if (string.IsNullOrEmpty(json))
            return new FarmSaveData();

        FarmSaveData data = JsonUtility.FromJson<FarmSaveData>(json);
        return data ?? new FarmSaveData();
    }

    public void ClearSave()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.Save();
    }
}