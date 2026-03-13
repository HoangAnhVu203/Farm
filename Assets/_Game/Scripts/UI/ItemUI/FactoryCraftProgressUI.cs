using UnityEngine;
using UnityEngine.UI;

public class FactoryCraftProgressUI : MonoBehaviour
{
    public static FactoryCraftProgressUI Instance { get; private set; }

    [Header("Refs")]
    [SerializeField] private Camera worldCamera;
    [SerializeField] private RectTransform root;
    [SerializeField] private Image progressFilled;
    [SerializeField] private Text timeText;
    [SerializeField] private Button finishButton;
    [SerializeField] private Text priceText;

    [Header("Follow")]
    [SerializeField] private Vector2 screenOffset = new Vector2(0f, 110f);

    [Header("Config")]
    [SerializeField] private int instantFinishCoinCost = 5;

    private FactoryMachine targetMachine;
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
        if (targetMachine == null)
        {
            Hide();
            return;
        }

        if (!targetMachine.IsCrafting || targetMachine.CurrentRecipe == null)
        {
            Hide();
            return;
        }

        RefreshPosition();
        RefreshData();
    }

    public void Show(FactoryMachine machine)
    {
        if (machine == null)
        {
            Hide();
            return;
        }

        if (!machine.IsCrafting || machine.CurrentRecipe == null)
        {
            Hide();
            return;
        }

        targetMachine = machine;
        gameObject.SetActive(true);

        RefreshPosition();
        RefreshData();
    }

    public void Hide()
    {
        targetMachine = null;
        gameObject.SetActive(false);
    }

    private void RefreshData()
    {
        if (targetMachine == null) return;

        float progress = targetMachine.GetProgress01();
        float remain = targetMachine.GetRemainingSeconds();

        if (progressFilled != null)
        {
            // giống cây: đầy -> hết dần
            progressFilled.fillAmount = 1f - progress;
        }

        if (timeText != null)
        {
            timeText.text = FormatTime(remain);
        }

        if (priceText != null)
        {
            priceText.text = instantFinishCoinCost.ToString();
        }
    }

    private void RefreshPosition()
    {
        if (root == null || worldCamera == null || targetMachine == null || parentCanvas == null) return;

        Vector3 worldPos = targetMachine.transform.position;
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
        if (targetMachine == null) return;

        // Sau này thêm check/trừ coin ở đây
        targetMachine.CompleteCraft();
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