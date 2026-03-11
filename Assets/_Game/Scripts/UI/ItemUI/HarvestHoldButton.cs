using UnityEngine;
using UnityEngine.EventSystems;

public class HarvestHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField] private PanelHarvest ownerPanel;

    private bool isHolding;

    private void Awake()
    {
        if (ownerPanel == null)
            ownerPanel = GetComponentInParent<PanelHarvest>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isHolding = true;

        HarvestController.Instance?.BeginHarvestHold();

        if (ownerPanel != null)
            ownerPanel.HidePanelOnly();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StopHold();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // không bắt buộc
        // nếu muốn kéo ra ngoài button vẫn tiếp tục harvest thì KHÔNG stop ở đây
    }

    private void Update()
    {
        // backup: nếu đang giữ mà object bị mất focus thì vẫn stop
        if (isHolding && HarvestController.Instance != null && !HarvestController.Instance.IsHarvestMode)
        {
            isHolding = false;
        }
    }

    private void StopHold()
    {
        if (!isHolding) return;
        isHolding = false;

        HarvestController.Instance?.EndHarvestHold();

        if (ownerPanel != null)
            ownerPanel.CloseUI();
    }
}