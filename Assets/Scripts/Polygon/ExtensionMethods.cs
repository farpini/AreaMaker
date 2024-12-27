using UnityEngine;
using System;
using Unity.Mathematics;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.IO;
using Unity.Collections;

public static class EnumExtensions
{
    // extension for enumerations
    public static T Next<T> (this T src) where T : struct
    {
        if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

        T[] Arr = (T[])Enum.GetValues(src.GetType());
        int j = Array.IndexOf<T>(Arr, src) + 1;
        return (Arr.Length == j) ? Arr[0] : Arr[j];
    }

    public static T Previous<T> (this T src) where T : struct
    {
        if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

        T[] Arr = (T[])Enum.GetValues(src.GetType());
        int j = Array.IndexOf<T>(Arr, src) - 1;
        return (j < 0) ? Arr[Arr.Length - 1] : Arr[j];
    }

    public static T Next<T> (this T src, int nextTimes) where T : struct
    {
        if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

        T[] Arr = (T[])Enum.GetValues(src.GetType());
        int j = Array.IndexOf<T>(Arr, src) + nextTimes;
        return Arr[j % Arr.Length];
    }

    public static T Previous<T> (this T src, int nextTimes) where T : struct
    {
        if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

        T[] Arr = (T[])Enum.GetValues(src.GetType());
        int j = Array.IndexOf<T>(Arr, src) - nextTimes;
        return Arr[(j < 0 ? Arr.Length : 0) + (j % Arr.Length)];
    }

    public static class Enums<TEnum> where TEnum : struct, IComparable, IFormattable, IConvertible
    {
        static Enums ()
        {
            if (!typeof(TEnum).IsEnum)
            {
                throw new InvalidOperationException();
            }
        }
    }
}

public static class SortedListExtensions
{
    ///Add item to sortedList (numeric key) to next available key item, and return key
    public static int AddNext<T> (this SortedList<int, T> sortedList, T item)
    {
        int key = 1; // Make it 0 to start from Zero based index
        int count = sortedList.Count;

        int counter = 0;
        do
        {
            if (count == 0) break;
            int nextKeyInList = sortedList.Keys[counter++];

            if (key != nextKeyInList) break;

            key = nextKeyInList + 1;

            if (count == 1 || counter == count) break;


            if (key != sortedList.Keys[counter])
                break;

        } while (true);

        sortedList.Add(key, item);
        return key;
    }

    public static int GetNext<T> (this SortedList<int, T> sortedList)
    {
        int key = 1; // Make it 0 to start from Zero based index
        int count = sortedList.Count;

        int counter = 0;
        do
        {
            if (count == 0) break;
            int nextKeyInList = sortedList.Keys[counter++];

            if (key != nextKeyInList) break;

            key = nextKeyInList + 1;

            if (count == 1 || counter == count) break;


            if (key != sortedList.Keys[counter])
                break;

        } while (true);

        return key;
    }
}

public static class GameObjectExtensions
{
    public static GameObject FindChildrenObject (this GameObject parent, string name)
    {
        Transform[] trs = parent.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in trs)
        {
            if (t.name == name)
            {
                return t.gameObject;
            }
        }
        return null;
    }
}

public static class ColorExtensions
{
    // Example: "#ff000099".ToColor() red with alpha ~50%
    // Example: "ffffffff".ToColor() white with alpha 100%
    // Example: "00ff00".ToColor() green with alpha 100%
    // Example: "0000ff00".ToColor() blue with alpha 0%
    public static Color ToColor (this string color)
    {
        if (color.StartsWith("#", StringComparison.InvariantCulture))
        {
            color = color.Substring(1); // strip #
        }

        if (color.Length == 6)
        {
            color += "FF"; // add alpha if missing
        }

        var hex = Convert.ToUInt32(color, 16);
        var r = ((hex & 0xff000000) >> 0x18) / 255f;
        var g = ((hex & 0xff0000) >> 0x10) / 255f;
        var b = ((hex & 0xff00) >> 8) / 255f;
        var a = ((hex & 0xff)) / 255f;

        return new Color(r, g, b, a);
    }

    public static float4 ToFloat4 (this Color color)
    {
        return new() { x = color.r, y = color.g, z = color.b, w = color.a };
    }
}

