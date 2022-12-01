using Mapbox.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Bounds2d : IEqualityComparer<Bounds2d>
{

    public double minX;
    public double maxX;
    public double minY;
    public double maxY;

    public Bounds2d(double minX, double maxX, double minY, double maxY)
    {
        this.minX = minX;
        this.maxX = maxX;
        this.minY = minY;
        this.maxY = maxY;
    }

    public bool Within(Vector2d pos)
    {
        return pos.x >= minX && pos.x <= maxX && pos.y >= minY && pos.y <= maxY;
    }

    public bool Equals(Bounds2d a, Bounds2d b)
    {
        return a.minX == b.minX && a.maxX == b.maxX && a.minY == b.minY && a.maxY == b.maxY;
    }

    public override bool Equals(object obj)
    {
        return obj is Bounds2d v && Equals(this, v);
    }

    public int GetHashCode(Bounds2d obj)
    {
        return obj.GetHashCode();
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }


}
