using Mapbox.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GGoogleMapsCache
{

    private static G G => G.Instance;

    public Dictionary<string, GGoogleMapsPOI> GoogleMapPOIs = new();

    public void PopulateWithNearbySearchResponse(GGoogleMapsResponses.NearbySearchResponse nearby)
    {
        for (int i = 0; i < nearby.Results.Count; i++)
        {
            GGoogleMapsPOI poi = nearby.Results[i];
            poi.IsDetailed = false;
            string placeID = poi.PlaceID;
            if (GoogleMapPOIs.ContainsKey(poi.PlaceID))
            {
                GoogleMapPOIs[placeID] = poi;
            }
            else
            {
                GoogleMapPOIs.Add(placeID, poi);

                Debug.Log($"D:{G.Haversine(G.Location.X, G.Location.Y, poi.Geometry.Location.Latitude, poi.Geometry.Location.Longitude)} {poi.Name} ({poi.PlaceID})");
                //test only
                G.POIManager.SpawnPOI(poi);
            }

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
