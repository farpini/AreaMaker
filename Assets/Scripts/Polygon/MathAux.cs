using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public enum Orientation2D { Horizontal = 0, Vertical = 1 };
public enum FourDirection { West = 0, South = 1, East = 2, North = 3 };
public enum EightDirection { West = 0, South = 1, East = 2, North = 3, NorthWest = 4, SouthWest = 5, SouthEast = 6, NorthEast = 7 };
public enum VerticalDirection { Up, Down };
public enum RectangleBorder { Top = 0, Right = 1, Bottom = 2, Left = 3 };
public enum RectangleCorner { BottomLeft = 0, BottomRight = 1, TopRight = 2, TopLeft = 3 };
public enum ComparisonValue { Equal, Greater, Less };
public enum TileRotation { Rot0 = 0, Rot90 = 90, Rot180 = 180, Rot270 = 270 };
public enum DiagonalDirection { BottomLeftUp, TopLeftDown, BottomRightUp, TopRightDown }
public enum DiceSide { Top = 0, Right = 1, Bottom = 2, Left = 3, Front = 4, Back = 5}
public enum OrderEightDirection { West = 0, SouthWest = 1, South = 2, SouthEast = 3, East = 4, NorthEast = 5, North = 6, NorthWest = 7 };


public static class MathAux
{
    public static int2 Int2_One = new int2(1, 1);

    public static void Initialize ()
    {
    }

    public static Vector2 Normalize (Vector2 min, Vector2 max, Vector2 value)
    {
        return new Vector2(Mathf.InverseLerp(min.x, max.x, value.x), Mathf.InverseLerp(min.y, max.y, value.y));
    }

    public static float Distance (Vector2 p1, Vector2 p2)
    {
        return Mathf.Sqrt(Mathf.Pow(p2.x - p1.x, 2) + Mathf.Pow(p2.y - p1.y, 2));
    }

    public static float AngleBetween (Vector2 p1, Vector2 p2)
    {
        float xDiff = p2.x - p1.x;
        float yDiff = p2.y - p1.y;

        return Mathf.Atan2(yDiff, xDiff) * 180.0f / Mathf.PI;
    }

    public static bool RectangleIntersectsRectangle (Rect rect1, Rect rect2)
    {
        if (PointInRectangle(rect1.min, rect2))
        {
            return true;
        }

        if (PointInRectangle(rect1.max, rect2))
        {
            return true;
        }

        if (PointInRectangle(new Vector2(rect1.min.x, rect1.max.y), rect2))
        {
            return true;
        }

        if (PointInRectangle(new Vector2(rect1.min.y, rect1.max.x), rect2))
        {
            return true;
        }

        if (PointInRectangle(rect2.min, rect1))
        {
            return true;
        }

        if (PointInRectangle(rect2.max, rect1))
        {
            return true;
        }

        if (PointInRectangle(new Vector2(rect2.min.x, rect2.max.y), rect1))
        {
            return true;
        }

        if (PointInRectangle(new Vector2(rect2.min.y, rect2.max.x), rect1))
        {
            return true;
        }

        return false;
    }

    public static bool PointInRectangle (Vector2Int pt, RectInt rect)
    {
        return pt.x >= rect.min.x && pt.x <= rect.max.x && pt.y >= rect.min.y && pt.y <= rect.max.y;
    }

    public static bool PointInRectangle (Vector2 pt, Rect rect)
    {
        return pt.x >= rect.min.x && pt.x <= rect.max.x && pt.y >= rect.min.y && pt.y <= rect.max.y;
    }

    public static bool PointInTriangle (Vector2 pt, Vector2 v1, Vector2 v2, Vector2 v3)
    {
        float alpha = ((v2.y - v3.y) * (pt.x - v3.x) + (v3.x - v2.x) * (pt.y - v3.y)) /
                ((v2.y - v3.y) * (v1.x - v3.x) + (v3.x - v2.x) * (v1.y - v3.y));
        float beta = ((v3.y - v1.y) * (pt.x - v3.x) + (v1.x - v3.x) * (pt.y - v3.y)) /
                ((v2.y - v3.y) * (v1.x - v3.x) + (v3.x - v2.x) * (v1.y - v3.y));
        float gamma = 1.0f - alpha - beta;

        return (alpha > 0 && beta > 0 && gamma > 0);
    }

