using Mapbox.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Consists of 4 GGoogleMapsQueryLocation. This will be used to determine what is displayed and what is not
/// </summary>
public class GGoogleMapsQueryCluster
{

    public GGoogleMapsQueryLocation Location1 = new();
    public GGoogleMapsQueryLocation Location2 = new();
    public GGoogleMapsQueryLocation Location3 = new();
    public GGoogleMapsQueryLocation Location4 = new();

    public IEnumerable<GGoogleMapsQueryLocation> Locations()
    {
        yield return Location1;
        yield return Location2;
        yield return Location3;
        yield return Location4;
    }

    private void ClearCluster()
    {
        Location1 = null;
        Location2 = null;
        Location3 = null;
        Location4 = null;
    }

    public void PrepareQueryCluster(Vector2d[] queryLocations)
    {
        ClearCluster();

    }

}
