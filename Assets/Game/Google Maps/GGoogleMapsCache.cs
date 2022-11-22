using Mapbox.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GGoogleMapsCache
{

    public Dictionary<string, GGoogleMapsPOI> GoogleMapPOIs = new();

    public void PopulateWithNearbySearchResponse(GGoogleMapsResponses.NearbySearchResponse nearby)
    {
        for (int i = 0; i < nearby.Results.Count; i++)
        {
            GGoogleMapsPOI poi = nearby.Results[i];
            poi.IsDetailed = false;
            GoogleMapPOIs[poi.PlaceID] = poi;
            Debug.Log($"{poi.Name} ({poi.PlaceID})");
        }
    }

    public void DebugAllPOINames()
    {
        foreach (KeyValuePair<string, GGoogleMapsPOI> pair in GoogleMapPOIs)
        {
            Debug.Log($"{pair.Value.Name} ({pair.Key})");
        }
    }

}
