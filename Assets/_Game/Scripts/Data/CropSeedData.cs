using UnityEngine;

[CreateAssetMenu(menuName = "Farm/Crop Seed Data")]
public class CropSeedData : ScriptableObject
{
    public string id;
    public string cropName;
    public float growDurationSeconds = 30f;

    [Header("Stage Sprites")]
    public Sprite stage1Sprite; // mới gieo
    public Sprite stage2Sprite; // nửa thời gian
    public Sprite stage3Sprite; // hoàn thành
}