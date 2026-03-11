using UnityEngine;

public class SoilPlot : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private SpriteRenderer soilRenderer;
    [SerializeField] private SpriteRenderer cropRenderer;

    private CropSeedData currentSeed;
    private long plantedUnixTime;
    private bool isPlanted;
    private CropGrowthStage currentStage = CropGrowthStage.Empty;

    public bool IsPlanted => isPlanted;
    public bool IsReadyToHarvest => isPlanted && currentStage == CropGrowthStage.ReadyToHarvest;
    public CropSeedData CurrentSeed => currentSeed;
    public long PlantedUnixTime => plantedUnixTime;

    public Vector3Int OriginCell
    {
        get
        {
            var placed = GetComponent<PlacedFarmItem>();
            if (placed != null)
                return placed.originCell;

            return Vector3Int.zero;
        }
    }

    public string SaveKey
    {
        get
        {
            Vector3Int c = OriginCell;
            return $"soil_{c.x}_{c.y}_{c.z}";
        }
    }

    private Vector3 cropStartLocalPos;

    private void Awake()
    {
        if (soilRenderer != null && cropRenderer != null)
        {
            cropRenderer.sortingLayerID = soilRenderer.sortingLayerID;
            cropRenderer.sortingOrder = soilRenderer.sortingOrder + 1;
            cropStartLocalPos = cropRenderer.transform.localPosition;
        }

        RefreshVisual();
    }

    private void Start()
    {
        SoilSaveManager.Instance?.RegisterPlot(this);
    }

    private void Update()
    {
        if (!isPlanted || currentSeed == null) return;
        UpdateGrowth();
    }

    public bool CanPlant()
    {
        return !isPlanted;
    }

    public bool CanMove()
    {
        if (IsReadyToHarvest) return false;
        return isPlanted;
    }

    public void Plant(CropSeedData seedData)
    {
        if (seedData == null) return;
        if (!CanPlant()) return;

        currentSeed = seedData;
        plantedUnixTime = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        isPlanted = true;
        currentStage = CropGrowthStage.Stage1;

        if (seedData.seedIcon != null && SowFXSpawner.Instance != null)
        {
            SowFXSpawner.Instance.PlaySowPopup(transform.position, seedData.seedIcon);
        }

        RefreshVisual();
        SoilSaveManager.Instance?.SaveAllPlots();
    }

    public void ClearPlot()
    {
        currentSeed = null;
        plantedUnixTime = 0;
        isPlanted = false;
        currentStage = CropGrowthStage.Empty;

        RefreshVisual();
        SoilSaveManager.Instance?.SaveAllPlots();
    }

    public void Harvest()
    {
        if (!IsReadyToHarvest) return;

        Sprite popupSprite = null;

        if (currentSeed != null &&
            currentSeed.harvestItem != null)
        {
            popupSprite = currentSeed.harvestItem.icon;

            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddItem(currentSeed.harvestItem, currentSeed.harvestAmount);
            }
        }

        if (popupSprite != null && HarvestFXSpawner.Instance != null)
        {
            HarvestFXSpawner.Instance.PlayHarvestPopup(transform.position, popupSprite);
        }

        currentSeed = null;
        plantedUnixTime = 0;
        isPlanted = false;
        currentStage = CropGrowthStage.Empty;

        RefreshVisual();
        SoilSaveManager.Instance?.SaveAllPlots();
    }

    public void LoadState(CropSeedData seedData, long plantedTime, bool planted)
    {
        currentSeed = seedData;
        plantedUnixTime = plantedTime;
        isPlanted = planted;

        if (!isPlanted || currentSeed == null)
            currentStage = CropGrowthStage.Empty;
        else
            UpdateGrowthImmediate();

        RefreshVisual();
    }

    private void UpdateGrowth()
    {
        CropGrowthStage newStage = GetStageByTime();

        if (newStage != currentStage)
        {
            currentStage = newStage;
            RefreshVisual();
        }
    }

    private void UpdateGrowthImmediate()
    {
        currentStage = GetStageByTime();
    }

    private CropGrowthStage GetStageByTime()
    {
        if (!isPlanted || currentSeed == null)
            return CropGrowthStage.Empty;

        long now = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        double elapsed = now - plantedUnixTime;
        double halfTime = currentSeed.growDurationSeconds * 0.5f;

        if (elapsed < halfTime)
            return CropGrowthStage.Stage1;

        if (elapsed < currentSeed.growDurationSeconds)
            return CropGrowthStage.Stage2;

        return CropGrowthStage.ReadyToHarvest;
    }

    private void RefreshVisual()
    {
        if (cropRenderer == null) return;

        if (!isPlanted || currentSeed == null)
        {
            cropRenderer.sprite = null;
            cropRenderer.enabled = false;
            cropRenderer.transform.localPosition = cropStartLocalPos;
            return;
        }

        cropRenderer.enabled = true;

        switch (currentStage)
        {
            case CropGrowthStage.Stage1:
                cropRenderer.sprite = currentSeed.stage1Sprite;
                break;

            case CropGrowthStage.Stage2:
                cropRenderer.sprite = currentSeed.stage2Sprite;
                break;

            case CropGrowthStage.ReadyToHarvest:
                cropRenderer.sprite = currentSeed.stage3Sprite;
                break;
        }

        MatchCropBottomToSoil();
    }

    private void MatchCropBottomToSoil()
    {
        if (soilRenderer == null || cropRenderer == null) return;
        if (cropRenderer.sprite == null) return;

        cropRenderer.transform.localPosition = cropStartLocalPos;

        Bounds soilBounds = soilRenderer.bounds;
        Bounds cropBounds = cropRenderer.bounds;

        float deltaY = soilBounds.min.y - cropBounds.min.y;

        Vector3 worldPos = cropRenderer.transform.position;
        worldPos.y += deltaY;

        if (cropRenderer.transform.parent != null)
        {
            cropRenderer.transform.localPosition =
                cropRenderer.transform.parent.InverseTransformPoint(worldPos);
        }
        else
        {
            cropRenderer.transform.position = worldPos;
        }
    }
}