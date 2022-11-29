using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Consists of 4 GGoogleMapsQueryLocation
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

}
