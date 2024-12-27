using UnityEngine;
using System.Collections.Generic;

namespace PolygonLib
{
    public class PolygonPath : ScriptableObject
    {
        [SerializeField]
        private Vector2[] points;

        [SerializeField]
        public PolygonPathType type;

        [SerializeField]
        private int index;

        public Vector2[] Points { get { return points; } }
        public PolygonPathType Type { get { return type; } }
        public int Index { get { return index; } }
        public int Count { get { return points.Length; } }


        void Awake ()
        {
            points = new Vector2[0];
            type = PolygonPathType.Outside;
            index = 0;
        }

        void OnDestroy ()
        {
        }

        public void SetPath (Vector2[] pts, PolygonPathType tp, int idx = 0)
        {
            type = tp;
            index = idx;
            UpdatePath(pts);
        }

        public void UpdatePath (Vector2[] pts)
        {
            points = pts;
        }

        public void ReversePath ()
        {
            List<Vector2> pointsReversed = new List<Vector2>(points);
            pointsReversed.Reverse();
            SetPath(pointsReversed.ToArray(), type == PolygonPathType.Outside ? PolygonPathType.Hole : PolygonPathType.Outside);
        }

        public Rect GetRectBound ()
        {
            if (points == null || points.Length == 0)
            {
                return Rect.zero;
            }

            Rect rectBound = new Rect(points[0], points[0]);

            for (int i = 1; i < points.Length; i++)
            {
                if (points[i].x < rectBound.xMin)
                {
                    rectBound.xMin = points[i].x;
                }

                if (points[i].y < rectBound.yMin)
                {
                    rectBound.yMin = points[i].y;
                }

                if (points[i].x > rectBound.xMax)
                {
                    rectBound.xMax = points[i].x;
                }

                if (points[i].y > rectBound.yMax)
                {
                    rectBound.yMax = points[i].y;
                }
            }

            return rectBound;
        }
            
        /*
        private bool IsPointInPrecision (Vector2 point, float precision)
        {
            Vector2 min = new Vector2(point.x - precision, point.y - precision);
            Vector2 max = new Vector2(point.x + precision, point.y + precision);
            return point.x > min.x && point.y > min.y && point.x < max.x && point.y < max.y;
        }
        */

        private Rect GetPointRect (Vector2 point, float rectSize)
        {
            Rect pointRect = new Rect(Vector2.zero, Vector2.one * rectSize);
            pointRect.center = point;
            return pointRect;
        }

        public bool GetConnectionPointsFromPointInPath(Vector2 point, out Vector2 previousPoint, out Vector2 nextPoint)
        {
            int lengthMinus = points.Length - 1;

            for (int i = 0; i < points.Length; i++)
            {
                if (points[i].Equals(point))
                {
                    previousPoint = points[i == 0 ? lengthMinus : i - 1];
                    nextPoint = points[i == lengthMinus ? 0 : i + 1];
                    return true;
                }
            }

            previousPoint = Vector2.zero;
            nextPoint = Vector2.zero;

            return false;
        }

        public bool ChangeVertice (Vector2 vertice, Vector2 newVertice)
        {
            for (int i = 0; i < points.Length; i++)
            {
                if (points[i].Equals(vertice))
                {
                    points[i] = new Vector2(newVertice.x, newVertice.y);
                    return true;
                }
            }

            return false;
        }

        public bool AddVertice (Vector2 previousVertice, Vector2 newVertice)
        {
            List<Vector2> verticeList = new List<Vector2>();

            bool previousVerticeFound = false;

            for (int i = 0; i < points.Length; i++)
            {
                verticeList.Add(points[i]);

                if (points[i].Equals(previousVertice) && !previousVerticeFound)
                {
                    previousVerticeFound = true;
                    verticeList.Add(newVertice);
                }
            }

            if (previousVerticeFound)
            {
                UpdatePath(verticeList.ToArray());
                return true;
            }

            return false;
        }

        public bool IsSegmentCrossPath (Vector2Int s1, Vector2Int s2)
        {
            Debug.LogError("Not implemented yet");
            return false;
        }
    }

    public enum PolygonPathType
    {
        Outside = 0, Hole = 1
    }
}