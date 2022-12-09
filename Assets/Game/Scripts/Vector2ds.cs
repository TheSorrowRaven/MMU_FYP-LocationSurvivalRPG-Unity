using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Utils;
using System;
using UnityEngine.Assertions.Comparers;

/// <summary>
/// Vector2d Serialiazble and Comparable
/// </summary>
public struct Vector2ds : IEqualityComparer<Vector2ds>
{

    public double x;
    public double y;

    public decimal Xdc => (decimal)Math.Round(x, 5);
    public decimal Ydc => (decimal)Math.Round(y, 5);

    public Vector2ds(double x, double y)
    {
        this.x = x;
        this.y = y;
    }

    public bool Equals(Vector2ds a, Vector2ds b)
    {
        //Debug.LogWarning($"{a.Xdc}&{b.Xdc}={a.Xdc == b.Xdc}, {a.Ydc}&{b.Ydc}={a.Ydc == b.Ydc}");
        return a.Xdc == b.Xdc && a.Ydc == b.Ydc;
    }

    public int GetHashCode(Vector2ds obj)
    {
        return obj.GetHashCode();
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Xdc, Ydc);
    }

    public override bool Equals(object obj)
    {
        return obj is Vector2ds v && Equals(this, v);
    }

    public static implicit operator Vector2d(Vector2ds vector2ds)
    {
        return new(vector2ds.x, vector2ds.y);
    }
    public static implicit operator Vector2ds(Vector2d vector2d)
    {
        return new(vector2d.x, vector2d.y);
    }

    public override string ToString()
    {
        return $"{x}, {y}";
    }

}
