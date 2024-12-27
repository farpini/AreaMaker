using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClipperLib;

namespace PolygonMeshLib
{
    public class PolygonMesh : ScriptableObject
    {
        [SerializeField]
        public PolygonBoundary Outside;

        [SerializeField]
        public List<PolygonBoundary> Holes;

        public int HolesCount { get { return Holes.Count; } }

        public Vector2Int BottomLeftPoint { get { return (Outside != null) ? Outside.BottomLeftPoint : new Vector2Int(int.MaxValue, int.MaxValue); } }

        public void Awake ()
        {
            Outside = null;
            Holes = new List<PolygonBoundary>();
        }

        public void CreateBoundaries (List<IntPoint> o, List<List<IntPoint>> hs)
        {
            Outside = ScriptableObject.CreateInstance<PolygonBoundary>();
            Outside.SetPoints(o, BoundaryType.Outside);

            Holes.Clear();
            for (int i = 0; i < hs.Count; i++)
            {
                Holes.Add(ScriptableObject.CreateInstance<PolygonBoundary>());
                Holes[i].SetPoints(hs[i], BoundaryType.Hole);
            }
        }

        public void CreateBoundaries (List<IntPoint> o, List<IntPoint> h)
        {
            Outside = ScriptableObject.CreateInstance<PolygonBoundary>();
            Outside.SetPoints(o, BoundaryType.Outside);

            Holes.Clear();
            Holes.Add(ScriptableObject.CreateInstance<PolygonBoundary>());
            Holes[0].SetPoints(h, BoundaryType.Hole);
        }

        public void CreateBoundaries (List<Vector2Int> o, List<List<Vector2Int>> hs)
        {
            Outside = ScriptableObject.CreateInstance<PolygonBoundary>();
            Outside.SetPoints(o, BoundaryType.Outside);

            Holes.Clear();
            for (int i = 0; i < hs.Count; i++)
            {
                Holes.Add(ScriptableObject.CreateInstance<PolygonBoundary>());
                Holes[i].SetPoints(hs[i], BoundaryType.Hole);
            }
        }

        public List<List<IntPoint>> GetBoundariesPoints ()
        {
            List<List<IntPoint>> boundaries = new List<List<IntPoint>>();
            boundaries.Add(Outside.GetPoints());
            for (int i = 0; i < Holes.Count; i++)
            {
                boundaries.Add(Holes[i].GetPoints());
            }

            return boundaries;
        }

        public List<IntPoint> GetOutsidePoints ()
        {
            return new List<IntPoint>(Outside.GetPoints());
        }

        public List<List<IntPoint>> GetHolesPoints ()
        {
            List<List<IntPoint>> holesPoints = new List<List<IntPoint>>();
            for (int i = 0; i < Holes.Count; i++)
            {
                holesPoints.Add(Holes[i].GetPoints());
            }

            return holesPoints;
        }
    }

    public enum BoundaryType { Outside, Hole };
    public enum BorderZone { Extern, Intern };

    public class PolygonBoundary : ScriptableObject
    {
        [SerializeField]
        private BoundaryType Type;

        [SerializeField]
        private List<IntPoint> Points;

        private int BottomLeftX;
        private int BottomLeftY;

        public Vector2Int BottomLeftPoint { get { return new Vector2Int(BottomLeftX, BottomLeftY); } }

        public void Awake ()
        {
            Points = new List<IntPoint>();
            BottomLeftX = int.MaxValue;
            BottomLeftY = int.MaxValue;
        }

        public void SetPoints (List<IntPoint> pts, BoundaryType t)
        {
            BottomLeftX = int.MaxValue;
            BottomLeftY = int.MaxValue;

            Points.Clear();
            for (int i = 0; i < pts.Count; i++)
            {
                IntPoint p = new IntPoint(pts[i]);
                Points.Add(pts[i]);

                if (p.X < BottomLeftX)
                {
                    BottomLeftX = (int)p.X;
                }

                if (p.Y < BottomLeftY)
                {
                    BottomLeftY = (int)p.Y;
                }
            }

            Type = t;
        }

