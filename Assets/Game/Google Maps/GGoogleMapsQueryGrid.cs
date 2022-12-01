using Mapbox.Unity.Utilities;
using Mapbox.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GGoogleMapsQueryGrid
{
    public static readonly GGoogleMapsQueryGrid Instance = new();

    private static GameSettings GameSettings => GameSettings.Instance;

    public static Vector2d[] GetQueryLocationsFromPosition(Vector2d coord)
    {
        double diameter = GameSettings.LatLonDistanceQueryRadius * 2;
        double minX = coord.x - (coord.x % diameter);
        double minY = coord.y - (coord.y % diameter);
        double maxX = minX + diameter;
        double maxY = minY + diameter;
        return new Vector2d[]
        {
            new Vector2d(minX, minY),
            new Vector2d(minX, maxY),
            new Vector2d(maxX, maxY),
            new Vector2d(maxX, minY),
        };
    }

    public static Bounds2d GetQueryLocationsFromPosition(Vector2d coord, Vector2d[] buffer)
    {
        double diameter = GameSettings.LatLonDistanceQueryRadius * 2;
        double minX = coord.x - (coord.x % diameter);
        double minY = coord.y - (coord.y % diameter);
        double maxX = minX + diameter;
        double maxY = minY + diameter;
        buffer[0] = new Vector2d(minX, minY);
        buffer[1] = new Vector2d(minX, maxY);
        buffer[2] = new Vector2d(maxX, maxY);
        buffer[3] = new Vector2d(maxX, minY);
        return new(minX, maxX, minY, maxY);
    }
}