    public static bool RectangleInRectangle (RectInt insideRect, RectInt onRect)
    {
        return PointInRectangle(insideRect.min, onRect) && PointInRectangle(insideRect.max, onRect);
    }

    public static Vector2Int RoundVectorToInt (Vector2 vector)
    {
        return new Vector2Int(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y));
    }

    public static int2 RoundVectorToInt (float2 vector)
    {
        return new int2((int)math.round(vector.x), (int)math.round(vector.y));
    }

    public static Vector2Int FloorVectorToInt (Vector2 vector)
    {
        return new Vector2Int(Mathf.FloorToInt(vector.x), Mathf.FloorToInt(vector.y));
    }

    public static Vector2Int CeilVectorToInt (Vector2 vector)
    {
        return new Vector2Int(Mathf.CeilToInt(vector.x), Mathf.CeilToInt(vector.y));
    }

    public static Vector2[] MovePoints (Vector2 movementValue, Vector2[] pointsToMove, bool roundToInt = false)
    {
        Vector2[] pointsMoved = new Vector2[pointsToMove.Length];

        for (int i = 0; i < pointsMoved.Length; i++)
        {
            pointsMoved[i] = MovePoint(movementValue, pointsToMove[i], roundToInt);
        }

        return pointsMoved;
    }

    public static Vector2 MovePoint (Vector2 movementValue, Vector2 pointToMove, bool roundToInt = false)
    {
        Vector2 pointMoved = pointToMove + movementValue;
        return roundToInt ? RoundVectorToInt(pointMoved) : pointMoved;
    }

    public static Rect MoveRect (Vector2 movementValue, Rect rectToMove, bool roundToInt = false)
    {
        rectToMove.position += movementValue;
        return rectToMove;
    }

    public static Vector2[] RotatePoints (Vector2 pivotPoint, float angleDegrees, Vector2[] pointsToRotate, Vector2 offsetValue, bool roundToInt = false)
    {
        Vector2[] pointsRotated = new Vector2[pointsToRotate.Length];

        for (int i = 0; i < pointsRotated.Length; i++)
        {
            pointsRotated[i] = RotatePoint(pivotPoint, angleDegrees, pointsToRotate[i], offsetValue, roundToInt);
        }

        return pointsRotated;
    }

    public static Vector2Int RotatePoint (Vector2Int pivotPoint, float angleDegrees, Vector2Int pointToRotate, Vector2Int offsetValue)
    {
        if (angleDegrees == 0.0f)
        {
            return pointToRotate;
        }

        Vector2Int pointTranslated = pointToRotate - pivotPoint;

        float s = Mathf.Sin(Mathf.Deg2Rad * angleDegrees);
        float c = Mathf.Cos(Mathf.Deg2Rad * angleDegrees);

        // rotate point
        Vector2 pointRotated = Vector2Int.zero;
        //pointRotated.x = Mathf.RoundToInt((pointTranslated.x * c) - (pointTranslated.y * s));
        //pointRotated.y = Mathf.RoundToInt((pointTranslated.x * s) + (pointTranslated.y * c));
        pointRotated.x = (pointTranslated.x * c) - (pointTranslated.y * s);
        pointRotated.y = (pointTranslated.x * s) + (pointTranslated.y * c);

        // translate point back
        pointRotated = pointRotated + pivotPoint + offsetValue;

        return RoundVectorToInt(pointRotated);
    }

    public static int2 RotatePoint (int2 pivotPoint, float angleDegrees, int2 pointToRotate, int2 offsetValue)
    {
        if (angleDegrees == 0.0f)
        {
            return pointToRotate;
        }

        int2 pointTranslated = pointToRotate - pivotPoint;

        float s = Mathf.Sin(Mathf.Deg2Rad * angleDegrees);
        float c = Mathf.Cos(Mathf.Deg2Rad * angleDegrees);

        // rotate point
        float2 pointRotated = int2.zero;
        //pointRotated.x = Mathf.RoundToInt((pointTranslated.x * c) - (pointTranslated.y * s));
        //pointRotated.y = Mathf.RoundToInt((pointTranslated.x * s) + (pointTranslated.y * c));
        pointRotated.x = (pointTranslated.x * c) - (pointTranslated.y * s);
        pointRotated.y = (pointTranslated.x * s) + (pointTranslated.y * c);

        // translate point back
        pointRotated = pointRotated + pivotPoint + offsetValue;

        return RoundVectorToInt(pointRotated);
    }

    /*
    public static Vector2Int RotatePointAndGetBottomLeft (Vector2Int pivotPoint, float angleDegrees, Vector2Int pointToRotate)
    {
        Vector2Int pointRotated = RotatePoint(pivotPoint, angleDegrees, pointToRotate);
        return new Vector2Int(Mathf.Min(pivotPoint.x, pointRotated.x), Mathf.Min(pivotPoint.y, pointRotated.y));
    }
    */

    public static Vector2 RotatePoint (Vector2 pivotPoint, float angleDegrees, Vector2 pointToRotate, Vector2 offsetValue, bool roundToInt = false)
    {
        if (angleDegrees == 0.0f)
        {
            return pointToRotate;
        }

        Vector2 pointTranslated = pointToRotate - pivotPoint;

        float s = Mathf.Sin(Mathf.Deg2Rad * angleDegrees);
        float c = Mathf.Cos(Mathf.Deg2Rad * angleDegrees);

        // rotate point
        Vector2 pointRotated = Vector2.zero;
        pointRotated.x = (pointTranslated.x * c) - (pointTranslated.y * s);
        pointRotated.y = (pointTranslated.x * s) + (pointTranslated.y * c);

        // translate point back
        pointRotated = pointRotated + pivotPoint + offsetValue;

        return roundToInt ? RoundVectorToInt(pointRotated) : pointRotated;
    }

    public static Rect RotateRect (Vector2 pivotPoint, float angleDegrees, Rect rectToRotate, Vector2 offsetValue, bool roundToInt = false)
    {
        if (angleDegrees == 0.0f)
        {
            return rectToRotate;
        }

        Vector2[] rectCorners = new Vector2[4];
        rectCorners[0] = rectToRotate.position;
        rectCorners[1] = new Vector2(rectToRotate.xMax, rectToRotate.yMin);
        rectCorners[2] = new Vector2(rectToRotate.xMin, rectToRotate.yMax);
        rectCorners[3] = new Vector2(rectToRotate.xMax, rectToRotate.yMax);

        Vector2 min = Vector2.one * int.MaxValue;
        Vector2 max = Vector2.one * int.MinValue;

        for (int i = 0; i < rectCorners.Length; i++)
        {
            Vector2 pointRotated = RotatePoint(pivotPoint, angleDegrees, rectCorners[i], offsetValue, roundToInt);

            if (min.x > pointRotated.x)
            {
                min.x = pointRotated.x;
            }
            if (min.y > pointRotated.y)
            {
                min.y = pointRotated.y;
            }

            if (max.x < pointRotated.x)
            {
                max.x = pointRotated.x;
            }
            if (max.y < pointRotated.y)
            {
                max.y = pointRotated.y;
            }
        }

        return roundToInt ? new Rect(RoundVectorToInt(min), RoundVectorToInt(max - min)) : new Rect(min, max - min);
    }

    public static RectInt RotateRect (Vector2Int pivotPoint, float angleDegrees, RectInt rectToRotate, Vector2Int offsetValue)
    {
        if (angleDegrees == 0.0f)
        {
            return rectToRotate;
        }

        Vector2Int[] rectCorners = new Vector2Int[4];
        rectCorners[0] = rectToRotate.position;
        rectCorners[1] = new Vector2Int(rectToRotate.xMax, rectToRotate.yMin);
        rectCorners[2] = new Vector2Int(rectToRotate.xMin, rectToRotate.yMax);
        rectCorners[3] = new Vector2Int(rectToRotate.xMax, rectToRotate.yMax);

        Vector2Int min = Vector2Int.one * int.MaxValue;
        Vector2Int max = Vector2Int.one * int.MinValue;

        for (int i = 0; i < rectCorners.Length; i++)
        {
            Vector2Int pointRotated = RotatePoint(pivotPoint, angleDegrees, rectCorners[i], offsetValue);

            if (min.x > pointRotated.x)
            {
                min.x = pointRotated.x;
            }
            if (min.y > pointRotated.y)
            {
                min.y = pointRotated.y;
            }

            if (max.x < pointRotated.x)
            {
                max.x = pointRotated.x;
            }
            if (max.y < pointRotated.y)
            {
                max.y = pointRotated.y;
            }
        }

        return new RectInt(min, max - min);
    }

    public static void RotateRect (int2 pivotPoint, float angleDegrees, int2 rectToRotatePos, int2 rectToRotateSize, int2 offsetValue, 
        out int2 rectPosition, out int2 rectSize)
    {
        rectPosition = rectToRotatePos;
        rectSize = rectToRotateSize;

        if (angleDegrees == 0.0f)
        {
            return;
        }

        int2[] rectCorners = new int2[4];
        rectCorners[0] = rectToRotatePos;
        rectCorners[1] = new int2(rectToRotatePos.x + rectToRotateSize.x, rectToRotatePos.y);
        rectCorners[2] = new int2(rectToRotatePos.x, rectToRotatePos.y + rectToRotateSize.y);
        rectCorners[3] = new int2(rectToRotatePos.x + rectToRotateSize.x, rectToRotatePos.y + rectToRotateSize.y);

        int2 min = new int2(1, 1) * int.MaxValue;
        int2 max = new int2(1, 1) * int.MinValue;

        for (int i = 0; i < rectCorners.Length; i++)
        {
            int2 pointRotated = RotatePoint(pivotPoint, angleDegrees, rectCorners[i], offsetValue);

            if (min.x > pointRotated.x)
            {
                min.x = pointRotated.x;
            }
            if (min.y > pointRotated.y)
            {
                min.y = pointRotated.y;
            }

            if (max.x < pointRotated.x)
            {
                max.x = pointRotated.x;
            }
            if (max.y < pointRotated.y)
            {
                max.y = pointRotated.y;
            }
        }

        rectPosition = min;
        rectSize = max - min;
    }

    public static Vector2 GetCornerPointFromRect (Rect rect, RectangleCorner corner)
    {
        switch(corner)
        {
            case RectangleCorner.BottomLeft: return rect.min;
            case RectangleCorner.BottomRight: return new Vector2(rect.max.x, rect.min.y);
            case RectangleCorner.TopRight: return rect.max;
            case RectangleCorner.TopLeft: return new Vector2(rect.min.x, rect.max.y);
            default: return Vector2.zero;
        }
    }

    public static Vector2 GetCornerPointFromBounds (Bounds bound, RectangleCorner corner)
    {
        return GetCornerPointFromRect(new Rect(bound.min, bound.size), corner);
    }

    public static RectInt GetSquareFromCenterPoint (Vector2Int point, ushort squareHalfEdge)
    {
        if (squareHalfEdge == 0)
        {
            return new RectInt(point, Vector2Int.zero);
        }

        return new RectInt(point - (Vector2Int.one * squareHalfEdge), Vector2Int.one * 2 * squareHalfEdge);
    }

    public static RectInt GetRectFromTwoPoints (Vector2Int p1, Vector2Int p2)
    {
        RectInt rect;

        if (p1.x <= p2.x && p1.y <= p2.y)
        {
            rect = new RectInt(p1, p2 - p1);
        }
        else if (p1.x <= p2.x && p1.y >= p2.y)
        {
            rect = new RectInt(new Vector2Int(p1.x, p2.y), new Vector2Int(p2.x - p1.x, p1.y - p2.y));
        }
        else if (p1.x >= p2.x && p1.y <= p2.y)
        {
            rect = new RectInt(new Vector2Int(p2.x, p1.y), new Vector2Int(p1.x - p2.x, p2.y - p1.y));
        }
        else
        {
            rect = new RectInt(p2, p1 - p2);
        }

        return rect;
    }

    public static void GetRectFromTwoPoints (int2 p1, int2 p2, out int2 init, out int2 size)
    {
        if (p1.x <= p2.x && p1.y <= p2.y)
        {
            init = p1;
            size = p2 - p1;
        }
        else if (p1.x <= p2.x && p1.y >= p2.y)
        {
            init = new int2(p1.x, p2.y);
            size = new int2(p2.x - p1.x, p1.y - p2.y);
        }
        else if (p1.x >= p2.x && p1.y <= p2.y)
        {
            init = new int2(p2.x, p1.y);
            size = new int2(p1.x - p2.x, p2.y - p1.y);
        }
        else
        {
            init = new int2(p2);
            size = new int2(p1 - p2);
        }

        size += new int2(1, 1);
    }

    public static Rect GetRectFromTwoPoints (Vector2 p1, Vector2 p2)
    {
        Rect rect;

        if (p1.x <= p2.x && p1.y <= p2.y)
        {
            rect = new Rect(p1, p2 - p1);
        }
        else if (p1.x <= p2.x && p1.y >= p2.y)
        {
            rect = new Rect(new Vector2(p1.x, p2.y), new Vector2(p2.x - p1.x, p1.y - p2.y));
        }
        else if (p1.x >= p2.x && p1.y <= p2.y)
        {
            rect = new Rect(new Vector2(p2.x, p1.y), new Vector2(p1.x - p2.x, p2.y - p1.y));
        }
        else
        {
            rect = new Rect(p2, p1 - p2);
        }

        return rect;
    }

    // get the nearest rectangle border from a point inside the rectangle
    // return true if its inside de rect and false otherwise
    public static bool GetNearestRectBorderFromPoint (Rect rect, Vector2 point, out RectangleBorder border)
    {
        border = RectangleBorder.Bottom;
        if (rect.Contains(point))
        {
            if (PointInTriangle(point, rect.min, rect.center, new Vector2(rect.max.x, rect.min.y)))
            {
                border = RectangleBorder.Bottom;
            }
            else if (PointInTriangle(point, rect.min, rect.center, new Vector2(rect.min.x, rect.max.y)))
            {
                border = RectangleBorder.Left;
            }
            else if (PointInTriangle(point, rect.max, rect.center, new Vector2(rect.min.x, rect.max.y)))
            {
                border = RectangleBorder.Top;
            }
            else
            {
                border = RectangleBorder.Right;
            }
        }
        else return false;

        return true;
    }

    public static Vector2Int ConvertPointToBorder(Vector2 point)
    {
        RectangleBorder rectBorder;

        Vector2Int pointInt = new Vector2Int((int)point.x, (int)point.y);

        GetNearestRectBorderFromPoint(new Rect(pointInt, Vector2.one), point, out rectBorder);

        // as the border is defined by +1 and +1 in both axis, sum one
        pointInt += Vector2Int.one;

        if (rectBorder == RectangleBorder.Top)
        {
            pointInt += Vector2Int.up;
        }
        else if (rectBorder == RectangleBorder.Right)
        {
            pointInt += Vector2Int.right;
        }

        if (rectBorder == RectangleBorder.Left || rectBorder == RectangleBorder.Right)
        {
            pointInt.x *= (-1);
        }

        return pointInt;
    }

    public static int GetEnabledBitsCount (ushort bits, int maxBits)
    {
        if (maxBits > 16)
        {
            return 0;
        }

        int bitsCount = 0;
        for (int i = 0; i < maxBits; i++)
        {
            if ((bits & (ushort)Mathf.Pow(2, i)) > 0)
            {
                bitsCount++;
            }
        }

        return bitsCount;
    }

    public static Vector2Int AdjustPositionForBottomLeftRotation (TileRotation rotation, Vector2Int rectDimension)
    {
        Vector2Int adjustedPos = Vector2Int.zero;

        if (rotation == TileRotation.Rot90)
        {
            adjustedPos.x = rectDimension.y;
        }
        else if (rotation == TileRotation.Rot180)
        {
            adjustedPos.x = rectDimension.x;
            adjustedPos.y = rectDimension.y;
        }
        else if (rotation == TileRotation.Rot270)
        {
            adjustedPos.y = rectDimension.x;
        }

        return adjustedPos;
    }

    public static Vector2Int GetTileNeighbourPosition (Vector2Int tilePosition, FourDirection neighbourDirection)
    {
        switch (neighbourDirection)
        {
            case FourDirection.North: return tilePosition + Vector2Int.up;
            case FourDirection.West:  return tilePosition + Vector2Int.left;
            case FourDirection.South: return tilePosition + Vector2Int.down;
            case FourDirection.East:  return tilePosition + Vector2Int.right;
            default: return tilePosition;
        }
    }

    public static Vector2Int GetTileNeighbourPosition (Vector2Int tilePosition, EightDirection neighbourDirection)
    {
        switch (neighbourDirection)
        {
            case EightDirection.North:
            case EightDirection.West:
            case EightDirection.South:
            case EightDirection.East: return GetTileNeighbourPosition(tilePosition, (FourDirection)neighbourDirection);
            case EightDirection.NorthWest: return tilePosition + new Vector2Int(-1, 1);
            case EightDirection.SouthWest: return tilePosition + new Vector2Int(-1, -1);
            case EightDirection.SouthEast: return tilePosition + new Vector2Int(1, -1);
            case EightDirection.NorthEast: return tilePosition + new Vector2Int(1, 1);
            default: return tilePosition;
        }
    }

    public static Vector2Int RotateMatrixIndex (Vector2Int matrixIndex, Vector2Int matrixSize, TileRotation matrixRotation)
    {
        Vector2Int matrixIndexRotated = matrixIndex;
        Vector2Int matrixSizeRotated = matrixSize;

        int rotationTimes = Mathf.RoundToInt(((int)matrixRotation / 90.0f));

        // each for loop rotates 90 degrees
        for (int i = 0; i < rotationTimes; i++)
        {
            matrixIndexRotated = new Vector2Int(matrixIndexRotated.y, matrixSizeRotated.x - matrixIndexRotated.x);
            matrixSizeRotated = new Vector2Int(matrixSizeRotated.y, matrixSizeRotated.x);
        }

        return matrixIndexRotated;
    }

    public static Vector2Int[,] GetRotatedMatrixIndexer (Vector2Int matrixSize, TileRotation matrixRotation)
    {
        Vector2Int[,] matrixIndexer = new Vector2Int[matrixSize.x, matrixSize.y];

        if (matrixRotation == TileRotation.Rot0 || matrixRotation == TileRotation.Rot180)
        {
            matrixIndexer = new Vector2Int[matrixSize.x, matrixSize.y];

            Vector2Int matrixDim = new Vector2Int(matrixIndexer.GetLength(0), matrixIndexer.GetLength(1));

            for (int i = 0; i < matrixDim.x; i++)
            {
                for (int j = 0; j < matrixDim.y; j++)
                {
                    matrixIndexer[i, j] = matrixRotation == TileRotation.Rot0 ? new Vector2Int(i, j) : new Vector2Int(matrixDim.x - i - 1, matrixDim.y - j - 1);
                }
            }

        }
        else
        {
            matrixIndexer = new Vector2Int[matrixSize.y, matrixSize.x];

            Vector2Int matrixDim = new Vector2Int(matrixIndexer.GetLength(0), matrixIndexer.GetLength(1));

            for (int i = 0; i < matrixDim.x; i++)
            {
                for (int j = 0; j < matrixDim.y; j++)
                {
                    matrixIndexer[i, j] = matrixRotation == TileRotation.Rot90 ? new Vector2Int(matrixDim.y - j - 1, i) : new Vector2Int(j, matrixDim.x - i - 1);
                }
            }

        }

        return matrixIndexer;
    }

    public static int2[,] GetRotatedMatrixIndexer (int2 matrixSize, TileRotation matrixRotation)
    {
        int2[,] matrixIndexer = new int2[matrixSize.x, matrixSize.y];

        if (matrixRotation == TileRotation.Rot0 || matrixRotation == TileRotation.Rot180)
        {
            matrixIndexer = new int2[matrixSize.x, matrixSize.y];

            int2 matrixDim = new int2(matrixIndexer.GetLength(0), matrixIndexer.GetLength(1));

            for (int i = 0; i < matrixDim.x; i++)
            {
                for (int j = 0; j < matrixDim.y; j++)
                {
                    matrixIndexer[i, j] = matrixRotation == TileRotation.Rot0 ? new int2(i, j) : new int2(matrixDim.x - i - 1, matrixDim.y - j - 1);
                }
            }

        }
        else
        {
            matrixIndexer = new int2[matrixSize.y, matrixSize.x];

            int2 matrixDim = new int2(matrixIndexer.GetLength(0), matrixIndexer.GetLength(1));

            for (int i = 0; i < matrixDim.x; i++)
            {
                for (int j = 0; j < matrixDim.y; j++)
                {
                    matrixIndexer[i, j] = matrixRotation == TileRotation.Rot90 ? new int2(matrixDim.y - j - 1, i) : new int2(j, matrixDim.x - i - 1);
                }
            }

        }

        return matrixIndexer;
    }

    /*
    public static Vector2Int FromRotationMatrixIndex (Vector2Int matrixIndex, Vector2Int matrixSize, TileRotation matrixRotation)
    {
        switch (matrixRotation)
        {
            case TileRotation.Rot0: return matrixIndex;
            case TileRotation.Rot90: return new Vector2Int(matrixIndex.y, matrixSize.y - 1 - matrixIndex.x);
            case TileRotation.Rot180: return new Vector2Int(matrixSize.x - 1 - matrixIndex.x, matrixSize.y - 1 - matrixIndex.y);
            case TileRotation.Rot270: return new Vector2Int(matrixSize.x - matrixIndex.y, matrixIndex.x);
            default: return matrixIndex;
        }
    }
    */

    public static bool GetRayPointIntersectRectPlane (Plane plane, Ray ray, out float rayDistance, out Vector3 intersectionPoint)
    {
        intersectionPoint = Vector3.zero;

        if (plane.Raycast(ray, out rayDistance))
        {
            intersectionPoint = ray.GetPoint(rayDistance);
            return true;
        }

        return false;
    }


    public static bool IsTransformHit ()
    {



        return false;
    }

    public static bool Approximately (float a, float b, float tolerance = 1e-5f)
    {
        return math.abs(a - b) <= tolerance;
    }

    public static float CrossProduct2D (float2 a, float2 b)
    {
        return a.x * b.y - b.x * a.y;
    }

    public static void Swap<T> (ref T lhs, ref T rhs)
    {
        T temp = lhs;
        lhs = rhs;
        rhs = temp;
    }

    public static bool IntersectLineSegments2D (float2 p1start, float2 p1end, float2 p2start, float2 p2end,
        out float2 intersection)
    {
        // Consider:
        //   p1start = p
        //   p1end = p + r
        //   p2start = q
        //   p2end = q + s
        // We want to find the intersection point where :
        //  p + t*r == q + u*s
        // So we need to solve for t and u
        var p = p1start;
        var r = p1end - p1start;
        var q = p2start;
        var s = p2end - p2start;
        var qminusp = q - p;

        float cross_rs = CrossProduct2D(r, s);

        if (Approximately(cross_rs, 0f))
        {
            // Parallel lines
            if (Approximately(CrossProduct2D(qminusp, r), 0f))
            {
                // Co-linear lines, could overlap
                float rdotr = Vector2.Dot(r, r);
                float sdotr = Vector2.Dot(s, r);
                // this means lines are co-linear
                // they may or may not be overlapping
                float t0 = Vector2.Dot(qminusp, r / rdotr);
                float t1 = t0 + sdotr / rdotr;
                if (sdotr < 0)
                {
                    // lines were facing in different directions so t1 > t0, swap to simplify check
                    Swap(ref t0, ref t1);
                }

                if (t0 <= 1 && t1 >= 0)
                {
                    // Nice half-way point intersection
                    float t = Mathf.Lerp(Mathf.Max(0, t0), Mathf.Min(1, t1), 0.5f);
                    intersection = p + t * r;
                    return true;
                }
                else
                {
                    // Co-linear but disjoint
                    intersection = Vector2.zero;
                    return false;
                }
            }
            else
            {
                // Just parallel in different places, cannot intersect
                intersection = Vector2.zero;
                return false;
            }
        }
        else
        {
            // Not parallel, calculate t and u
            float t = CrossProduct2D(qminusp, s) / cross_rs;
            float u = CrossProduct2D(qminusp, r) / cross_rs;
            if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
            {
                intersection = p + t * r;
                return true;
            }
            else
            {
                // Lines only cross outside segment range
                intersection = Vector2.zero;
                return false;
            }
        }
    }
}