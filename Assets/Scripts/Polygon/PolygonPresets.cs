using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PolygonLib;

public class PolygonPresets
{
    private List<PolygonGroup> presets;

    private readonly int squareSize;
    private readonly int circleEdgesCount;
    private readonly int polygonMultiplier;

    public PolygonPresets (int _squareSize, int _circleEdgesCount, int _polygonMultiplier)
    {
        presets = new List<PolygonGroup>();

        squareSize        = _squareSize;
        circleEdgesCount  = _circleEdgesCount;
        polygonMultiplier = _polygonMultiplier;

        CreateSquare();
        CreateTriangles();
        CreateArcs();
    }

    public void OnDestroy ()
    {
        for (int i = 0; i < presets.Count; i++)
        {
            Object.Destroy(presets[i]);
        }

        presets.Clear();
    }

    public PolygonGroup GetPolygonPreset (int polyForm, int polyRotation, Vector2Int polyPosition, Vector2Int polySize)
    {
        PolygonGroup polygonPreset = PolyMath.CreateEmptyPolygonGroup();

        if (polyForm == 0)
        {
            //polygonPreset = presets[0];
            polygonPreset = PolyMath.CreatePolygonGroupFromRect(new RectInt(polyPosition, polySize), polygonMultiplier);
        }
        else
        {
            if (polyForm == 1)
            {
                if (polyRotation < 4 && polyRotation >= 0)
                {
                    polygonPreset = presets[1 + polyRotation];
                }
            }
            else if (polyForm == 2)
            {
                if (polyRotation < 4 && polyRotation >= 0)
                {
                    polygonPreset = presets[5 + polyRotation];
                }
            }
            else if (polyForm == 3)
            {
                if (polyRotation < 4 && polyRotation >= 0)
                {
                    polygonPreset = presets[9 + polyRotation];
                }
            }

            polygonPreset = PolyMath.ClonePolygonGroup(polygonPreset);

            PolyMath.MovePolygonGroup(polygonPreset, new Vector2(polyPosition.x, polyPosition.y) * polygonMultiplier);
        }

        return polygonPreset;
    }

    private void CreateSquare ()
    {
        RectInt squareRect = new RectInt(Vector2Int.zero, Vector2Int.one);

        presets.Add(PolyMath.CreatePolygonGroupFromRect(squareRect, squareSize * polygonMultiplier));
    }

    private void CreateTriangles ()
    {
        Vector2Int v1 = Vector2Int.zero;
        Vector2Int v2 = Vector2Int.up;
        Vector2Int v3 = Vector2Int.one;

        presets.Add(PolyMath.CreatePolygonGroupFromTriangle(v1, v2, v3, squareSize * polygonMultiplier));

        v1 = Vector2Int.right;

        presets.Add(PolyMath.CreatePolygonGroupFromTriangle(v1, v2, v3, squareSize * polygonMultiplier));

        v2 = Vector2Int.zero;

        presets.Add(PolyMath.CreatePolygonGroupFromTriangle(v1, v2, v3, squareSize * polygonMultiplier));

        v3 = Vector2Int.up;

        presets.Add(PolyMath.CreatePolygonGroupFromTriangle(v1, v2, v3, squareSize * polygonMultiplier));
    }

    private void CreateArcs ()
    {
        int arcEdgesCount   = circleEdgesCount / 4;
        float arcEdgesAngle = 90.0f / arcEdgesCount;
        int radius          = squareSize * polygonMultiplier;

        List<Vector2Int> arcPoints = new List<Vector2Int>();
        List<Vector2Int> arcNegativePoints = new List<Vector2Int>();

        arcPoints.Add(Vector2Int.zero);
        arcNegativePoints.Add(Vector2Int.one * radius);

        for (int i = 0; i <= arcEdgesCount; i++)
        {
            float angle = i * arcEdgesAngle;

            Vector2 point = new Vector2(Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Sin(Mathf.Deg2Rad * angle)) * radius;

            Vector2Int pointRounded = new Vector2Int(Mathf.RoundToInt(point.x), Mathf.RoundToInt(point.y));

            //Debug.Log(pointRounded);

            arcPoints.Add(pointRounded);
            arcNegativePoints.Add(pointRounded);
        }

        arcPoints.Reverse();

        Vector2Int pivotPoint = new Vector2Int(radius, radius) / 2;

        presets.Add(PolyMath.CreatePolygonGroupFromConventionalVerticeList(RotateList(pivotPoint, 270.0f, arcPoints)));
        presets.Add(PolyMath.CreatePolygonGroupFromConventionalVerticeList(RotateList(pivotPoint, 180.0f, arcPoints)));
        presets.Add(PolyMath.CreatePolygonGroupFromConventionalVerticeList(RotateList(pivotPoint, 90.0f, arcPoints)));
        presets.Add(PolyMath.CreatePolygonGroupFromConventionalVerticeList(RotateList(pivotPoint, 0.0f, arcPoints)));

        presets.Add(PolyMath.CreatePolygonGroupFromConventionalVerticeList(RotateList(pivotPoint, 270.0f, arcNegativePoints)));
        presets.Add(PolyMath.CreatePolygonGroupFromConventionalVerticeList(RotateList(pivotPoint, 180.0f, arcNegativePoints)));
        presets.Add(PolyMath.CreatePolygonGroupFromConventionalVerticeList(RotateList(pivotPoint, 90.0f, arcNegativePoints)));
        presets.Add(PolyMath.CreatePolygonGroupFromConventionalVerticeList(RotateList(pivotPoint, 0.0f, arcNegativePoints)));
    }

    private List<Vector2Int> RotateList (Vector2Int pivotPoint, float angleDegrees, List<Vector2Int> points)
    {
        if (angleDegrees == 0.0f)
        {
            return new List<Vector2Int>(points);
        }

        List<Vector2Int> pointsRotated = new List<Vector2Int>();

        float s = Mathf.Sin(Mathf.Deg2Rad * angleDegrees);
        float c = Mathf.Cos(Mathf.Deg2Rad * angleDegrees);

        for (int i = 0; i < points.Count; i++)
        {
            Vector2Int pointTranslated = points[i] - pivotPoint;

            Vector2 pointRotated = Vector2Int.zero;
            pointRotated.x = (pointTranslated.x * c) - (pointTranslated.y * s);
            pointRotated.y = (pointTranslated.x * s) + (pointTranslated.y * c);

            // translate point back
            pointRotated = pointRotated + pivotPoint;

            pointsRotated.Add(new Vector2Int(Mathf.RoundToInt(pointRotated.x), Mathf.RoundToInt(pointRotated.y)));
        }

        return pointsRotated;
    }
}