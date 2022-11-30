using Mapbox.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Is the processed result of a NearbySearch query, consists of up to 60 GGoogleMapsPOI
/// </summary>
public class GGoogleMapsQueryLocation
{

    public Vector2d Location;
    public List<GGoogleMapsPOI> POIs = new();

}
