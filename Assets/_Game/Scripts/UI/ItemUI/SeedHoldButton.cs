using UnityEngine;
using UnityEngine.EventSystems;

public class SeedHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private PanelSow ownerPanel;
    [SerializeField] private CropSeedData seedData;

    private bool isHolding;

    private void Awake()
    {
        if (ownerPanel == null)
            ownerPanel = GetComponentInParent<PanelSow>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (seedData == null) return;

        isHolding = true;

        SowController.Instance?.BeginSowHold(seedData);

        if (ownerPanel != null)
            ownerPanel.HidePanelOnly();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StopHold();
    }

    private void Update()
    {
        if (!isHolding) return;

        // backup: nếu mode đã tắt thì reset cờ
        if (SowController.Instance == null || !SowController.Instance.IsSowMode)
        {
            isHolding = false;
            return;
        }

        // backup: nếu đã thả chuột/tay ngoài button thì vẫn tự stop
        if (!IsPrimaryPressed())
        {
            StopHold();
        }
    }

    private void StopHold()
    {
        if (!isHolding) return;
        isHolding = false;

        SowController.Instance?.EndSowHold();

        if (ownerPanel != null)
            ownerPanel.CloseUI();
    }

    private bool IsPrimaryPressed()
    {
        var mouse = UnityEngine.InputSystem.Mouse.current;
        if (mouse != null)
            return mouse.leftButton.isPressed;

        if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count > 0)
        {
            var phase = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[0].phase;
            return phase == UnityEngine.InputSystem.TouchPhase.Began
                || phase == UnityEngine.InputSystem.TouchPhase.Moved
                || phase == UnityEngine.InputSystem.TouchPhase.Stationary;
        }

        return false;
    }

    public void SetSeed(CropSeedData seed)
    {
        seedData = seed;
    }
}