using System;
using System.Collections.Generic;
using UnityEngine;

namespace PolygonLib
{
    using ClipperLib;
    using LibTessDotNet;
    using ConversionLib;
    using Unity.Mathematics;
    using Unity.Collections;

    public static class PolyMath
    {
        private static Clipper cp;
        private static ClipperOffset co;

        /// <summary>
        /// PolygonGroup constructors: Empty, Rect, PolygonObjects
        /// </summary>
        /// 
        /// <returns> PolygonGroup </returns>
        public static PolygonGroup CreateEmptyPolygonGroup ()
        {
            PolygonGroup polyGroup = ScriptableObject.CreateInstance<PolygonGroup>();
            polyGroup.SetPolygons(null);
            return polyGroup;
        }

        public static PolygonGroup CreatePolygonGroupFromRect (RectInt rect, int size = 1, bool isHole = false)
        {
            PolygonGroup polyGroup = ScriptableObject.CreateInstance<PolygonGroup>();
            polyGroup.SetPolygons(new PolygonObject[] { CreatePolygonObjectFromRect (rect, size, isHole) }, 
                new Rect(rect.position * size, rect.size * size));
            return polyGroup;
        }

        public static PolygonGroup CreatePolygonGroup (List<PolygonObject> polys, Rect polysBounds)
        {
            PolygonGroup polyGroup = ScriptableObject.CreateInstance<PolygonGroup>();
            polyGroup.SetPolygons(polys.ToArray(), polysBounds);
            return polyGroup;
        }

        public static PolygonGroup CreatePolygonGroupOfChamferedRectFromRect (RectInt rect, int size = 1, int chamferSize = 1)
        {
            PolygonGroup polyGroup = ScriptableObject.CreateInstance<PolygonGroup>();
            polyGroup.SetPolygons(new PolygonObject[] { CreatePolygonObjectOfChamferedRectFromRect(rect, size, chamferSize) },
                new Rect(rect.position * size, rect.size * size));
            return polyGroup;
        }

        public static PolygonGroup CreatePolygonGroupFromFourPoints (Vector2Int p1, Vector2Int p2, Vector2Int p3, Vector2Int p4, int size = 1, bool isHole = false)
        {
            PolygonGroup polyGroup = ScriptableObject.CreateInstance<PolygonGroup>();
            polyGroup.SetPolygons(new PolygonObject[] { CreatePolygonObjectFromFourPoints(p1, p2, p3, p4, size, isHole) });
            polyGroup.ComputeBounds();

            return polyGroup;
        }

        public static PolygonGroup CreatePolygonGroupFromConventionalVerticeList (List<Vector2Int> vertices, int size = 1)
        {
            PolygonGroup polyGroup = ScriptableObject.CreateInstance<PolygonGroup>();
            polyGroup.SetPolygons(new PolygonObject[] { CreatePolygonObjectFromConventionalVerticeList(vertices, size) });
            polyGroup.ComputeBounds();

            return polyGroup;
        }

        public static PolygonGroup CreatePolygonGroupFromTriangle (Vector2Int v1, Vector2Int v2, Vector2Int v3, int size = 1, bool isHole = false)
        {
            PolygonGroup polyGroup = ScriptableObject.CreateInstance<PolygonGroup>();
            Vector2 min = new Vector2(Math.Min(Mathf.Min(v1.x, v2.x), v3.x), Math.Min(Mathf.Min(v1.y, v2.y), v3.y));
            Vector2 max = new Vector2(Math.Max(Mathf.Max(v1.x, v2.x), v3.x), Math.Max(Mathf.Max(v1.y, v2.y), v3.y));
            polyGroup.SetPolygons(new PolygonObject[] { CreatePolygonObjectFromTriangle(v1, v2, v3, size, isHole) },
                new Rect(min, max - min));
            return polyGroup;
        }

        public static PolygonGroup ClonePolygonGroup (PolygonGroup polygon)
        {
            PolygonGroup polyGroup = ScriptableObject.CreateInstance<PolygonGroup>();
            PolygonObject[] polyObjects = new PolygonObject[polygon.PolygonCount];
            for (int i = 0; i < polyObjects.Length; i++)
            {
                polyObjects[i] = ClonePolygonObject(polygon.Polygons[i]);
            }

            polyGroup.SetPolygons(polyObjects, polygon.PolygonBounds);
            return polyGroup;
        }

        /// <summary>
        /// PolygonObject constructors: Rect, PolygonPaths
        /// </summary>
        /// 
        /// <returns> PolygonObject </returns>
        public static PolygonObject CreatePolygonObjectFromRect (RectInt rect, int size, bool isHole)
        {
            PolygonObject polyObject = ScriptableObject.CreateInstance<PolygonObject>();
            polyObject.SetPaths(new PolygonPath[] { CreatePolygonPathFromRect(rect, size, isHole) });
            return polyObject;
        }

        public static PolygonObject CreatePolygonObjectFromTriangle (Vector2Int v1, Vector2Int v2, Vector2Int v3, int size, bool isHole)
        {
            PolygonObject polyObject = ScriptableObject.CreateInstance<PolygonObject>();
            polyObject.SetPaths(new PolygonPath[] { CreatePolygonPathFromTriangle(v1, v2, v3, size, isHole) });
            return polyObject;
        }

        public static PolygonObject CreatePolygonObjectFromFourPoints (Vector2Int p1, Vector2Int p2, Vector2Int p3, Vector2Int p4, int size, bool isHole)
        {
            PolygonObject polyObject = ScriptableObject.CreateInstance<PolygonObject>();
            polyObject.SetPaths(new PolygonPath[] { CreatePolygonPathFromFourPoints(p1, p2, p3, p4, size, isHole) });
            return polyObject;
        }

        public static PolygonObject CreatePolygonObjectOfChamferedRectFromRect (RectInt rect, int size, int chamferSize)
        {
            PolygonObject polyObject = ScriptableObject.CreateInstance<PolygonObject>();
            polyObject.SetPaths(new PolygonPath[] { CreatePolygonPathOfChamferedRectFromRect(rect, size, chamferSize) });
            return polyObject;
        }

