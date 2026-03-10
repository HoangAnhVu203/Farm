using UnityEngine;

public class SoilPlot : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private SpriteRenderer soilRenderer;
    [SerializeField] private SpriteRenderer cropRenderer;

    private CropSeedData currentSeed;
    private float plantedTime;
    private bool isPlanted;
    private CropGrowthStage currentStage = CropGrowthStage.Empty;

    public bool IsPlanted => isPlanted;
    public bool IsReadyToHarvest => isPlanted && currentStage == CropGrowthStage.ReadyToHarvest;
    public CropSeedData CurrentSeed => currentSeed;

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

    private void Update()
    {
        if (!isPlanted || currentSeed == null) return;
        UpdateGrowth();
    }

    public bool CanPlant()
    {
        return !isPlanted;
    }

    public void Plant(CropSeedData seedData)
    {
        if (seedData == null) return;
        if (!CanPlant()) return;

        currentSeed = seedData;
        isPlanted = true;

        if (cropRenderer != null)
        {
            cropRenderer.sprite = seedData.stage1Sprite;
            cropRenderer.enabled = true;
        }
    }

    public void ClearPlot()
    {
        currentSeed = null;
        isPlanted = false;

        if (cropRenderer != null)
        {
            cropRenderer.sprite = null;
            cropRenderer.enabled = false;
        }
    }

    public void Harvest()
    {
        if (!IsReadyToHarvest) return;

        // TODO: cộng item vào kho tại đây nếu cần

        currentSeed = null;
        plantedTime = 0f;
        isPlanted = false;
        currentStage = CropGrowthStage.Empty;

        RefreshVisual();
    }

    private void UpdateGrowth()
    {
        float elapsed = Time.time - plantedTime;
        float halfTime = currentSeed.growDurationSeconds * 0.5f;

        CropGrowthStage newStage;

        if (elapsed < halfTime)
            newStage = CropGrowthStage.Stage1;
        else if (elapsed < currentSeed.growDurationSeconds)
            newStage = CropGrowthStage.Stage2;
        else
            newStage = CropGrowthStage.ReadyToHarvest;

        if (newStage != currentStage)
        {
            currentStage = newStage;
            RefreshVisual();
        }
    }

    

    private void OnMouseDown()
    {
        if (IsReadyToHarvest)
        {
            Harvest();
        }
    }

    private void OnMouseUpAsButton()
    {
        if (IsReadyToHarvest)
        {
            Harvest();
            return;
        }

        if (!isPlanted)
        {
            UIManager.Instance.OpenUI<PanelSow>();
        }
    }

    
}