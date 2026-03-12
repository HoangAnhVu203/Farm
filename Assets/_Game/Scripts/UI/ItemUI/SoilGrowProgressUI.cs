using UnityEngine;
using UnityEngine.UI;

public class SoilGrowProgressUI : MonoBehaviour
{
    public static SoilGrowProgressUI Instance { get; private set; }

    [Header("Refs")]
    [SerializeField] private Camera worldCamera;
    [SerializeField] private RectTransform root;
    [SerializeField] private Image progressFilled;
    [SerializeField] private Text timeText;
    [SerializeField] private Button finishButton;
    [SerializeField] private Text priceText;

    [Header("Follow")]
    [SerializeField] private Vector2 screenOffset = new Vector2(0f, 110f);

    private SoilPlot targetSoil;
    private Canvas parentCanvas;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (worldCamera == null)
            worldCamera = Camera.main;

        parentCanvas = GetComponentInParent<Canvas>();

        if (finishButton != null)
        {
            finishButton.onClick.RemoveAllListeners();
            finishButton.onClick.AddListener(OnClickFinishNow);
        }

        Hide();
    }

    private void Update()
    {
        if (targetSoil == null)
        {
            Hide();
            return;
        }

        if (!targetSoil.IsPlanted || targetSoil.IsReadyToHarvest)
        {
            Hide();
            return;
        }

        RefreshPosition();
        RefreshData();
    }

    public void Show(SoilPlot soil)
    {
        if (soil == null)
        {
            Hide();
            return;
        }

        if (!soil.IsPlanted || soil.IsReadyToHarvest)
        {
            Hide();
            return;
        }

        targetSoil = soil;
        gameObject.SetActive(true);

        RefreshPosition();
        RefreshData();
    }

    public void Hide()
    {
        targetSoil = null;
        gameObject.SetActive(false);
    }

    private void RefreshData()
    {
        if (targetSoil == null) return;

        float progress = targetSoil.GetGrowProgress01();
        float remain = targetSoil.GetRemainingSeconds();

        if (progressFilled != null)
        {
            // bạn muốn từ 1 về 0
            progressFilled.fillAmount = 1f - progress;
        }

        if (timeText != null)
        {
            timeText.text = FormatTime(remain);
        }

        if (priceText != null)
        {
            int cost = targetSoil.CurrentSeed != null ? targetSoil.CurrentSeed.instantFinishCoinCost : 0;
            priceText.text = cost.ToString();
        }
    }

    private void RefreshPosition()
    {
        if (root == null || worldCamera == null || targetSoil == null || parentCanvas == null) return;

        Vector3 worldPos = targetSoil.transform.position;
        Vector3 screenPos = worldCamera.WorldToScreenPoint(worldPos);

        RectTransform canvasRect = parentCanvas.transform as RectTransform;
        Vector2 anchoredPos;

        if (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, screenPos, null, out anchoredPos);
        }
        else
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, screenPos, worldCamera, out anchoredPos);
        }

        root.anchoredPosition = anchoredPos + screenOffset;
    }

    private void OnClickFinishNow()
    {
        if (targetSoil == null) return;

        // Sau này thay bằng check/trừ coin
        // int cost = targetSoil.CurrentSeed.instantFinishCoinCost;

        targetSoil.FinishInstantly();
        Hide();
    }

    private string FormatTime(float seconds)
    {
        int total = Mathf.CeilToInt(seconds);
        int minutes = total / 60;
        int sec = total % 60;
        return $"{minutes:00}:{sec:00}";
    }
}