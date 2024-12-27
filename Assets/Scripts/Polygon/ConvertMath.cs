using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ConversionLib
{
    using ClipperLib;
    using LibTessDotNet;
    using Unity.Collections;
    using Unity.Mathematics;

    public static class ConvertMath
    {
        public static List<Vector2Int> ConvertIntPointArrayToVector2IntList (IntPoint[] arr)
        {
            List<Vector2Int> lstOut = new List<Vector2Int>();
            for (int i = 0; i < arr.Length; i++)
            {
                lstOut.Add(IntPoint.ToVector2Int(arr[i]));
            }
            return lstOut;
        }

        public static Vector2Int[] ConvertIntPointArrayToVector2IntArray (IntPoint[] arr)
        {
            Vector2Int[] arrOut = new Vector2Int[arr.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                arrOut[i] = IntPoint.ToVector2Int(arr[i]);
            }
            return arrOut;
        }

        public static IntPoint[] ConvertVector2IntArrToIntPointArray (Vector2Int[] arr)
        {
            IntPoint[] arrOut = new IntPoint[arr.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                arrOut[i] = IntPoint.ToPoint(arr[i]);
            }
            return arrOut;
        }

        public static Vector2[] ConvertVector2IntArrToVector2Arr (Vector2Int[] arr)
        {
            Vector2[] arrOut = new Vector2[arr.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                arrOut[i] = new Vector2(arr[i].x, arr[i].y);
            }
            return arrOut;
        }

        public static Vector2[] ConvertIntPointListToVector2Arr (List<IntPoint> lst)
        {
            Vector2[] arrOut = new Vector2[lst.Count];
            for (int i = 0; i < arrOut.Length; i++)
            {
                arrOut[i] = IntPoint.ToVector2(lst[i]);
            }
            return arrOut;
        }

        public static List<IntPoint> ConvertVector2ToIntPointList (Vector2[] arr)
        {
            List<IntPoint> lstOut = new List<IntPoint>();
            for (int i = 0; i < arr.Length; i++)
            {
                lstOut.Add(IntPoint.ToPoint(arr[i]));
            }
            return lstOut;
        }

        public static List<IntPoint> ConvertVector2ToIntPointList (Vector2Int[] arr)
        {
            List<IntPoint> lstOut = new List<IntPoint>();
            for (int i = 0; i < arr.Length; i++)
            {
                lstOut.Add(IntPoint.ToPoint(arr[i]));
            }
            return lstOut;
        }

        public static List<IntPoint> ConvertVector2ToIntPointList (List<Vector2Int> lst)
        {
            List<IntPoint> lstOut = new List<IntPoint>();
            for (int i = 0; i < lst.Count; i++)
            {
                lstOut.Add(IntPoint.ToPoint(lst[i]));
            }
            return lstOut;
        }

        public static List<IntPoint> ConvertInt2ToIntPointList (NativeArray<int2> arr)
        {
            List<IntPoint> lstOut = new List<IntPoint>();
            for (int i = 0; i < arr.Length; i++)
            {
                lstOut.Add(IntPoint.ToPoint(arr[i]));
            }
            return lstOut;
        }

        public static ContourVertex[] ConvertVector2ArrToContourVertexArr (Vector2[] arr, int z = 0)
        {
            ContourVertex[] arrOut = new ContourVertex[arr.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                arrOut[i] = new ContourVertex() { Position = new Vec3 { X = arr[i].x, Y = arr[i].y, Z = z } };
            }

            return arrOut;
        }
    }
}