        public static PolygonObject CreatePolygonObject (List<PolygonPath> paths)
        {
            PolygonObject polyObject = ScriptableObject.CreateInstance<PolygonObject>();
            polyObject.SetPaths(paths.ToArray());
            return polyObject;
        }

        public static PolygonObject CreatePolygonObjectFromConventionalVerticeList (List<Vector2Int> vertices, int size = 1)
        {
            List<PolygonPath> paths = new List<PolygonPath>();
            bool isHole = false;
            List<Vector2> points = new List<Vector2>();
            for (int i = 0; i < vertices.Count; i++)
            {
                // in the conventional vertice list, a negative vertice represents the beginning of a new path
                if (vertices[i].x < 0)
                {
                    paths.Add(CreatePolygonPath(points.ToArray(), isHole));
                    points = new List<Vector2>();
                    isHole = true;
                }
                else
                {
                    points.Add(vertices[i] * size);
                }
            }
            paths.Add(CreatePolygonPath(points.ToArray(), isHole));
            return CreatePolygonObject(paths);
        }

        public static PolygonObject ClonePolygonObject (PolygonObject polyObj)
        {
            PolygonObject polyObject = ScriptableObject.CreateInstance<PolygonObject>();
            PolygonPath[] polyPaths = new PolygonPath[polyObj.PathCount];
            for (int i = 0; i < polyPaths.Length; i++)
            {
                polyPaths[i] = ClonePolygonPath(polyObj.Paths[i]);
            }
            polyObject.SetPaths(polyPaths);
            return polyObject;
        }

        /// <summary>
        /// PolygonPath constructors: Rect, IntPoint, Vector2
        /// </summary>
        /// 
        /// <returns> PolygonPath </returns>
        public static PolygonPath CreatePolygonPathFromRect (RectInt rect, int size, bool isHole)
        {
            PolygonPath polyPath = ScriptableObject.CreateInstance<PolygonPath>();
            polyPath.SetPath(GetPathFromRect(rect, size, isHole), PolygonPathType.Outside);
            return polyPath;
        }

        public static PolygonPath CreatePolygonPathFromTriangle (Vector2Int v1, Vector2Int v2, Vector2Int v3, int size, bool isHole)
        {
            PolygonPath polyPath = ScriptableObject.CreateInstance<PolygonPath>();
            polyPath.SetPath(GetPathFromTriangle(v1, v2, v3, size, isHole), PolygonPathType.Outside);
            return polyPath;
        }

        public static PolygonPath CreatePolygonPathFromFourPoints (Vector2Int p1, Vector2Int p2, Vector2Int p3, Vector2Int p4, int size, bool isHole)
        {
            PolygonPath polyPath = ScriptableObject.CreateInstance<PolygonPath>();
            polyPath.SetPath(GetPathFromFourPoints(p1, p2, p3, p4, size, isHole), PolygonPathType.Outside);
            return polyPath;
        }

        public static PolygonPath CreatePolygonPathOfChamferedRectFromRect (RectInt rect, int size, int chamferSize)
        {
            PolygonPath polyPath = ScriptableObject.CreateInstance<PolygonPath>();
            polyPath.SetPath(GetPathOfChamferedRectFromRect(rect, size, chamferSize, false), PolygonPathType.Outside);
            return polyPath;
        }

        public static PolygonPath CreatePolygonPath (List<IntPoint> points, bool isHole)
        {
            return CreatePolygonPath(ConvertMath.ConvertIntPointListToVector2Arr(points), isHole);
        }

        public static PolygonPath CreatePolygonPath (Vector2[] points, bool isHole)
        {
            PolygonPath polyPath = ScriptableObject.CreateInstance<PolygonPath>();
            polyPath.SetPath(points, isHole ? PolygonPathType.Hole : PolygonPathType.Outside);
            return polyPath;
        }

        public static PolygonPath ClonePolygonPath (PolygonPath polyPth)
        {
            PolygonPath polyPath = ScriptableObject.CreateInstance<PolygonPath>();
            Vector2[] polyPoints = new Vector2[polyPth.Points.Length];
            for (int i = 0; i < polyPoints.Length; i++)
            {
                Vector2 pt = polyPth.Points[i];
                polyPoints[i] = new Vector2(pt.x, pt.y);
            }
            polyPath.SetPath(polyPoints, polyPth.Type, polyPth.Index);
            //polyPath.SetPath(polyPth.Points, polyPth.Type, polyPth.Index);
            return polyPath;
        }

        /// <summary>
        /// GetPathFromRect: get a path from a rect with size multiplier and hole flag
        /// </summary>
        /// 
        /// <returns> Vector2[] </returns>
        public static Vector2[] GetPathFromRect (RectInt rect, int size, bool isHole)
        {
            List<Vector2> pathList = new List<Vector2>();
            pathList.Add(rect.position * size);
            pathList.Add(new Vector2(rect.max.x, rect.min.y) * size);
            pathList.Add(new Vector2(rect.max.x, rect.max.y) * size);
            pathList.Add(new Vector2(rect.min.x, rect.max.y) * size);

            if (isHole)
            {
                pathList.Reverse();
            }

            return pathList.ToArray();
        }

        public static Vector2[] GetPathFromTriangle (Vector2Int v1, Vector2Int v2, Vector2Int v3, int size, bool isHole)
        {
            List<Vector2> pathList = new List<Vector2>();
            pathList.Add(v1 * size);
            pathList.Add(v2 * size);
            pathList.Add(v3 * size);

            if (isHole)
            {
                pathList.Reverse();
            }

            return pathList.ToArray();
        }

        public static Vector2[] GetPathFromFourPoints (Vector2Int p1, Vector2Int p2, Vector2Int p3, Vector2Int p4, int size, bool isHole)
        {
            List<Vector2> pathList = new List<Vector2>();
            pathList.Add(p1 * size);
            pathList.Add(p2 * size);
            pathList.Add(p3 * size);
            pathList.Add(p4 * size);

            if (isHole)
            {
                pathList.Reverse();
            }

            return pathList.ToArray();
        }

