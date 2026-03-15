using UnityEngine;

public class PlacedAnimal : MonoBehaviour
{
    [SerializeField] private AnimalType animalType = AnimalType.None;
    [SerializeField] private AnimalPen currentPen;

    public AnimalType AnimalType => animalType;
    public AnimalPen CurrentPen => currentPen;

    public void Init(FarmItemData data, AnimalPen pen)
    {
        if (data != null)
            animalType = data.animalType;

        currentPen = pen;
    }

    public void SetPen(AnimalPen pen)
    {
        currentPen = pen;
    }
}