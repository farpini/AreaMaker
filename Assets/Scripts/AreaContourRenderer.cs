using PolygonLib;
using System.Collections.Generic;
using UnityEngine;

public class AreaContourRenderer : AreaRenderer
{
    protected int contourWidth = 0;

    public override void SetWidth (float widthValue)
    {
        int width = (int)widthValue;

        if (contourWidth != width)
        {
            SetPolygon(currentPolygonArea, currentPrecision);
        }

        contourWidth = width;
    }

    public override void SetPolygon (PolygonGroup polygonGroup, float precision)
    {
        currentPolygonArea = polygonGroup;
        currentPrecision = precision;

        Mesh mesh = new();

        if (currentPolygonArea != null && currentPolygonArea.PolygonCount > 0)
        {
            var contour = PolyMath.CreatePolygonGroupBorder(currentPolygonArea, false, contourWidth);

            int[] triangles = PolyMath.Triangulate(contour, 0f, (1f / precision), out List<Vector3> vertices, out List<Vector3> uvs);

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetUVs(0, uvs);
        }

        meshFilter.mesh = mesh;
    }
}
