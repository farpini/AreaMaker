using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PolyComp
{
    using ClipperLib;
    using LibTessDotNet;
    using PolygonMeshLib;

    public static class Poly2Mesh
    {
        public const int MeshRendererMultiplier = 16;

        /*
        public static int[] Triangulate (List<SpritePathData> paths, Vector2Int origin, out Vector3[] vertices, out List<Vector3> uvs)
        {
            // transformation function from Vector2Int to LibTess contour vertex
            Func<Vector2Int, ContourVertex> Vector2IntToContourVertex = (p) => new ContourVertex() { Position = new Vec3 { X = p.x, Y = p.y, Z = 0 } };

            // create the tess class for triangulation
            Tess tess = new Tess();
            tess.NoEmptyPolygons = true;

            // get the contours from polygons
            for (int i = 0; i < paths.Count; i++)
            {
                // Add a new countor. Holes are automatically generated.
                var contourPointsContourVertex = paths[i].vertices.Select(Vector2IntToContourVertex).ToArray();
                tess.AddContour(contourPointsContourVertex, paths[i].isHole ? ContourOrientation.Clockwise : ContourOrientation.CounterClockwise);
            }

            // do the tessellation
            tess.Tessellate(WindingRule.EvenOdd, ElementType.Polygons, 3);

            // extract the vertices and triangles
            vertices = new Vector3[tess.Vertices.Length];
            uvs      = new List<Vector3>();

            for (int i = 0; i < vertices.Length; i++)
            {
                Vec3 v = tess.Vertices[i].Position;
                vertices[i] = new Vector2(v.X, v.Y) - origin;
                uvs.Add(new Vector3(v.X, v.Y, 0.0f));
            }

            //return PolygonComp.ConvertTypeArrayIntToUshort(tess.Elements);
            return tess.Elements;
        }
        */

        public static int[] Triangulate (List<Vector2[]> poly, out List<Vector3> vertices, out List<Vector3> uvs)
        {
            // transformation function from ClipperLip Point to LibTess contour vertex
            Func<Vector2, ContourVertex> xfToContourVertex = (p) => new ContourVertex() { Position = new Vec3 { X = p.x, Y = p.y, Z = 0 } };

            // create the tess class for triangulation
            Tess tess = new Tess();
            tess.NoEmptyPolygons = true;

            // get the contours from polygons
            for (int i = 0; i < poly.Count; i++)
            {
                var contourPointsContourVertex = poly[i].Select(xfToContourVertex).ToArray();
                tess.AddContour(contourPointsContourVertex, (i != 0) ? ContourOrientation.Clockwise : ContourOrientation.CounterClockwise);
            }

            // do the tessellation
            tess.Tessellate(WindingRule.EvenOdd, ElementType.Polygons, 3);

            // extract the vertices and triangles
            Vector2Int pointInt = Vector2Int.zero;
            vertices = new List<Vector3>();
            uvs = new List<Vector3>();

            for (int i = 0; i < tess.Vertices.Length; i++)
            {
                Vec3 v = tess.Vertices[i].Position;
                vertices.Add(new Vector3(v.X, v.Y, 0.0f));
                uvs.Add(new Vector3(v.X, v.Y, 0.0f));
            }

            return tess.Elements;
        }

        public static int[] Triangulate (List<PolygonMesh> polygons, int polygonZlayer, ushort uvSize, Vector2 uvOffset, out List<Vector3> vertices, out List<Vector3> uvs)
        {
            // transformation function from ClipperLip Point to LibTess contour vertex
            Func<IntPoint, ContourVertex> xfToContourVertex = (p) => new ContourVertex() { Position = new Vec3 { X = p.X, Y = p.Y, Z = 0 } };

            // create the tess class for triangulation
            Tess tess = new Tess();
            tess.NoEmptyPolygons = true;

            // get the contours from polygons
            for (int i = 0; i < polygons.Count; i++)
            {
                List<List<IntPoint>> polygonContours = polygons[i].GetBoundariesPoints();

                for (int j = 0; j < polygonContours.Count; j++)
                {
                    // Add a new countor. Holes are automatically generated.
                    var contourPointsContourVertex = polygonContours[j].Select(xfToContourVertex).ToArray();
                    //Debug.Log("Orientation: " + tess.GetContourOrientation(contourPointsContourVertex));
                    //Debug.Log("Orientation: " + PolygonComp.GetBoundaryOrientation(polygonContours[j]));
                    tess.AddContour(contourPointsContourVertex, (j != 0) ? ContourOrientation.Clockwise : ContourOrientation.CounterClockwise);
                }
            }

            // do the tessellation
            tess.Tessellate(WindingRule.EvenOdd, ElementType.Polygons, 3);

            // extract the vertices and triangles
            Vector2Int pointInt = Vector2Int.zero;
            vertices            = new List<Vector3>();
            uvs                 = new List<Vector3>();
            
            for (int i = 0; i < tess.Vertices.Length; i++)
            {
                Vec3 v = tess.Vertices[i].Position;
                vertices.Add(new Vector3(v.X, v.Y, 0.0f));
                uvs.Add(new Vector3(v.X / uvSize + uvOffset.x, v.Y / uvSize + uvOffset.y, polygonZlayer));
            }

            return tess.Elements;
        }

        public static int[] Triangulate2 (List<PolygonMesh> polygons, int layerId, ushort uvSize, Vector2 uvOffset, out List<Vector3> vertices, out List<Vector3> uvs)
        {
            // create the tess class for triangulation
            Tess tess = new Tess();
            tess.NoEmptyPolygons = true;

            // get the contours from polygons
            for (int j = 0; j < polygons.Count; j++)
            {
                List<List<IntPoint>> polygonContours = polygons[j].GetBoundariesPoints();

                for (int k = 0; k < polygonContours.Count; k++)
                {
                    List<IntPoint> polygonContour = polygonContours[k];
                    ContourVertex[] contourVertex = new ContourVertex[polygonContour.Count];
                    for (int m = 0; m < contourVertex.Length; m++)
                    {
                        IntPoint p = polygonContour[m];
                        contourVertex[m] = new ContourVertex() { Position = new Vec3 { X = p.X, Y = p.Y, Z = layerId } };
                    }

                    tess.AddContour(contourVertex, (k != 0) ? ContourOrientation.Clockwise : ContourOrientation.CounterClockwise);
                }
            }

            // do the tessellation
            tess.Tessellate(WindingRule.EvenOdd, ElementType.Polygons, 3);

            // extract the vertices and triangles
            Vector2Int pointInt = Vector2Int.zero;
            vertices = new List<Vector3>();
            uvs = new List<Vector3>();

            for (int i = 0; i < tess.Vertices.Length; i++)
            {
                Vec3 v = tess.Vertices[i].Position;
                vertices.Add(new Vector3(v.X, v.Y, 0.0f));
                uvs.Add(new Vector3(v.X / uvSize + uvOffset.x, v.Y / uvSize + uvOffset.y, v.Z));

            }

            //Print(vertices.ToArray());
            //Print(tess.Elements);

            return tess.Elements;
        }

        private static void Print (int[] arr)
        {
            string a = "Tris: ";
            for (int i = 0; i < arr.Length; i++)
            {
                a += arr[i] + " ";
            }
            Debug.Log(a);
        }

        private static void Print (Vector3[] arr)
        {
            string a = "Verts: ";
            for (int i = 0; i < arr.Length; i++)
            {
                a += " [" + arr[i].x + "][" + arr[i].y + "] ,";
            }
            Debug.Log(a);
        }
    }
}