public static class VectorsExtensions
{
    
    public static Vector3 WithY (this Vector3 v3, float? y = null)
    {
        return new Vector3(v3.x, y ?? v3.y, v3.z);
    }
    public static Vector3 WithZ (this Vector3 v3, float? z = null)
    {
        return new Vector3(v3.x, v3.y, z ?? v3.z);
    }

    // extensions for vector2
    public static Vector3 With (this Vector2 v3, Vector2 v2)
    {
        return new Vector3(v2.x, 0.0f, v2.y);
    }
    public static Vector2 ToXY (this Vector3 v)
    {
        return new Vector2(v.x, v.z);
    }
    public static Vector2 LerpVec (this Vector2 v2, Vector2 a, Vector2 b, Vector2 t)
    {
        return new Vector2(Mathf.InverseLerp(a.x, b.x, t.x), Mathf.InverseLerp(a.y, b.y, t.y));
    }
    public static Vector2 DirectionTo (this Vector2 source, Vector2 destination)
    {
        return Vector3.Normalize(destination - source);
    }
    public static Vector2Int ToInt (this Vector2 source)
    {
        return new Vector2Int((int)source.x, (int)source.y);
    }
    public static Vector2Int RoundToInt (this Vector2 source)
    {
        return new Vector2Int(Mathf.RoundToInt(source.x), Mathf.RoundToInt(source.y));
    }
    public static Vector2Int FloorToInt (this Vector2 source)
    {
        return new Vector2Int(Mathf.FloorToInt(source.x), Mathf.FloorToInt(source.y));
    }
    public static Vector2Int CeilToInt (this Vector2 source)
    {
        return new Vector2Int(Mathf.CeilToInt(source.x), Mathf.CeilToInt(source.y));
    }
    public static bool SameValue (this Vector2Int source, int3 compare)
    {
        return source.x == compare.x && source.y == compare.y;
    }

    // extensions for vector3
    public static Vector3 With (this Vector3 v3, float? x = null, float? y = null, float? z = null)
    {
        return new Vector3(x ?? v3.x, y ?? v3.y, z ?? v3.z);
    }
    public static Vector3 With (this Vector3 v3, Vector2 v2)
    {
        return new Vector3(v2.x, v2.y, v3.z);
    }
    public static Vector3 ToXZ (this Vector2 aVec)
    {
        return new Vector3(aVec.x, 0, aVec.y);
    }
    public static Vector3 ToXZ (this Vector2 aVec, float aYValue)
    {
        return new Vector3(aVec.x, aYValue, aVec.y);
    }

    // extensions for vector3int
    public static Vector3Int Add (this Vector3Int v3, int val)
    {
        return new Vector3Int(v3.x + val, v3.y + val, v3.z + val);
    }

    // extensions for rects
    public static RectInt ToInt (this Rect rect)
    {
        return new RectInt(rect.position.ToInt(), rect.size.ToInt());
    }

    public static RectInt RountToInt (this Rect rect)
    {
        return new RectInt(rect.position.RoundToInt(), rect.size.RoundToInt());
    }

    public static RectInt Mult (this RectInt rect, int multValue)
    {
        return new RectInt(rect.position * multValue, rect.size * multValue);
    }

    public static Rect Mult (this Rect rect, int multValue)
    {
        return new Rect(rect.position * multValue, rect.size * multValue);
    }
}

public static class MathematicsExtensions
{
    public static quaternion To_quaternion (this Quaternion source)
    {
        return new quaternion(source.x, source.y, source.z, source.w);
    }

    public static Quaternion To_Quaternion (this quaternion source)
    {
        return new Quaternion(source.value.x, source.value.y, source.value.z, source.value.w);
    }

    public static Vector2Int ToVector2Int (this int2 source)
    {
        return new Vector2Int(source.x, source.y);
    }

    public static bool IsWithin (this int value, int minimum, int maximum)
    {
        return value >= minimum && value <= maximum;
    }

    public static int2 RoundToInt2 (this Vector2 source)
    {
        return (int2)math.round(source);
    }

    public static int2 Toint2 (this Vector2Int source)
    {
        return new int2(source.x, source.y);
    }