        public void SetPoints (List<Vector2Int> pts, BoundaryType t)
        {
            BottomLeftX = int.MaxValue;
            BottomLeftY = int.MaxValue;

            Points.Clear();
            for (int i = 0; i < pts.Count; i++)
            {
                IntPoint p = new IntPoint(pts[i]);
                Points.Add(p);

                if (p.X < BottomLeftX)
                {
                    BottomLeftX = (int)p.X;
                }

                if (p.Y < BottomLeftY)
                {
                    BottomLeftY = (int)p.Y;
                }
            }

            Type = t;
        }

        public List<IntPoint> GetPoints ()
        {
            return Points;
        }

        public List<IntPoint> GenerateBorder (BorderZone borderZone, ushort size)
        {
            List<IntPoint> boundary = new List<IntPoint>();

            if (Points.Count < 4)
            {
                Debug.LogError("class PolygonBoundary GenerateBorder : boundary has less then 3 points.");
                return null;
            }

            int sizeValue = size;
            if (borderZone == BorderZone.Extern)
            {
                sizeValue = (Type == BoundaryType.Outside) ? -sizeValue : sizeValue;
            }
            else
            {
                sizeValue = (Type == BoundaryType.Outside) ? sizeValue : -sizeValue;
            }

            for (int i = 0; i < Points.Count; i++)
            {
                int previousIdx = i - 1;
                int nextIdx = i + 1;
                if (previousIdx < 0)
                {
                    previousIdx = Points.Count - 1;
                }
                if (nextIdx == Points.Count)
                {
                    nextIdx = 0;
                }

                IntPoint previousPoint = Points[previousIdx];
                IntPoint currentPoint = Points[i];
                IntPoint nextPoint = Points[nextIdx];

                IntPoint newPoint = new IntPoint(currentPoint);

                if (previousPoint.Y == currentPoint.Y)
                {
                    if (previousPoint.X > currentPoint.X)
                    {
                        if (nextPoint.Y > currentPoint.Y)
                        {
                            newPoint.X -= sizeValue;
                            newPoint.Y -= sizeValue;
                        }
                        else
                        {
                            newPoint.X += sizeValue;
                            newPoint.Y -= sizeValue;
                        }
                    }
                    else
                    {
                        if (nextPoint.Y > currentPoint.Y)
                        {
                            newPoint.X -= sizeValue;
                            newPoint.Y += sizeValue;
                        }
                        else
                        {
                            newPoint.X += sizeValue;
                            newPoint.Y += sizeValue;
                        }
                    }
                }
                else
                {
                    if (previousPoint.Y > currentPoint.Y)
                    {
                        if (nextPoint.X > currentPoint.X)
                        {
                            newPoint.X += sizeValue;
                            newPoint.Y += sizeValue;
                        }
                        else
                        {
                            newPoint.X += sizeValue;
                            newPoint.Y -= sizeValue;
                        }
                    }
                    else
                    {
                        if (nextPoint.X > currentPoint.X)
                        {
                            newPoint.X -= sizeValue;
                            newPoint.Y += sizeValue;
                        }
                        else
                        {
                            newPoint.X -= sizeValue;
                            newPoint.Y -= sizeValue;
                        }
                    }
                }

                boundary.Add(newPoint);
            }

            return boundary;
        }

        public RectInt GetBounds ()
        {
            Vector2Int rectMin = new Vector2Int(int.MaxValue, int.MaxValue);
            Vector2Int rectMax = new Vector2Int(int.MinValue, int.MinValue);

            for (int i = 0; i < Points.Count; i++)
            {
                IntPoint p = Points[i];
                if (rectMin.x > p.X)
                {
                    rectMin.x = (int)p.X;
                }
                if (rectMin.y > p.Y)
                {
                    rectMin.y = (int)p.Y;
                }
                if (rectMax.x < p.X)
                {
                    rectMax.x = (int)p.X;
                }
                if (rectMax.y < p.Y)
                {
                    rectMax.y = (int)p.Y;
                }
            }

            return new RectInt(rectMin, rectMax - rectMin);
        }

        public void PrintPoints ()
        {
            Debug.Log("Outside:");
            for (int i = 0; i < Points.Count; i++)
            {
                Debug.Log("x: " + Points[i].X + " y: " + Points[i].Y);
            }
        }
    }
}