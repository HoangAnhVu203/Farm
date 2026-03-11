using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoilSaveManager : MonoBehaviour
{
    public static SoilSaveManager Instance { get; private set; }

    private const string SAVE_KEY = "FARM_SOIL_SAVE";

    [Header("Seed Database")]
    [SerializeField] private List<CropSeedData> allSeeds = new();

    private readonly Dictionary<string, SoilPlot> registeredPlots = new();
    private readonly Dictionary<string, CropSeedData> seedLookup = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        BuildSeedLookup();
    }

    private IEnumerator Start()
    {
        yield return null;
        LoadAllPlots();
    }

    private void BuildSeedLookup()
    {
        seedLookup.Clear();

        for (int i = 0; i < allSeeds.Count; i++)
        {
            CropSeedData seed = allSeeds[i];
            if (seed == null || string.IsNullOrWhiteSpace(seed.id)) continue;

            if (!seedLookup.ContainsKey(seed.id))
                seedLookup.Add(seed.id, seed);
        }
    }

    public void RegisterPlot(SoilPlot plot)
    {
        if (plot == null) return;
        if (string.IsNullOrWhiteSpace(plot.SaveKey)) return;

        registeredPlots[plot.SaveKey] = plot;
    }

    public void UnregisterPlot(SoilPlot plot)
    {
        if (plot == null) return;
        if (string.IsNullOrWhiteSpace(plot.SaveKey)) return;

        if (registeredPlots.ContainsKey(plot.SaveKey))
            registeredPlots.Remove(plot.SaveKey);
    }

    public void SaveAllPlots()
    {
        SoilSaveFile saveFile = new SoilSaveFile();

        foreach (var kvp in registeredPlots)
        {
            SoilPlot plot = kvp.Value;
            if (plot == null) continue;

            SoilPlotSaveData data = new SoilPlotSaveData
            {
                plotKey = plot.SaveKey,
                isPlanted = plot.IsPlanted,
                seedId = plot.CurrentSeed != null ? plot.CurrentSeed.id : "",
                plantedUnixTime = plot.IsPlanted ? plot.PlantedUnixTime : 0
            };

            saveFile.plots.Add(data);
        }

        string json = JsonUtility.ToJson(saveFile);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    public void LoadAllPlots()
    {
        if (!PlayerPrefs.HasKey(SAVE_KEY)) return;

        string json = PlayerPrefs.GetString(SAVE_KEY, "");
        if (string.IsNullOrEmpty(json)) return;

        SoilSaveFile saveFile = JsonUtility.FromJson<SoilSaveFile>(json);
        if (saveFile == null || saveFile.plots == null) return;

        for (int i = 0; i < saveFile.plots.Count; i++)
        {
            SoilPlotSaveData data = saveFile.plots[i];
            if (data == null || string.IsNullOrWhiteSpace(data.plotKey)) continue;

            if (!registeredPlots.TryGetValue(data.plotKey, out SoilPlot plot) || plot == null)
                continue;

            CropSeedData seedData = null;
            if (!string.IsNullOrWhiteSpace(data.seedId))
                seedLookup.TryGetValue(data.seedId, out seedData);

            plot.LoadState(seedData, data.plantedUnixTime, data.isPlanted);
        }
    }

    public void ClearSave()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.Save();
    }
}