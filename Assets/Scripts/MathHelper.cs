using UnityEngine;
using System;

public static class MathHelper
{
    public static float VectorDistance(Vector3 _v, Vector3 _w)
    {
        return (float)Math.Sqrt( Math.Pow( (_w.x - _v.x), 2) + Math.Pow( (_w.y - _v.y), 2) + Math.Pow((_w.z - _v.z), 2) );
    }

    public static float DotProduct(Vector3 _v, Vector3 _w)
    {
        return ((_v.x) * (_w.x)) + ((_v.y) * (_w.y)) + ((_v.z) * (_w.z));
    }

    public static Vector3 CrossProduct(Vector3 _v, Vector3 _w)
    {
        return new Vector3( (_v.y * _w.z) - (_v.z * _w.y), (_v.z * _w.x) - (_v.x * _w.z), (_v.x * _w.y) - (_v.z * _w.x));
    }

    public static float AngleBetween(Vector3 _v, Vector3 _w)
    {
        return ((float)Math.Acos( DotProduct(_v, _w) / (_v.magnitude * _w.magnitude))) * Mathf.Rad2Deg;
    }
}