        /// <summary>
        /// GetPathOfChamferedRectFromRect: get a path of chamfered rect from a rect with size multiplier, chamfersize and hole flag
        /// </summary>
        ///    _ _ _            _ _ _ _
        ///  _|     |_         |       |
        /// |         |        |       |
        /// |         |  from  |       |
        /// |_       _|        |       |
        ///   |_ _ _|          |_ _ _ _|
        ///   
        /// <returns> Vector2[] </returns>
        public static Vector2[] GetPathOfChamferedRectFromRect (RectInt rect, int size, int chamferSize, bool isHole)
        {
            List<Vector2> pathList = new List<Vector2>();

            Vector2 point = Vector2.zero;

            point = rect.position;
            pathList.Add(new Vector2(point.x,               point.y + chamferSize) * size);
            pathList.Add(new Vector2(point.x + chamferSize, point.y + chamferSize) * size);
            pathList.Add(new Vector2(point.x + chamferSize, point.y              ) * size);

            point = new Vector2(rect.max.x, rect.min.y);
            pathList.Add(new Vector2(point.x - chamferSize, point.y              ) * size);
            pathList.Add(new Vector2(point.x - chamferSize, point.y + chamferSize) * size);
            pathList.Add(new Vector2(point.x              , point.y + chamferSize) * size);

            point = new Vector2(rect.max.x, rect.max.y);
            pathList.Add(new Vector2(point.x              , point.y - chamferSize) * size);
            pathList.Add(new Vector2(point.x - chamferSize, point.y - chamferSize) * size);
            pathList.Add(new Vector2(point.x - chamferSize, point.y              ) * size);

            point = new Vector2(rect.min.x, rect.max.y);
            pathList.Add(new Vector2(point.x + chamferSize, point.y              ) * size);
            pathList.Add(new Vector2(point.x + chamferSize, point.y - chamferSize) * size);
            pathList.Add(new Vector2(point.x              , point.y - chamferSize) * size);

            if (isHole)
            {
                pathList.Reverse();
            }

            return pathList.ToArray();
        }

        public static PolygonGroup CleanPolygonGroup (PolygonGroup polyGroup, int tileSize)
        {
            List<List<IntPoint>> points = new List<List<IntPoint>>();

            PolygonObject[] polyObjects = polyGroup.Polygons;

            for (int i = 0; i < polyObjects.Length; i++)
            {
                PolygonPath[] polyPaths = polyObjects[i].Paths;

                for (int j = 0; j < polyPaths.Length; j++)
                {
                    Vector2[] polyPoints = polyPaths[j].Points;

                    for (int k = 0; k < polyPoints.Length; k++)
                    {
                        polyPoints[k] /= tileSize;
                        polyPoints[k] = polyPoints[k].RoundToInt() * tileSize;
                    }

                    List<IntPoint> pathCleaned = Clipper.CleanPolygon(ConvertMath.ConvertVector2ToIntPointList(polyPoints));

                    polyPaths[j].UpdatePath(ConvertMath.ConvertIntPointListToVector2Arr(pathCleaned));
                }
            }

            return polyGroup;
        }

        public static PolygonGroup SimplifyPolygonGroup (PolygonGroup polys, bool computeBounds = true)
        {
            cp = new Clipper();

            //cp.ReverseSolution = true;
            cp.StrictlySimple = true; // true maybe
            cp.PreserveCollinear = false;

            cp.AddPaths(Clipper.SimplifyPolygons(GetClipperPathsFromPolygonGroup(polys), PolyFillType.pftNonZero), PolyType.ptSubject, true);

            PolyTree solution = new PolyTree();

            cp.Execute(ClipType.ctUnion, solution, PolyFillType.pftNonZero);

            return GetPolygonGroupFromSolution(solution, computeBounds);


            /*
            Paths result = new Paths();
            Clipper c = new Clipper();
            c.StrictlySimple = true;
            c.AddPaths(polys, PolyType.ptSubject, true);
            c.Execute(ClipType.ctUnion, result, fillType, fillType);
            return result;
            return polyGroup;
            */
        }
 
        public static void MovePolygonGroup (PolygonGroup poly, Vector2 movementValue)
        {
            if (movementValue == Vector2.zero || movementValue == Vector2Int.zero)
            {
                return;
            }

            PolygonObject[] polyObjects = poly.Polygons;

            for (int i = 0; i < polyObjects.Length; i++)
            {
                PolygonPath[] polyPaths = polyObjects[i].Paths;

                for (int j = 0; j < polyPaths.Length; j++)
                {
                    polyPaths[j].UpdatePath(MathAux.MovePoints(movementValue, polyPaths[j].Points, true));
                }
            }

            poly.SetBounds(MathAux.MoveRect(movementValue, poly.PolygonBounds, true));
        }

        public static void RotatePolygonGroup (PolygonGroup poly, float rotateAngle, Vector2 pivotPoint, bool keepPosition = false)
        {
            if (rotateAngle == 0.0f)
            {
                return;
            }

            //Debug.Log("Pivot: " + pivotPoint);

            Rect rectRotated = MathAux.RotateRect(pivotPoint, rotateAngle, poly.PolygonBounds, Vector2.zero, true);

            // this offset position is used to keep position on bottom left
            Vector2 rotatedOffsetPosition = keepPosition ? poly.PolygonBounds.position - rectRotated.position : Vector2.zero;

            //Debug.Log("Init: " + poly.PolygonBounds.position + "Size: " + poly.PolygonBounds.size + "  Rotated: " + rectRotated.position);
            //Debug.Log("Offset: " + rotatedOffsetPosition);

            rectRotated = MathAux.MoveRect(rotatedOffsetPosition, rectRotated, true);

            PolygonObject[] polyObjects = poly.Polygons;

            for (int i = 0; i < polyObjects.Length; i++)
            {
                PolygonPath[] polyPaths = polyObjects[i].Paths;

                for (int j = 0; j < polyPaths.Length; j++)
                {
                    polyPaths[j].UpdatePath(MathAux.RotatePoints(pivotPoint, rotateAngle, polyPaths[j].Points, rotatedOffsetPosition, true));
                }
            }

            poly.SetBounds(rectRotated);
        }

