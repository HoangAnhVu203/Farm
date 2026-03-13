using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FarmWorldRestoreBootstrap : MonoBehaviour
{
    [SerializeField] private Tilemap groundTilemap;

    private IEnumerator Start()
    {
        yield return null;

        Debug.Log("[WORLD RESTORE] Start");

        PreplacedFarmItemRegistry.Instance?.Rebuild();
        FarmSaveManager.Instance?.RestorePreplacedItems(groundTilemap);
    }
}