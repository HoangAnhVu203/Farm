// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;

// public class SeedItemView : MonoBehaviour
// {
//     [SerializeField] private Image iconImage;
//     [SerializeField] private Text nameText;
//     [SerializeField] private Text timeText;
//     [SerializeField] private Button button;

//     private CropSeedData seedData;
//     private PanelSow owner;

//     public void Setup(CropSeedData data, PanelSow panel)
//     {
//         seedData = data;
//         owner = panel;

//         if (iconImage != null)
//             iconImage.sprite = data.stage1Sprite;

//         if (nameText != null)
//             nameText.text = data.cropName;

//         if (timeText != null)
//             timeText.text = $"{data.growDurationSeconds:0}s";
//     }

//     private void Awake()
//     {
//         if (button != null)
//         {
//             button.onClick.RemoveAllListeners();
//             button.onClick.AddListener(OnClick);
//         }
//     }

//     private void OnClick()
//     {
//         if (seedData == null || owner == null) return;
//         owner.SelectSeed(seedData);
//     }
// }