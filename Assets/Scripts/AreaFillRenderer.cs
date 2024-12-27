using PolygonLib;
using System.Collections.Generic;
using UnityEngine;

public class AreaFillRenderer : AreaRenderer
{
    private float currentWidth;
    private float currentSpeed;

    public override void SetPolygon (PolygonGroup polygonGroup, float precision)
    {
        currentPolygonArea = polygonGroup;
        currentPrecision = precision;

        Mesh mesh = new();

        if (currentPolygonArea != null && currentPolygonArea.PolygonCount > 0)
        {
            int[] triangles = PolyMath.Triangulate(currentPolygonArea, 0f, (1f / precision), out List<Vector3> vertices, out List<Vector3> uvs);

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetUVs(0, uvs);
        }

        meshFilter.mesh = mesh;
    }

    public override void SetWidth (float widthValue)
    {
        currentWidth = widthValue;
        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        meshRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat("_Width", currentWidth);
        meshRenderer.SetPropertyBlock(propertyBlock);
    }

    public override void SetSpeed (float speedValue)
    {
        currentSpeed = speedValue;
        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        meshRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat("_Speed", currentSpeed);
        meshRenderer.SetPropertyBlock(propertyBlock);
    }
}
