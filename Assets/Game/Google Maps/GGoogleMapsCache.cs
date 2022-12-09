using Mapbox.Utils;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GGoogleMapsCache
{

    private static G G => G.Instance;

    public ConcurrentDictionary<string, GGoogleMapsPOI> PlaceIDToGoogleMapPOI = new();

    public ConcurrentDictionary<Vector2ds, GGoogleMapsQueryLocation> LocationToQueryLocation = new();
    public ConcurrentDictionary<Vector2ds, Task<GGoogleMapsQueryLocation>> PendingQueryLocations = new();


    //TASK
    public void PopulatePOIsWithQueryLocation(GGoogleMapsQueryLocation queryLocation)
    {
        foreach (GGoogleMapsPOI poi in queryLocation.DensityFilteredPOIs)
        {
            string placeID = poi.PlaceID;
            if (PlaceIDToGoogleMapPOI.ContainsKey(poi.PlaceID))
            {
                //Was added in previous request, Update it
                PlaceIDToGoogleMapPOI[placeID] = poi;
                continue;
            }
            if (!PlaceIDToGoogleMapPOI.TryAdd(placeID, poi))
            {
                //Added at the same time (concurrency), Update with (latest?)
                PlaceIDToGoogleMapPOI[placeID] = poi;
                continue;
            }

            //Debug.Log($"D:{G.Haversine(G.Location.X, G.Location.Y, poi.Geometry.Location.Latitude, poi.Geometry.Location.Longitude)} {poi.Name} ({poi.PlaceID})");
            //test only
            //G.POIManager.SpawnPOI(poi);
        }
    }

    public void DebugAllPOINames()
    {
        foreach (KeyValuePair<string, GGoogleMapsPOI> pair in PlaceIDToGoogleMapPOI)
        {
            Debug.Log($"{pair.Value.Name} ({pair.Key})");
        }
    }

    //TASK
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
            //If this fails (FinishQueryLocationTaskWithFailure) it will return null
            return await queryLocationTask;
        }
        return null;
    }

    //TASK
    public void AddQueryLocationTask(Vector2ds location, Task<GGoogleMapsQueryLocation> queryLocation)
    {
        if (!PendingQueryLocations.TryAdd(location, queryLocation))
        {
            PendingQueryLocations[location] = queryLocation;
        }
    }
    //TASK
    public void FinishQueryLocationTask(Vector2ds location, GGoogleMapsQueryLocation queryLocation)
    {
        PendingQueryLocations.TryRemove(location, out _);
        if (!LocationToQueryLocation.TryAdd(location, queryLocation))
        {
            LocationToQueryLocation[location] = queryLocation;
        }
    }
    //TASK
    public void FinishQueryLocationTaskWithFailure(Vector2ds location)
    {
        PendingQueryLocations.TryRemove(location, out _);
    }

}