        public static PolygonGroup GetResizePolygonGroup (PolygonGroup poly, bool expandPoly, ushort expandSize = 1)
        {
            PolygonObject[] polyObjects = poly.Polygons;

            List<PolygonObject> newPolyObjects = new List<PolygonObject>();

            for (int i = 0; i < polyObjects.Length; i++)
            {
                PolygonPath[] polyPaths = polyObjects[i].Paths;

                List<PolygonPath> newPolyPaths = new List<PolygonPath>();

                for (int j = 0; j < polyPaths.Length; j++)
                {
                    bool outsideBorder = polyPaths[j].Type == PolygonPathType.Outside ? expandPoly : !expandPoly;
                    PolygonPath resizePath = CreatePolygonPathBorderFromPolygonPath(polyPaths[j], expandPoly, expandSize);
                    newPolyPaths.Add(resizePath);
                }

                newPolyObjects.Add(CreatePolygonObject(newPolyPaths));
            }

            PolygonGroup resizePolygonGroup = CreatePolygonGroup(newPolyObjects, Rect.zero);
            return resizePolygonGroup;
        }

        /// <summary>
        /// CreatePolygonGroupBorder: get a polygon group border from a polygon group source
        /// </summary>
        /// 
        /// <returns> PolygonGroup </returns>
        public static PolygonGroup CreatePolygonGroupBorder (PolygonGroup poly, bool outsideBorder, double borderSize = 1)
        {
            PolygonObject[] polyObjects = poly.Polygons;

            List<PolygonObject> newPolyObjects = new List<PolygonObject>();

            for (int i = 0; i < polyObjects.Length; i++)
            {
                PolygonPath[] polyPaths = polyObjects[i].Paths;

                for (int j = 0; j < polyPaths.Length; j++)
                {
                    List<PolygonPath> newPolyPaths = new List<PolygonPath>();

                    bool needReversePath = (polyPaths[j].Type == PolygonPathType.Hole) ^ outsideBorder;

                    PolygonPath originalPath = CreatePolygonPath(polyPaths[j].Points, polyPaths[j].Type == PolygonPathType.Hole);

                    if (needReversePath)
                    {
                        originalPath.ReversePath();
                    }

                    //PolygonPath borderPath = CreatePolygonPathBorderFromPolygonPath(polyPaths[j], outsideBorder, borderSize);

                    double bsize = polyPaths[j].Type == PolygonPathType.Hole ? -borderSize : borderSize;

                    PolygonPath borderPath = GetPolygonPathOffset(polyPaths[j], outsideBorder ? bsize : -bsize);

                    if (!needReversePath)
                    {
                        borderPath.ReversePath();
                    }

                    if (originalPath.Type == borderPath.Type)
                    {
                        Debug.LogError("CANNOT BE EQUAL.");
                    }

                    if (originalPath.Type == PolygonPathType.Outside)
                    {
                        newPolyPaths.Add(originalPath);
                        newPolyPaths.Add(borderPath);
                    }
                    else
                    {
                        newPolyPaths.Add(borderPath);
                        newPolyPaths.Add(originalPath);
                    }

                    if (newPolyPaths.Count != 2)
                    {
                        Debug.LogError("CANNOT BE DIFFERENT THAN 2. Its " + newPolyPaths.Count);
                    }

                    newPolyObjects.Add(CreatePolygonObject(newPolyPaths));
                }
            }

            PolygonGroup border = CreatePolygonGroup(newPolyObjects, Rect.zero);
            ComputePolygonGroupBounds(border);

            return border;
        }

        /// <summary>
        /// CreatePolygonPathBorderFromPolygonPath: create a border polygon path from a polygon path
        /// </summary>
        /// 
        /// <returns> PolygonPath </returns>
        public static PolygonPath CreatePolygonPathBorderFromPolygonPath (PolygonPath polyPath, bool outsideBorder, ushort borderSize = 1)
        {
            int sizeValue = borderSize;
            if (outsideBorder)
            {
                sizeValue = polyPath.Type == PolygonPathType.Outside ? -sizeValue : sizeValue;
            }
            else
            {
                sizeValue = polyPath.Type == PolygonPathType.Outside ? sizeValue : -sizeValue;
            }

            if (polyPath.Type == PolygonPathType.Hole)
            {
                sizeValue = -sizeValue;
            }

            Vector2[] polyPoints   = polyPath.Points;
            Vector2[] borderPoints = new Vector2[polyPoints.Length];

            for (int i = 0; i < polyPoints.Length; i++)
            {
                int previousIdx = i - 1;
                int nextIdx = i + 1;
                if (previousIdx < 0)
                {
                    previousIdx = polyPoints.Length - 1;
                }
                if (nextIdx == polyPoints.Length)
                {
                    nextIdx = 0;
                }

                Vector2 previousPoint = polyPoints[previousIdx];
                Vector2 currentPoint  = polyPoints[i];
                Vector2 nextPoint     = polyPoints[nextIdx];

                Vector2 newPoint      = currentPoint;

                if (previousPoint.y == currentPoint.y)
                {
                    if (previousPoint.x > currentPoint.x)
                    {
                        if (nextPoint.y > currentPoint.y)
                        {
                            newPoint.x -= sizeValue;
                            newPoint.y -= sizeValue;
                        }
                        else
                        {
                            newPoint.x += sizeValue;
                            newPoint.y -= sizeValue;
                        }
                    }
                    else
                    {
                        if (nextPoint.y > currentPoint.y)
                        {
                            newPoint.x -= sizeValue;
                            newPoint.y += sizeValue;
                        }
                        else
                        {
                            newPoint.x += sizeValue;
                            newPoint.y += sizeValue;
                        }
                    }
                }
                else
                {
                    if (previousPoint.y > currentPoint.y)
                    {
                        if (nextPoint.x > currentPoint.x)
                        {
                            newPoint.x += sizeValue;
                            newPoint.y += sizeValue;
                        }
                        else
                        {
                            newPoint.x += sizeValue;
                            newPoint.y -= sizeValue;
                        }
                    }
                    else
                    {
                        if (nextPoint.x > currentPoint.x)
                        {
                            newPoint.x -= sizeValue;
                            newPoint.y += sizeValue;
                        }
                        else
                        {
                            newPoint.x -= sizeValue;
                            newPoint.y -= sizeValue;
                        }
                    }
                }

                borderPoints[i] = newPoint;
            }

            return CreatePolygonPath(borderPoints, polyPath.Type == PolygonPathType.Hole);
        }

