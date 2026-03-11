using UnityEngine;
using UnityEngine.UI;

public class PanelHarvest : UICanvas
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseUI);
        }
    }

    public void HidePanelOnly()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
        else
            gameObject.SetActive(false);
    }

    public void CloseUI()
    {
        HarvestController.Instance?.EndHarvestHold();
        UIManager.Instance.CloseUIDirectly<PanelHarvest>();
    }
}