using UnityEngine;

public class IsoSorting : MonoBehaviour
{
    [SerializeField] private Transform foot;
    [SerializeField] private SpriteRenderer sr;

    void LateUpdate()
    {
        if (sr == null || foot == null) return;

        sr.sortingOrder = Mathf.RoundToInt(-foot.position.y * 100);
    }
}