        /// <summary>
        /// Union: get a polygongroup resulted from two polygons union
        /// </summary>
        /// 
        /// <returns> PolygonGroup </returns>
        public static PolygonGroup Union (PolygonGroup poly1, PolygonGroup poly2, bool computeBounds = true)
        {
            cp = new Clipper();

            //cp.ReverseSolution = true;
            cp.StrictlySimple = false; // true maybe
            cp.PreserveCollinear = false;

            cp.AddPaths(GetClipperPathsFromPolygonGroup(poly1), PolyType.ptClip, true);
            cp.AddPaths(GetClipperPathsFromPolygonGroup(poly2), PolyType.ptClip, true);

            PolyTree solution = new PolyTree();

            cp.Execute(ClipType.ctUnion, solution, PolyFillType.pftNonZero);

            return GetPolygonGroupFromSolution(solution, computeBounds);
        }

        public static PolygonGroup Union2 (PolygonGroup poly1, PolygonGroup poly2, bool computeBounds = true)
        {
            cp = new Clipper();

            //cp.ReverseSolution = true;
            cp.StrictlySimple = true; // true maybe
            //cp.PreserveCollinear = false;

            cp.AddPaths(GetClipperPathsFromPolygonGroup(poly1), PolyType.ptSubject, true);
            cp.AddPaths(GetClipperPathsFromPolygonGroup(poly2), PolyType.ptSubject, true);

            PolyTree solution = new PolyTree();

            cp.Execute(ClipType.ctUnion, solution, PolyFillType.pftNonZero);

            return GetPolygonGroupFromSolution(solution, computeBounds);
        }

        public static PolygonGroup Union (PolygonGroup polys, bool computeBounds = true)
        {
            cp = new Clipper();

            //cp.ReverseSolution = true;
            cp.StrictlySimple = false; // true maybe
            cp.PreserveCollinear = false;

            cp.AddPaths(Clipper.SimplifyPolygons(GetClipperPathsFromPolygonGroup(polys), PolyFillType.pftNonZero), PolyType.ptClip, true);

            PolyTree solution = new PolyTree();

            cp.Execute(ClipType.ctUnion, solution, PolyFillType.pftNonZero);

            return GetPolygonGroupFromSolution(solution, computeBounds);
        }

        /// <summary>
        /// Difference: get a polygongroup resulted from the differente between two polygons
        /// </summary>
        /// 
        /// <returns> PolygonGroup </returns>
        public static PolygonGroup Difference (PolygonGroup clipPoly, PolygonGroup subjectPoly, bool computeBounds = true)
        {
            cp = new Clipper();

            //cp.ReverseSolution = true;
            cp.StrictlySimple = false; // true maybe
            cp.PreserveCollinear = false;

            cp.AddPaths(GetClipperPathsFromPolygonGroup(clipPoly), PolyType.ptClip, true);
            cp.AddPaths(GetClipperPathsFromPolygonGroup(subjectPoly), PolyType.ptSubject, true);

            PolyTree solution = new PolyTree();

            cp.Execute(ClipType.ctDifference, solution, PolyFillType.pftNonZero, PolyFillType.pftNonZero);

            return GetPolygonGroupFromSolution(solution, computeBounds);
        }

        /// <summary>
        /// Intersection: get a polygongroup resulted from the intersection between two polygons
        /// </summary>
        /// 
        /// <returns> PolygonGroup </returns>
        public static PolygonGroup Intersection (PolygonGroup clipPoly, PolygonGroup subjectPoly, bool computeBounds = true)
        {
            cp = new Clipper();

            //cp.ReverseSolution = true;
            cp.StrictlySimple = true; // true maybe
            //cp.PreserveCollinear = false;

            cp.AddPaths(GetClipperPathsFromPolygonGroup(clipPoly), PolyType.ptClip, true);
            cp.AddPaths(GetClipperPathsFromPolygonGroup(subjectPoly), PolyType.ptSubject, true);

            PolyTree solution = new PolyTree();

            cp.Execute(ClipType.ctIntersection, solution, PolyFillType.pftNonZero, PolyFillType.pftNonZero);

            return GetPolygonGroupFromSolution(solution, computeBounds);
        }

        /// <summary>
        /// XOR: get a polygongroup resulted from the XOR operation between two polygons
        /// </summary>
        /// 
        /// <returns> PolygonGroup </returns>
        public static PolygonGroup XOR (PolygonGroup clipPoly, PolygonGroup subjectPoly, bool computeBounds = true)
        {
            cp = new Clipper();

            //cp.ReverseSolution = true;
            cp.StrictlySimple = true; // true maybe
            //cp.PreserveCollinear = false;

            cp.AddPaths(GetClipperPathsFromPolygonGroup(clipPoly), PolyType.ptClip, true);
            cp.AddPaths(GetClipperPathsFromPolygonGroup(subjectPoly), PolyType.ptSubject, true);

            PolyTree solution = new PolyTree();

            cp.Execute(ClipType.ctXor, solution, PolyFillType.pftNonZero, PolyFillType.pftNonZero);

            return GetPolygonGroupFromSolution(solution, computeBounds);
        }

        public static PathOrientation PathOrientation (List<Vector2Int> pathVertices)
        {
            return Clipper.GetPathOrientation(ConvertMath.ConvertVector2ToIntPointList(pathVertices));
        }

        /// <summary>
        /// IsPolygonIntersectPolygon: check whether a polygon intersects another polygon
        /// </summary>
        /// 
        /// <returns> bool </returns>
        public static bool IsPolygonIntersectPolygon (PolygonGroup poly1, PolygonGroup poly2)
        {
            return Intersection(poly1, poly2, false).PolygonCount > 0;
        }

        /// <summary>
        /// IsPolygonInsidePolygon: check whether a polygon is completed inside another polygon
        /// </summary>
        ///  __________
        /// |poly2     |
        /// |   _____  |
        /// |  |poly1| |
        /// |  |_____| |
        /// |__________|
        /// 
        /// <returns> bool </returns>
        public static bool IsPolygonInsidePolygon (PolygonGroup poly1, PolygonGroup poly2)
        {
            return Difference(poly2, poly1, false).PolygonCount == 0;
        }

