using System.Collections.Generic;
using UnityEngine;

public static class PolygonService
{
    public static bool IsPointInPolygon(Vector2 point, List<Vector2> polygonPoints)
    {
        if (polygonPoints == null || polygonPoints.Count < 3)
            return false;

        int intersectCount = 0;

        for (int i = 0; i < polygonPoints.Count; i++)
        {
            Vector2 firstPoint = polygonPoints[i];
            Vector2 secondPoint = polygonPoints[(i + 1) % polygonPoints.Count];

            if ((firstPoint.y > point.y) != (secondPoint.y > point.y))
            {
                float intersectPointX =
                    firstPoint.x +
                    (point.y - firstPoint.y) *
                    (secondPoint.x - firstPoint.x) /
                    (secondPoint.y - firstPoint.y);

                if (intersectPointX > point.x)
                {
                    intersectCount++;
                }
            }
        }

        return (intersectCount % 2) == 1;
    }

    public static Vector2 GetRandomPositionInPolygon(List<Vector2> polygon)
    {
        if (polygon == null || polygon.Count < 3)
        {
            Debug.LogWarning("Polygon không hợp lệ.");
            return Vector2.zero;
        }

        Vector2 min = polygon[0];
        Vector2 max = polygon[0];

        foreach (Vector2 p in polygon)
        {
            min = Vector2.Min(min, p);
            max = Vector2.Max(max, p);
        }

        for (int i = 0; i < 100; i++)
        {
            Vector2 randomPoint = new Vector2(
                Random.Range(min.x, max.x),
                Random.Range(min.y, max.y)
            );

            if (IsPointInPolygon(randomPoint, polygon))
                return randomPoint;
        }

        Debug.LogWarning("Không tìm được vị trí hợp lệ trong polygon.");
        return Vector2.zero;
    }
}