using UnityEngine;

public class AnimalPen : MonoBehaviour
{
    [SerializeField] private AnimalType acceptType = AnimalType.None;

    public AnimalType AcceptType => acceptType;

    public bool CanAccept(FarmItemData itemData)
    {
        if (itemData == null) return false;
        if (!itemData.isAnimal) return false;
        return itemData.animalType == acceptType;
    }
}