        /// <summary>
        /// IsPolygonTouchPolygon: check whether a polygon touches another polygon
        /// </summary>
        /// 
        /// <returns> bool </returns>
        public static bool IsPolygonTouchPolygon (PolygonGroup poly1, PolygonGroup poly2)
        {
            Debug.LogError("IsPolygonTouchPolygon not implemented yet");
            return false; //return Union(poly2, poly1, false).PolygonCount < 0;
        }

        /// <summary>
        /// GetClipperPathsFromPolygonGroup: get clipper paths pattern from a polygon group
        /// </summary>
        /// TODO: remove checking orientation
        /// <returns> List<List<IntPoint>> </returns>
        private static List<List<IntPoint>> GetClipperPathsFromPolygonGroup (PolygonGroup polyGroup)
        {
            List<List<IntPoint>> paths = new List<List<IntPoint>>();

            PolygonObject[] polygons = polyGroup.Polygons;

            for (int i = 0; i < polygons.Length; i++)
            {
                PolygonPath[] polyPaths = polygons[i].Paths;

                for (int j = 0; j < polyPaths.Length; j++)
                {
                    List<IntPoint> lstPoints = ConvertMath.ConvertVector2ToIntPointList(polyPaths[j].Points);
                    PathOrientation pathOrientation = Clipper.GetPathOrientation(lstPoints);
                    PathOrientation pathOriginal = polyPaths[j].Type == PolygonPathType.Hole ? 
                        ClipperLib.PathOrientation.Clockwise : ClipperLib.PathOrientation.CounterClockwise;
                    if (pathOrientation != pathOriginal)
                    {
                        //Debug.LogError("DIFFER ORIENTATION!!");
                    }

                    paths.Add(lstPoints);
                }
            }

            return paths;
        }

        public static PathOrientation GetOrientation (NativeArray<int2> path)
        {
            return Clipper.GetPathOrientation(ConvertMath.ConvertInt2ToIntPointList(path));
        }

        public static PolygonGroup UnionTouchedHoles (PolygonGroup polyGroup)
        {
            Debug.LogError("Not implemented yet");
            return null;

            /*
            PolygonObject[] polyObjects = polyGroup.Polygons;

            List<PolygonObject> newPolygonObjects = new List<PolygonObject>();

            for (int i = 0; i < polyObjects.Length; i++)
            {
                PolygonPath[] polyPaths = polyObjects[i].Paths;

                List<PolygonObject> holesObjects = new List<PolygonObject>();

                for (int j = 0; j < polyPaths.Length; j++)
                {
                    if (polyPaths[j].Type == PolygonPathType.Hole)
                    {
                        PolygonPath holeBorder = CreatePolygonPathBorderFromPolygonPath(polyPaths[j], true, 1);
                        holeBorder.ReversePath();
                        holesObjects.Add(CreatePolygonObject(new List<PolygonPath>() { holeBorder }));
                    }
                }

                PolygonGroup holesGroup = CreatePolygonGroup(holesObjects, Rect.zero);

                // union the polygon meshes holes
                holesGroup = Union(holesGroup, false);

                // reverse the holes to its origin points
                holesObjects = new List<PolygonObject>();

                PolygonObject[] polyHolesObjects = holesGroup.Polygons;

                for (int j = 0; j < polyHolesObjects.Length; j++)
                {
                    PolygonPath[] polyHolesPaths = polyHolesObjects[j].Paths;

                    List<PolygonPath> newPolyPaths = new List<PolygonPath>();

                    newPolyPaths.Add(polyPaths[0]);

                    for (int k = 0; k < polyHolesPaths.Length; k++)
                    {
                        PolygonPath newPolyPathHole = CreatePolygonPathBorderFromPolygonPath(polyHolesPaths[k], false, 1);
                        newPolyPathHole.ReversePath();
                        newPolyPaths.Add(newPolyPathHole);
                    }

                    newPolygonObjects.Add(CreatePolygonObject(newPolyPaths));
                }
            }

            return CreatePolygonGroup(newPolygonObjects, Rect.zero);
            */
        }

        public static PolygonGroup GetPolygonGroupOffset (PolygonGroup polyGroup, int polyOffset)
        {
            if (polyOffset == 0)
            {
                return polyGroup;
            }

            co = new ClipperOffset();

            co.AddPaths(GetClipperPathsFromPolygonGroup(polyGroup), JoinType.jtMiter, EndType.etClosedPolygon);

            PolyTree solution = new PolyTree();

            co.Execute(ref solution, polyOffset);

            return GetPolygonGroupFromSolution(solution, false);
        }

        public static PolygonPath GetPolygonPathOffset (PolygonPath polyPath, double polyOffset)
        {
            if (polyOffset == 0)
            {
                return polyPath;
            }

            co = new ClipperOffset();

            co.AddPath(GetClipperPathFromPolygonPath(polyPath), JoinType.jtMiter, EndType.etClosedPolygon);

            List<List<IntPoint>> pathsResult = new List<List<IntPoint>>();

            co.Execute(ref pathsResult, polyOffset);

            if (pathsResult.Count > 0)
            {
                return CreatePolygonPath(ConvertMath.ConvertIntPointListToVector2Arr(pathsResult[0]), polyPath.Type == PolygonPathType.Hole);
            }

            return polyPath;
        }

        public static List<IntPoint> GetClipperPathFromPolygonPath (PolygonPath polyPath)
        {
            List<IntPoint> path = new List<IntPoint>();

            Vector2[] pathPoint = polyPath.Points;

            for (int i = 0; i < pathPoint.Length; i++)
            {
                path.Add(new IntPoint(pathPoint[i]));
            }

            return path;
        }

