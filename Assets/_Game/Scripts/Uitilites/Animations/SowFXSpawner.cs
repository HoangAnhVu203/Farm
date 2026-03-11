using UnityEngine;

public class SowFXSpawner : MonoBehaviour
{
    public static SowFXSpawner Instance { get; private set; }

    [SerializeField] private SowSeedPopupFX sowSeedPopupPrefab;
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 0.15f, 0f);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void PlaySowPopup(Vector3 worldPos, Sprite seedSprite)
    {
        if (sowSeedPopupPrefab == null || seedSprite == null) return;

        SowSeedPopupFX fx = Instantiate(sowSeedPopupPrefab, worldPos + spawnOffset, Quaternion.identity);
        fx.Play(seedSprite);
    }
}