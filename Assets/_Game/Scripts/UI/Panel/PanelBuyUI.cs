using UnityEngine;
using UnityEngine.UI;

public class PanelBuyUI : UICanvas
{
    [SerializeField] private RectTransform root;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Vector2 screenOffset = new Vector2(0f, 120f);

    private Camera cam;
    private Transform target;

    public void Setup(Camera cameraRef, Transform followTarget, System.Action onConfirm, System.Action onCancel)
    {
        cam = cameraRef;
        target = followTarget;

        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(() => onConfirm?.Invoke());
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(() => onCancel?.Invoke());
        }

        gameObject.SetActive(true);
        RefreshPosition();
    }

    public void SetTarget(Transform followTarget)
    {
        target = followTarget;
        RefreshPosition();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        target = null;
    }

    private void LateUpdate()
    {
        RefreshPosition();
    }

    void RefreshPosition()
    {
        if (root == null || cam == null || target == null) return;

        Vector3 screenPos = cam.WorldToScreenPoint(target.position);
        root.position = screenPos + (Vector3)screenOffset;
    }
}