    public static float2 Tofloat2 (this Vector2 source)
    {
        return new float2(source.x, source.y);
    }

    public static int2 North (this int2 source)
    {
        return new int2(source.x, source.y + 1);
    }

    public static int2 South (this int2 source)
    {
        return new int2(source.x, source.y - 1);
    }

    public static int2 West (this int2 source)
    {
        return new int2(source.x - 1, source.y);
    }

    public static int2 East (this int2 source)
    {
        return new int2(source.x + 1, source.y);
    }

    public static int2 NorthWest (this int2 source)
    {
        return new int2(source.x - 1, source.y + 1);
    }

    public static int2 NorthEast (this int2 source)
    {
        return new int2(source.x + 1, source.y + 1);
    }

    public static int2 SouthWest (this int2 source)
    {
        return new int2(source.x - 1, source.y - 1);
    }

    public static int2 SouthEast (this int2 source)
    {
        return new int2(source.x + 1, source.y - 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float2 MoveTowards (this float2 current, float2 target, float maxDelta)
    {
        float num = target.x - current.x;
        float num2 = target.y - current.y;
        float num3 = num * num + num2 * num2;
        if (num3 == 0f || (maxDelta >= 0f && num3 <= maxDelta * maxDelta))
        {
            return target;
        }

        float num4 = (float)math.sqrt(num3);
        return new float2(current.x + num / num4 * maxDelta, current.y + num2 / num4 * maxDelta);
    }

    public static float3 WithY (this float3 current, float yValue)
    {
        return new float3(current.x, yValue, current.z);
    }

    public static float3 WithZ (this float3 current, float zValue)
    {
        return new float3(current.x, current.y, zValue);
    }

    public static int2 ToWaypoint (this float3 current, float waypointWidth)
    {
         return (int2)new float2(math.round(current.xz / waypointWidth));
    }

    public static int2 ToWaypoint (this float2 current, float waypointWidth)
    {
        return (int2)new float2(math.round(current / waypointWidth));
    }

    public static float2 WithAddX (this float2 current, float xValue)
    {
        return new float2(current.x + xValue, current.y);
    }

    public static float2 WithAddY (this float2 current, float yValue)
    {
        return new float2(current.x, current.y + yValue);
    }

    public static float4 WithAddX (this float4 current, float xValue)
    {
        return new float4(current.x + xValue, current.y, current.z, current.w);
    }

    public static float4 WithAddY (this float4 current, float yValue)
    {
        return new float4(current.x, current.y + yValue, current.z, current.w);
    }

    public static float4 WithAddZ (this float4 current, float zValue)
    {
        return new float4(current.x, current.y, current.z+ zValue, current.w);
    }

    public static float4 WithAddW (this float4 current, float WValue)
    {
        return new float4(current.x, current.y, current.z, current.w + WValue);
    }

    public static Color ToColor (this float4 color)
    {
        return new() { r = color.x, g = color.y, b = color.z, a = color.w };
    }

    public static int2 RoundVectorToInt (this float2 vector)
    {
        return new int2((int)math.round(vector.x), (int)math.round(vector.y));
    }

    public static int2 RotatePoint (this int2 pointToRotate, int2 pivotPoint, int2 offsetValue, float angleDegrees)
    {
        if (angleDegrees == 0.0f)
        {
            return pointToRotate;
        }

        int2 pointTranslated = pointToRotate - pivotPoint;

        float s = math.sin(Mathf.Deg2Rad * angleDegrees);
        float c = math.cos(Mathf.Deg2Rad * angleDegrees);

        // rotate point
        float2 pointRotated = int2.zero;
        //pointRotated.x = Mathf.RoundToInt((pointTranslated.x * c) - (pointTranslated.y * s));
        //pointRotated.y = Mathf.RoundToInt((pointTranslated.x * s) + (pointTranslated.y * c));
        pointRotated.x = (pointTranslated.x * c) - (pointTranslated.y * s);
        pointRotated.y = (pointTranslated.x * s) + (pointTranslated.y * c);

        // translate point back
        pointRotated = pointRotated + pivotPoint + offsetValue;

        return pointRotated.RoundVectorToInt();
    }
}