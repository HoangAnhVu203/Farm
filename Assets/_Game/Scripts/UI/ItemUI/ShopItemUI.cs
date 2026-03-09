using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private FarmItemData itemData;

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClickItem);
    }

    void OnClickItem()
    {
        if (FarmPlacementController.Instance == null) return;
        FarmPlacementController.Instance.StartPlacingNewItem(itemData);
    }
}