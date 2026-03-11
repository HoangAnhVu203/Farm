using UnityEngine;

public class HarvestFXSpawner : MonoBehaviour
{
    public static HarvestFXSpawner Instance { get; private set; }

    [SerializeField] private HarvestPopupFX harvestPopupPrefab;
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 0.2f, 0f);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void PlayHarvestPopup(Vector3 worldPos, Sprite icon)
    {
        if (harvestPopupPrefab == null || icon == null) return;

        HarvestPopupFX fx = Instantiate(harvestPopupPrefab, worldPos + spawnOffset, Quaternion.identity);
        fx.Play(icon);
    }
}