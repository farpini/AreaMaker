using UnityEngine;
using PolygonLib;
using System.Collections.Generic;
using ClipperLib;

public class Area : MonoBehaviour
{
    [SerializeField] private AreaFillRenderer fill;
    [SerializeField] private AreaContourRenderer contour;

    public AreaFillRenderer FillRenderer => fill;
    public AreaContourRenderer ContourRenderer => contour;

    private PolygonGroup areaRegion;


    private void Awake ()
    {
        areaRegion = PolyMath.CreateEmptyPolygonGroup();
    }

    public void Union (List<Vector2Int> areaVertices, float precision)
    {
        var pathOrientation = PolyMath.PathOrientation(areaVertices);

        if (pathOrientation == PathOrientation.Clockwise)
        {
            areaVertices.Reverse();
        }

        var polygon = PolyMath.CreatePolygonGroupFromConventionalVerticeList(areaVertices);
        areaRegion = PolyMath.Union(areaRegion, polygon);

        fill.SetPolygon(areaRegion, precision);
        contour.SetPolygon(areaRegion, precision);
    }

    public void Subtract (List<Vector2Int> areaVertices, float precision)
    {
        var pathOrientation = PolyMath.PathOrientation(areaVertices);

        if (pathOrientation == PathOrientation.Clockwise)
        {
            areaVertices.Reverse();
        }

        var polygon = PolyMath.CreatePolygonGroupFromConventionalVerticeList(areaVertices);
        areaRegion = PolyMath.Difference(polygon, areaRegion);

        fill.SetPolygon(areaRegion, precision);
        contour.SetPolygon(areaRegion, precision);
    }
}