        /// <summary>
        /// GetPolygonGroupFromSolution: get a polygon group from a clipper solution
        /// </summary>
        /// 
        /// <returns> PolygonGroup </returns>
        private static PolygonGroup GetPolygonGroupFromSolution (PolyTree solution, bool computeBounds = true)
        {
            List<PolyNode> nonHoleNodes = new List<PolyNode>();

            // first get all non-hole nodes
            PolyNode polyNode = solution.GetFirst();

            while (polyNode != null)
            {
                if (!polyNode.IsOpen)
                {
                    if (!polyNode.IsHole)
                    {
                        nonHoleNodes.Add(polyNode);
                    }
                }
                else
                {
                    Debug.LogError("class Poly2Clip GetPolygonGroupFromSolution : Some poly node is open.");
                }

                polyNode = polyNode.GetNext();
            }

            List<List<IntPoint>> outsidePaths = new List<List<IntPoint>>();

            List<PolygonObject> polys = new List<PolygonObject>();

            // now gets the childs for each parent
            for (int i = 0; i < nonHoleNodes.Count; i++)
            {
                if (computeBounds)
                {
                    outsidePaths.Add(nonHoleNodes[i].Contour);
                }

                List<PolygonPath> polyPaths = new List<PolygonPath>();

                polyPaths.Add(CreatePolygonPath(nonHoleNodes[i].Contour, false));

                List<PolyNode> childNodes = nonHoleNodes[i].Childs;

                for (int j = 0; j < childNodes.Count; j++)
                {
                    if (!childNodes[j].IsHole)
                    {
                        Debug.LogError("It is not hole");
                    }

                    polyPaths.Add(CreatePolygonPath(childNodes[j].Contour, true));
                }

                polys.Add(CreatePolygonObject(polyPaths));
            }

            return CreatePolygonGroup(polys, computeBounds ?
                IntRect.ToRect(Clipper.GetBounds(outsidePaths)) : new Rect(Vector2.zero, Vector2.zero));
        }

        /// <summary>
        /// ComputePolygonGroupBounds: compute the bounds of a polygon group
        /// </summary>
        /// 
        /// <returns> Rect </returns>
        public static Rect ComputePolygonGroupBounds (PolygonGroup polygon)
        {
            Rect bounds = new Rect(Vector2.zero, Vector2.zero);

            if (polygon.PolygonCount > 0)
            {
                bounds = IntRect.ToRect(Clipper.GetBounds(GetClipperPathsFromPolygonGroup(polygon)));
            }

            polygon.SetBounds(bounds);

            return bounds;
        }

        /// <summary>
        /// Triangulate: triangulate a polygon group
        /// </summary>
        /// 
        /// <returns> int[], out List<Vector3>, out List<Vector3></returns>
        public static int[] Triangulate (PolygonGroup polygon, float height, float polygonMultiplier, out List<Vector3> vertices, out List<Vector3> uvs)
        {
            Tess tess = new Tess();
            tess.NoEmptyPolygons = true;
            tess.Normal = new Vec3() { X = 0.0f, Y = 0.0f, Z = -1.0f };

            PolygonObject[] polys = polygon.Polygons;

            for (int i = 0; i < polys.Length; i++)
            {
                PolygonPath[] paths = polys[i].Paths;

                for (int j = 0; j < paths.Length; j++)
                {
                    tess.AddContour(ConvertMath.ConvertVector2ArrToContourVertexArr(paths[j].Points),
                        paths[j].Type == PolygonPathType.Outside ? 
                        ContourOrientation.CounterClockwise :
                        ContourOrientation.Clockwise);
                }
            }

            tess.Tessellate(WindingRule.NonZero, ElementType.Polygons, 3);

            // extract the vertices and triangles
            Vector2Int pointInt = Vector2Int.zero;
            vertices = new List<Vector3>();
            uvs = new List<Vector3>();

            for (int i = 0; i < tess.Vertices.Length; i++)
            {
                Vec3 v = tess.Vertices[i].Position;
                vertices.Add(new Vector3(v.X * polygonMultiplier, height, v.Y * polygonMultiplier));
                uvs.Add(new Vector3(v.X, v.Y, 0.0f));
            }

            return tess.Elements;
        }

        /// <summary>
        /// Triangulate: triangulate a polygon group with layer id, uv size and uv offset
        /// </summary>
        /// 
        /// <returns> int[], out List<Vector3>, out List<Vector3></returns>
        public static int[] Triangulate (PolygonGroup polygon, float height, LayerProperties layerProp, out List<Vector3> vertices, out List<Vector3> uvs)
        {
            Tess tess = new Tess();
            tess.NoEmptyPolygons = true;
            tess.Normal = new Vec3() { X = 0.0f, Y = 0.0f, Z = -1.0f };

            PolygonObject[] polys = polygon.Polygons;

            for (int i = 0; i < polys.Length; i++)
            {
                PolygonPath[] paths = polys[i].Paths;

                for (int j = 0; j < paths.Length; j++)
                {
                    tess.AddContour(ConvertMath.ConvertVector2ArrToContourVertexArr(paths[j].Points),
                        paths[j].Type == PolygonPathType.Outside ?
                        ContourOrientation.CounterClockwise :
                        ContourOrientation.Clockwise);
                }
            }

            tess.Tessellate(WindingRule.NonZero, ElementType.Polygons, 3);

            // extract the vertices and triangles
            Vector2Int pointInt = Vector2Int.zero;
            vertices = new List<Vector3>();
            uvs = new List<Vector3>();

            for (int i = 0; i < tess.Vertices.Length; i++)
            {
                Vec3 v = tess.Vertices[i].Position;
                vertices.Add(new Vector3(v.X, height, v.Y));
                uvs.Add(new Vector3(v.X / layerProp.size + layerProp.offset.x, v.Y / layerProp.size + layerProp.offset.y, layerProp.id));
            }

            return tess.Elements;
        }

