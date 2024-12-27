using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PolygonLib
{
    public class PolygonObject : ScriptableObject
    {
        [SerializeField]
        private PolygonPath[] paths;

        public PolygonPath[] Paths { get { return paths; } }
        public int PathCount { get { return Paths.Length; } }


        void Awake ()
        {
            paths = new PolygonPath[0];
        }

        void OnDestroy ()
        {
            foreach (PolygonPath path in paths)
            {
                UnityEngine.Object.Destroy(path);
            }
        }

        public void SetPaths (PolygonPath[] pths)
        {
            paths = pths;
            for (int j = 0; j < paths.Length; j++)
            {
                if (j == 0 && paths[j].Type == PolygonPathType.Hole)
                {
                    Debug.LogError("ERROR PO A");
                }
                else if (j != 0 && paths[j].Type == PolygonPathType.Outside)
                {
                    Debug.LogError("ERROR PO B");
                }
            }
        }

        public Rect GetRectBound ()
        {
            Rect rectBound = paths[0].GetRectBound();

            for (int i = 1; i < paths.Length; i++)
            {
                Rect pathRectBound = paths[i].GetRectBound();

                if (pathRectBound.xMin < rectBound.xMin)
                {
                    rectBound.xMin = pathRectBound.xMin;
                }

                if (pathRectBound.yMin < rectBound.yMin)
                {
                    rectBound.yMin = pathRectBound.yMin;
                }

                if (pathRectBound.xMax > rectBound.xMax)
                {
                    rectBound.xMax = pathRectBound.xMax;
                }

                if (pathRectBound.yMax > rectBound.yMax)
                {
                    rectBound.yMax = pathRectBound.yMax;
                }
            }

            return rectBound;
        }

        public bool GetConnectionPointsFromPointInPath (Vector2 point, out Vector2 previousPoint, out Vector2 nextPoint, out int pathIndex)
        {
            for (int i = 0; i < paths.Length; i++)
            {
                if (paths[i].GetConnectionPointsFromPointInPath(point, out previousPoint, out nextPoint))
                {
                    pathIndex = i;
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
            for (int i = 0; i < paths.Length; i++)
            {
                if (paths[i].ChangeVertice(vertice, newVertice))
                {
                    return true;
                }
            }

            return false;
        }

        public bool AddVertice (Vector2 previousVertice, Vector2 newVertice)
        {
            for (int i = 0; i < paths.Length; i++)
            {
                if (paths[i].AddVertice(previousVertice, newVertice))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsSegmentCrossPath (Vector2Int s1, Vector2Int s2)
        {
            for (int i = 0; i < paths.Length; i++)
            {
                if (paths[i].IsSegmentCrossPath(s1, s2))
                {
                    return true;
                }
            }

            return false;
        }
    }
}