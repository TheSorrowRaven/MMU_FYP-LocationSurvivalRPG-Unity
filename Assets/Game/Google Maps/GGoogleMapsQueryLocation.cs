using Mapbox.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GGoogleMapsResponses;

/// <summary>
/// Is the processed result of a NearbySearch query, consists of up to 60 GGoogleMapsPOI
/// </summary>
public class GGoogleMapsQueryLocation
{

    public string nextPageToken = null; //Temp use only during requests
    public bool hasFailed = false;

    public Vector2d Location;
    public List<GGoogleMapsPOI> POIs = new();

    public GGoogleMapsQueryLocation(Vector2d location)
    {
        Location = location;
    }

    //TASK
    public void AddPOIsWithNearbySearchResponse(NearbySearchResponse nearby)
    {
        for (int i = 0; i < nearby.Results.Count; i++)
        {
            GGoogleMapsPOI poi = nearby.Results[i];
            poi.IsDetailed = false;
            POIs.Add(poi);
        }
    }

}
