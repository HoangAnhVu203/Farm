using UnityEngine;
using UnityEngine.UI;

public class PanelSow : UICanvas
{
    [Header("Buttons")]
    [SerializeField] private Button riceButton;
    [SerializeField] private Button cornButton;
    [SerializeField] private Button carrotButton;
    [SerializeField] private Button tomatoButton;
    [SerializeField] private Button pumpkinButton;

    [Header("Seed Data")]
    [SerializeField] private CropSeedData riceSeed;
    [SerializeField] private CropSeedData cornSeed;
    [SerializeField] private CropSeedData carrotSeed;
    [SerializeField] private CropSeedData tomatoSeed;
    [SerializeField] private CropSeedData pumpkinSeed;

    [Header("Refs")]
    [SerializeField] private SoilInteractionController soilInteractionController;

    private void Awake()
    {
        if (soilInteractionController == null)
            soilInteractionController = SoilInteractionController.Instance;

        BindButtons();
    }

    private void OnEnable()
    {
        if (soilInteractionController == null)
            soilInteractionController = SoilInteractionController.Instance;
    }

    private void BindButtons()
    {
        if (riceButton != null)
        {
            riceButton.onClick.RemoveAllListeners();
            riceButton.onClick.AddListener(() => SelectSeed(riceSeed));
        }

        if (cornButton != null)
        {
            cornButton.onClick.RemoveAllListeners();
            cornButton.onClick.AddListener(() => SelectSeed(cornSeed));
        }

        if (carrotButton != null)
        {
            carrotButton.onClick.RemoveAllListeners();
            carrotButton.onClick.AddListener(() => SelectSeed(carrotSeed));
        }

        if (tomatoButton != null)
        {
            tomatoButton.onClick.RemoveAllListeners();
            tomatoButton.onClick.AddListener(() => SelectSeed(tomatoSeed));
        }

        if (pumpkinButton != null)
        {
            pumpkinButton.onClick.RemoveAllListeners();
            pumpkinButton.onClick.AddListener(() => SelectSeed(pumpkinSeed));
        }
    }

    public void SelectSeed(CropSeedData seed)
    {
        if (seed == null) return;

        if (soilInteractionController == null)
            soilInteractionController = SoilInteractionController.Instance;

        if (soilInteractionController == null)
        {
            Debug.LogWarning("[PanelSow] Không tìm thấy SoilInteractionController.");
            return;
        }

        SoilPlot soil = soilInteractionController.GetSelectedSoil();
        if (soil != null && soil.CanPlant())
        {
            soil.Plant(seed);
        }

        UIManager.Instance.CloseUIDirectly<PanelSow>();
    }

    public void CloseUI()
    {
        UIManager.Instance.CloseUIDirectly<PanelSow>();
    }
}