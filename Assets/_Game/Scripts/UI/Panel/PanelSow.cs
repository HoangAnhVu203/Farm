using UnityEngine;

public class PanelSow : UICanvas
{
    [Header("Optional Root")]
    [SerializeField] private GameObject panelRoot;

    public void HidePanelOnly()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
        else
            gameObject.SetActive(false);
    }

    public void CloseUI()
    {
        SowController.Instance?.EndSowHold();
        UIManager.Instance.CloseUIDirectly<PanelSow>();
    }
}