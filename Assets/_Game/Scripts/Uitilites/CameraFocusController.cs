using UnityEngine;

public class CameraFocusController : MonoBehaviour
{
    public static CameraFocusController Instance { get; private set; }

    [Header("Refs")]
    [SerializeField] private Camera cam;

    [Header("Move")]
    [SerializeField] private float moveSmoothTime = 0.22f;
    [SerializeField] private float stopDistance = 0.03f;

    [Header("Zoom")]
    [SerializeField] private bool enableZoomOnFocus = true;
    [SerializeField] private float focusZoomSize = 4.2f;
    [SerializeField] private float zoomSmoothTime = 0.2f;
    [SerializeField] private float zoomStopThreshold = 0.03f;

    [Header("Optional")]
    [SerializeField] private bool restoreOriginalZoomAfterFocus = false;
    [SerializeField] private float restoreDelay = 0.15f;

    private Vector3 moveVelocity;
    private float zoomVelocity;

    private Vector3 targetCamPos;
    private float targetZoomSize;

    private float originalZoomSize;
    private bool hasOriginalZoom;

    private bool isFocusing;
    private bool isRestoringZoom;
    private float restoreTimer;

    public bool IsBusy => isFocusing || isRestoringZoom;
    public bool IsFocusing => isFocusing;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (cam == null) cam = Camera.main;
    }

    private void Start()
    {
        if (cam != null && cam.orthographic)
        {
            originalZoomSize = cam.orthographicSize;
            hasOriginalZoom = true;
        }
    }

    public void FocusTo(Transform target)
    {
        if (target == null || cam == null) return;
        FocusToPosition(target.position);
    }

    public void FocusToPosition(Vector3 worldPos)
    {
        if (cam == null) return;

        targetCamPos = new Vector3(worldPos.x, worldPos.y, cam.transform.position.z);

        if (cam.orthographic)
        {
            if (!hasOriginalZoom)
            {
                originalZoomSize = cam.orthographicSize;
                hasOriginalZoom = true;
            }

            targetZoomSize = enableZoomOnFocus
                ? Mathf.Min(cam.orthographicSize, focusZoomSize)
                : cam.orthographicSize;
        }

        isFocusing = true;
        isRestoringZoom = false;
        restoreTimer = 0f;
    }

    public void CancelFocus()
    {
        isFocusing = false;
        isRestoringZoom = false;
        restoreTimer = 0f;
    }

    private void LateUpdate()
    {
        if (cam == null) return;

        if (isFocusing)
        {
            UpdateFocus();
            return;
        }

        if (isRestoringZoom)
        {
            UpdateRestoreZoom();
        }
    }

    private void UpdateFocus()
    {
        cam.transform.position = Vector3.SmoothDamp(
            cam.transform.position,
            targetCamPos,
            ref moveVelocity,
            moveSmoothTime
        );

        bool moveDone = Vector3.Distance(cam.transform.position, targetCamPos) <= stopDistance;

        bool zoomDone = true;

        if (cam.orthographic)
        {
            cam.orthographicSize = Mathf.SmoothDamp(
                cam.orthographicSize,
                targetZoomSize,
                ref zoomVelocity,
                zoomSmoothTime
            );

            zoomDone = Mathf.Abs(cam.orthographicSize - targetZoomSize) <= zoomStopThreshold;
        }

        if (moveDone && zoomDone)
        {
            cam.transform.position = targetCamPos;

            if (cam.orthographic)
                cam.orthographicSize = targetZoomSize;

            isFocusing = false;

            if (restoreOriginalZoomAfterFocus && cam.orthographic && hasOriginalZoom)
            {
                isRestoringZoom = true;
                restoreTimer = restoreDelay;
            }
        }
    }

    private void UpdateRestoreZoom()
    {
        if (!cam.orthographic || !hasOriginalZoom)
        {
            isRestoringZoom = false;
            return;
        }

        if (restoreTimer > 0f)
        {
            restoreTimer -= Time.deltaTime;
            return;
        }

        cam.orthographicSize = Mathf.SmoothDamp(
            cam.orthographicSize,
            originalZoomSize,
            ref zoomVelocity,
            zoomSmoothTime
        );

        if (Mathf.Abs(cam.orthographicSize - originalZoomSize) <= zoomStopThreshold)
        {
            cam.orthographicSize = originalZoomSize;
            isRestoringZoom = false;
        }
    }
}