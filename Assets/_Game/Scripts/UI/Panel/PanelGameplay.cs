using UnityEngine;

public class PanelGameplay : UICanvas
{
    public void OpenShop()
    {
        UIManager.Instance.OpenUI<PanelShop>();
    }
}
