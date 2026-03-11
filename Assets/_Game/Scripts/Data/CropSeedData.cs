using UnityEngine;

[CreateAssetMenu(menuName = "Farm/Crop Seed Data")]
public class CropSeedData : ScriptableObject
{
    public string id;
    public string cropName;
    public float growDurationSeconds = 30f;

    [Header("Seed UI")]
    public Sprite seedIcon;

    [Header("Harvest")]
    public FarmInventoryItemData harvestItem;
    public int harvestAmount = 1;

    [Header("Stage Sprites")]
    public Sprite stage1Sprite;
    public Sprite stage2Sprite;
    public Sprite stage3Sprite;
}