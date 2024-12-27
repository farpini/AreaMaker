using System.Collections.Generic;
using UnityEngine;
using System;

namespace PolygonLib
{
    public class PolygonLayer
    {
        public readonly int    layerId;
        public readonly string layerName;
        public readonly byte   layerType;
        public readonly ushort layerSize;

        public Vector2      Position { get { return Polygon != null ? Polygon.PolygonBounds.position : Vector2.zero; } }
        public Rect         Bounds   { get { return Polygon != null ? Polygon.PolygonBounds : new Rect(Vector2.zero, Vector2.zero); } }

        public PolygonGroup Polygon  { get; private set; } 
        public Mesh         Mesh     { get; private set; }
        public bool         Valid    { get { return Mesh.vertices.Length >= 3; } }


        public PolygonLayer (int id, string name, byte type, ushort size)
        {
            layerId = id;
            layerName = name;
            layerType = type;
            layerSize = size;

            ClearPolygon();
        }

        public void Destroy()
        {
            UnityEngine.Object.Destroy(Mesh);
            UnityEngine.Object.Destroy(Polygon);
        }

        public bool CheckPolygon (LayerCheck layerCheck, PolygonGroup otherPolygon)
        {
            if (layerCheck == LayerCheck.Overlap)
            {
                return PolyMath.IsPolygonIntersectPolygon(Polygon, otherPolygon);
            }
            else if (layerCheck == LayerCheck.Touch)
            {
                return PolyMath.IsPolygonTouchPolygon(Polygon, otherPolygon);
            }
 
            return false;
        }

        public void EditPolygon (PolygonEditProcedure procedure, PolygonGroup polygonEdited, Action rendererRefresh = null)
        {
            if (procedure == PolygonEditProcedure.AddAndRemove)
            {
                Debug.LogWarning("PolygonEditProcedure.AddAndRemove not implemented yet.");
                return;
            }

            if (procedure == PolygonEditProcedure.Add)
            {
                polygonEdited = PolyMath.Union(polygonEdited, Polygon);
            }
            else if (procedure == PolygonEditProcedure.Remove)
            {
                polygonEdited = PolyMath.Difference(polygonEdited, Polygon);
            }

            SetPolygons(polygonEdited);

            Triangulate();

            rendererRefresh?.Invoke();
        }

        private void Triangulate ()
        {
            Mesh = new Mesh();

            if (Polygon.PolygonCount > 0)
            {
                List<Vector3> vertices;
                List<Vector3> uvs;

                LayerProperties layerProperty = new LayerProperties(layerId);
                layerProperty.size   = layerSize;
                layerProperty.offset = Vector2.zero;

                int[] triangles = PolyMath.Triangulate(Polygon, 0.0f, layerProperty, out vertices, out uvs);

                Mesh.vertices = vertices.ToArray();
                Mesh.triangles = triangles;
                Mesh.SetUVs(0, uvs);
                
                Mesh.RecalculateNormals();
                Mesh.RecalculateBounds();
            }
        }

        private void SetPolygons (PolygonGroup poly)
        {
            Polygon = poly;
        }

        public void ClearPolygon ()
        {
            SetPolygons(PolyMath.CreateEmptyPolygonGroup());
            Mesh = new Mesh();
        }
    }

    public enum PolygonEditProcedure
    {
        Add = 0,
        Remove = 1,
        AddAndRemove = 2,
        Set = 3
    }

    public enum LayerCheck
    {
        Overlap = 0,
        Touch = 1,
    }

    public class LayerProperties : IEquatable<LayerProperties>
    {
        public int id = 0;
        public ushort size = 100;
        public Vector2 offset = Vector2.zero;

        public LayerProperties (int layerId)
        {
            id = layerId;
        }

        public bool Equals (LayerProperties other)
        {
            return other.id == id;
        }

        public override int GetHashCode ()
        {
            return 1877310944 + id.GetHashCode();
        }
    }
}