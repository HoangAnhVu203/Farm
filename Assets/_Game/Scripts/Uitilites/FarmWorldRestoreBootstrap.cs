using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FarmWorldRestoreBootstrap : MonoBehaviour
{
    [SerializeField] private Tilemap groundTilemap;

    private IEnumerator Start()
    {
        // Chờ đến cuối frame để đảm bảo tất cả Awake() + Start() (bao gồm
        // PreplacedFactoryMachineBootstrap.Start()) đã chạy xong trước khi restore.
        yield return new WaitForEndOfFrame();

        Debug.Log("[WORLD RESTORE] Start");

        PreplacedFarmItemRegistry.Instance?.Rebuild();
        FarmSaveManager.Instance?.RestorePreplacedItems(groundTilemap);
    }
}