        /// <summary>
        /// Triangulate: triangulate a polygon group with layer id, uv size and uv offset
        /// </summary>
        /// 
        /// <returns> int[], out List<Vector3>, out List<Vector3></returns>
        public static int[] Triangulate (PolygonGroup polygon, float polygonMultiplier, out List<Vector3> vertices, out List<Vector3> uvs)
        {
            Tess tess = new Tess();
            tess.NoEmptyPolygons = true;
            tess.Normal = new Vec3() { X = 0.0f, Y = 0.0f, Z = -1.0f };

            PolygonObject[] polys = polygon.Polygons;

            for (int i = 0; i < polys.Length; i++)
            {
                PolygonPath[] paths = polys[i].Paths;

                for (int j = 0; j < paths.Length; j++)
                {
                    tess.AddContour(ConvertMath.ConvertVector2ArrToContourVertexArr(paths[j].Points),
                        paths[j].Type == PolygonPathType.Outside ?
                        ContourOrientation.CounterClockwise :
                        ContourOrientation.Clockwise);

                    if (j == 0 && paths[j].Type == PolygonPathType.Hole)
                    {
                        Debug.LogError("ERROR A");
                    }
                    else if (j != 0 && paths[j].Type == PolygonPathType.Outside)
                    {
                        Debug.LogError("ERROR B");
                    }
                }
            }

            tess.Tessellate(WindingRule.NonZero, ElementType.Polygons, 3);

            // extract the vertices and triangles
            vertices = new List<Vector3>();
            uvs = new List<Vector3>();

            for (int i = 0; i < tess.Vertices.Length; i++)
            {
                Vec3 v = tess.Vertices[i].Position;
                vertices.Add(new Vector3(v.X, 0.0f, v.Y) / polygonMultiplier);
                uvs.Add(new Vector3(v.X, v.Y, 0.0f));
            }

            return tess.Elements;
        }

        public static PolygonGroup CreateChamferedPolygonFromPolygonGroup (PolygonGroup polygon, int chamferSize = 5)
        {
            PolygonGroup polyGroup = ScriptableObject.CreateInstance<PolygonGroup>();

            PolygonObject[] polygonObjects = new PolygonObject[polygon.Polygons.Length];

            for (int i = 0; i < polygonObjects.Length; i++)
            {
                polygonObjects[i] = CreateChamferedPolygonObjectFromPolygonObject(polygon.Polygons[i], chamferSize);
            }

            polyGroup.SetPolygons(polygonObjects);
            //polyGroup.ComputeBounds();

            return polyGroup;
        }

        public static PolygonObject CreateChamferedPolygonObjectFromPolygonObject (PolygonObject polygonObject, int chamferSize = 1)
        {
            PolygonObject polyObject = ScriptableObject.CreateInstance<PolygonObject>();

            PolygonPath[] polygonPaths = new PolygonPath[polygonObject.Paths.Length];

            for (int i = 0; i < polygonPaths.Length; i++)
            {
                polygonPaths[i] = CreateChamferedPolygonPathFromPolygonPath(polygonObject.Paths[i], chamferSize);
            }

            polyObject.SetPaths(polygonPaths);

            return polyObject;
        }

        public static PolygonPath CreateChamferedPolygonPathFromPolygonPath (PolygonPath polygonPath, int chamferSize = 1)
        {
            PolygonPath polyPath = ScriptableObject.CreateInstance<PolygonPath>();

            polyPath.SetPath(GetChamferedPathPoints(polygonPath.Points, polygonPath.Type == PolygonPathType.Outside, chamferSize), polygonPath.Type);

            return polyPath;
        }

        public static Vector2[] GetChamferedPathPoints (Vector2[] points, bool counterclock, int chamferSize = 1)
        {
            List<Vector2> pointsChamfered = new List<Vector2>();

            int cfSize = chamferSize;// counterclock ? chamferSize : -chamferSize;

            float maxDist = Mathf.Sqrt(2 * cfSize * cfSize) + 0.1f; // this will exclude to chamfer wide angles

            for (int i = 0; i < points.Length; i++)
            {
                Vector2 previousPoint = (i == 0) ? points[points.Length - 1] : points[i - 1];
                Vector2 point         = points[i];
                Vector2 nextPoint     = (i == points.Length - 1) ? points[0] : points[i + 1];

                Vector2 p1 = new Vector2(point.x - math.sign(point.x - previousPoint.x) * cfSize, point.y - math.sign(point.y - previousPoint.y) * cfSize);
                Vector2 p2 = new Vector2(point.x + math.sign(nextPoint.x - point.x) * cfSize, point.y + math.sign(nextPoint.y - point.y) * cfSize);

                if (Vector2.Distance(p1, p2) < maxDist)
                {
                    pointsChamfered.Add(p1);
                    pointsChamfered.Add(p2);
                }
                else
                {
                    pointsChamfered.Add(point);
                }
            }

            return pointsChamfered.ToArray();
        }

        public static void AddPolygonGroup (ref PolygonGroup polygon, PolygonGroup polygonToAdd)
        {
            int polygonObjectSize = polygon.Polygons.Length + polygonToAdd.Polygons.Length;

            PolygonObject[] polygonObjects = new PolygonObject[polygonObjectSize];

            int polyObjectCount = 0;

            for (int i = 0; i < polygon.Polygons.Length; i++)
            {
                polygonObjects[polyObjectCount++] = polygon.Polygons[i];
            }

            for (int i = 0; i < polygonToAdd.Polygons.Length; i++)
            {
                polygonObjects[polyObjectCount++] = polygonToAdd.Polygons[i];
            }

            polygon.SetPolygons(polygonObjects);
        }

        public static PolygonGroup CleanManualPolygonGroup (PolygonGroup polygon, int tileSize)
        {
            for (int i = 0; i < polygon.PolygonCount; i++)
            {
                PolygonObject polyObject = polygon.Polygons[i];

                for (int j = 0; j < polyObject.Paths.Length; j++)
                {
                    PolygonPath polyPath = polyObject.Paths[j];

                    PolygonPathType polyPathType = polyPath.type;

                    Vector2[] pathPoints = polyPath.Points;

                    List<Vector2> pathCleaned = new List<Vector2>();

                    Vector2 previousPoint = new Vector2(-1, -1);

                    for (int k = 0; k < pathPoints.Length; k++)
                    {
                        Vector2 point = pathPoints[k] / tileSize;
                        point = point.RoundToInt() * tileSize;

                        if (previousPoint != point)
                        {
                            pathCleaned.Add(point);
                            previousPoint = point;
                        }
                    }

                    if (pathCleaned[pathCleaned.Count - 1] == pathCleaned[0])
                    {
                        pathCleaned.RemoveAt(pathCleaned.Count - 1);
                    }

                    polygon.Polygons[i].Paths[j].SetPath(pathCleaned.ToArray(), polyPathType);
                }
            }

            return ClonePolygonGroup(polygon);
        }
    }
}