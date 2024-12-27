using System.Collections.Generic;
using UnityEngine;

namespace PolygonLib
{
    public class PolygonGroup : ScriptableObject
    {
        [SerializeField]
        private PolygonObject[] polygons;

        [SerializeField]
        private Rect bounds;

        public PolygonObject[] Polygons { get { return polygons; } }
        public int PolygonCount { get { return polygons.Length; } }
        public Rect PolygonBounds { get { return bounds; } }


        void Awake ()
        {
            SetPolygons(null, new Rect(Vector2.zero, Vector2.zero));
        }

        void OnDestroy ()
        {
            foreach (PolygonObject poly in polygons)
            {
                UnityEngine.Object.Destroy(poly);
            }
        }

        public void PrintDebug ()
        {
            //DebugUtil.PrintVector2Arr(polygons[0].Paths[0].Points, "Contour");
        }

        public void SetPolygons (PolygonObject[] polys)
        {
            SetPolygons(polys, new Rect(Vector2.zero, Vector2.zero));
        }

        public void SetPolygons (PolygonObject[] polys, Rect polyBounds)
        {
            if (polys == null)
            {
                polygons = new PolygonObject[0];
                return;
            }

            polygons = polys;
            SetBounds(polyBounds);
        }

        public void SetBounds (Rect polyBounds)
        {
            bounds = polyBounds;
        }

        public PolygonPath[] GetPolygonsPaths (out int totalVerticeCount)
        {
            List<PolygonPath> paths = new List<PolygonPath>();

            totalVerticeCount = 0;

            for (int i = 0; i < polygons.Length; i++)
            {
                PolygonPath[] pathsArr = polygons[i].Paths;
                for (int j = 0; j < pathsArr.Length; j++)
                {
                    totalVerticeCount += pathsArr[j].Count;
                    paths.Add(pathsArr[j]);
                }
            }

            return paths.ToArray();
        }

        public int GetPolygonsPathCount ()
        {
            int polyPathCount = 0;

            for (int i = 0; i < polygons.Length; i++)
            {
                polyPathCount += polygons[i].PathCount;
            }

            return polyPathCount;
        }

        public void ComputeBounds ()
        {
            if (polygons == null || polygons.Length == 0)
            {
                return;
            }

            Rect rectBound = polygons[0].GetRectBound();

            for (int i = 1; i < polygons.Length; i++)
            {
                Rect polyObjBound = polygons[i].GetRectBound();

                if (polyObjBound.xMin < rectBound.xMin)
                {
                    rectBound.xMin = polyObjBound.xMin;
                }

                if (polyObjBound.yMin < rectBound.yMin)
                {
                    rectBound.yMin = polyObjBound.yMin;
                }

                if (polyObjBound.xMax > rectBound.xMax)
                {
                    rectBound.xMax = polyObjBound.xMax;
                }

                if (polyObjBound.yMax > rectBound.yMax)
                {
                    rectBound.yMax = polyObjBound.yMax;
                }
            }

            bounds = rectBound;
        }

        public bool GetConnectionPointsFromPointInPath (Vector2 point, out Vector2 previousPoint, out Vector2 nextPoint, out int pathIndex)
        {
            for (int i = 0; i < polygons.Length; i++)
            {
                if (polygons[i].GetConnectionPointsFromPointInPath(point, out previousPoint, out nextPoint, out pathIndex))
                {
                    return true;
                }
            }

            previousPoint = Vector2.zero;
            nextPoint = Vector2.zero;
            pathIndex = -1;

            return false;
        }

        public bool ChangeVertice (Vector2 vertice, Vector2 newVertice)
        {
            for (int i = 0; i < polygons.Length; i++)
            {
                if (polygons[i].ChangeVertice(vertice, newVertice))
                {
                    return true;
                }
            }

            return false;
        }

        public bool AddVertice (Vector2 previousVertice, Vector2 newVertice)
        {
            for (int i = 0; i < polygons.Length; i++)
            {
                if (polygons[i].AddVertice(previousVertice, newVertice))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsSegmentCrossRegion (Vector2Int s1, Vector2Int s2)
        {
            for (int i = 0; i < polygons.Length; i++)
            {
                if (polygons[i].IsSegmentCrossPath(s1, s2))
                {
                    return true;
                }
            }

            return false;
        }
    }
}