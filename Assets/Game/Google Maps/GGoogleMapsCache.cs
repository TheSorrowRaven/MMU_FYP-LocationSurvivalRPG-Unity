using Mapbox.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GGoogleMapsCache
{

    private static G G => G.Instance;

    public Dictionary<string, GGoogleMapsPOI> PlaceIDToGoogleMapPOI = new();

    public Dictionary<Vector2ds, GGoogleMapsQueryLocation> LocationToQueryLocation = new();
    public Dictionary<Vector2ds, Task<GGoogleMapsQueryLocation>> PendingQueryLocations = new();

    public void PopulateWithNearbySearchResponse(GGoogleMapsResponses.NearbySearchResponse nearby)
    {
        for (int i = 0; i < nearby.Results.Count; i++)
        {
            GGoogleMapsPOI poi = nearby.Results[i];
            poi.IsDetailed = false;
            string placeID = poi.PlaceID;
            if (PlaceIDToGoogleMapPOI.ContainsKey(poi.PlaceID))
            {
                PlaceIDToGoogleMapPOI[placeID] = poi;
            }
            else
            {
                PlaceIDToGoogleMapPOI.Add(placeID, poi);

                Debug.Log($"D:{G.Haversine(G.Location.X, G.Location.Y, poi.Geometry.Location.Latitude, poi.Geometry.Location.Longitude)} {poi.Name} ({poi.PlaceID})");
                //test only
                G.POIManager.SpawnPOI(poi);
            }

        }
    }

    public void DebugAllPOINames()
    {
        foreach (KeyValuePair<string, GGoogleMapsPOI> pair in PlaceIDToGoogleMapPOI)
        {
            Debug.Log($"{pair.Value.Name} ({pair.Key})");
        }
    }

    /// <summary>
    /// If this returns null, it is not cached/querying
    /// </summary>
    public async Task<GGoogleMapsQueryLocation> TryGetCachedGGoogleMapsQueryLocation(Vector2ds location)
    {
        if (LocationToQueryLocation.TryGetValue(location, out GGoogleMapsQueryLocation queryLocation))
        {
            return queryLocation;
        }
        if (PendingQueryLocations.TryGetValue(location, out Task<GGoogleMapsQueryLocation> queryLocationTask))
        {
            return await queryLocationTask;
        }
        return null;
    }

    public void AddQueryLocationTask(Vector2ds location, Task<GGoogleMapsQueryLocation> queryLocation)
    {
        PendingQueryLocations.Add(location, queryLocation);
    }
    public void FinishQueryLocationTask(Vector2ds location, GGoogleMapsQueryLocation queryLocation)
    {
        PendingQueryLocations.Remove(location);
        LocationToQueryLocation.Add(location, queryLocation);
    }

}
