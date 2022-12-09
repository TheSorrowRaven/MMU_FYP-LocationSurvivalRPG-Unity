using Mapbox.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static GGoogleMapsResponses;

/// <summary>
/// Is the processed result of a NearbySearch query, consists of up to 60 GGoogleMapsPOI
/// </summary>
public class GGoogleMapsQueryLocation
{
    private class GPOIComparer : IComparer<GGoogleMapsPOI>
    {
        public static GPOIComparer Shared = new();

        public int Compare(GGoogleMapsPOI x, GGoogleMapsPOI y)
        {
            return y.TotalUserRatings - x.TotalUserRatings;
        }
    }


    private static POIManager POIManager => POIManager.Instance;
    private static GameSettings GameSettings => GameSettings.Instance;

    public string nextPageToken = null; //Temp use only during requests
    public bool hasFailed = false;

    public Vector2d Location;

    private readonly List<GGoogleMapsPOI> POIs = new(); // Only has types we want
    public readonly List<GGoogleMapsPOI> DensityFilteredPOIs = new();

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
        FilterPOIs();
    }


    /// <summary>
    /// 1. Ignore POI types that we don't want
    /// 2. Sort POIs in the most important order (by total user reviews)
    /// 3. 
    /// </summary>
    private void FilterPOIs()
    {
        //int initialCount = POIs.Count;
        //1. (FILTER) Type
        for (int i = 0; i < POIs.Count; i++)
        {
            GGoogleMapsPOI poi = POIs[i];
            List<string> types = poi.Types;
            bool ignored = false;
            for (int j = 0; j < types.Count; j++)
            {
                string type = types[j];
                if (!POIManager.TryGetPOITypeDefinition(type, out POITypeDefinition definition))
                {
                    POIManager.AddUndefinedTypes(type);
                    continue;
                }
                if (definition.Ignored)
                {
                    ignored = true;
                    break;
                }
            }
            if (ignored)
            {
                POIs.RemoveAt(i);
                i--;
            }
        }

        //Debug.Log($"Type Filtered {POIs.Count - initialCount} items. Left {POIs.Count}");

        //2. (SORT) TotalUserReviews
        POIs.Sort(GPOIComparer.Shared);

        //Populate DensityFilteredPOIs
        for (int i = 0; i < POIs.Count; i++)
        {
            DensityFilteredPOIs.Add(POIs[i]);
        }

        //3. (FILTER) Density Filter
        for (int i = 0; i < DensityFilteredPOIs.Count; i++)
        {
            GGoogleMapsPOI poi = DensityFilteredPOIs[i];
            //Loop into not as important POIs
            for (int j = i + 1; j < DensityFilteredPOIs.Count; j++)
            {
                GGoogleMapsPOI comparingPOI = DensityFilteredPOIs[j];
                double distance = G.Haversine(poi.Location, comparingPOI.Location);
                //Debug.Log($"Distance is {distance}");
                if (distance < GameSettings.MetersDistanceDensityRadius)
                {
                    //Too close, remove
                    //Debug.Log($"Density Filtering Off: {comparingPOI.Name}");
                    DensityFilteredPOIs.RemoveAt(j);
                    j--;
                }
            }
        }

        //Debug.Log($"Density Filtered {POIs.Count - DensityFilteredPOIs.Count} items. Left {DensityFilteredPOIs.Count}");
        //foreach (var item in POIs)
        //{
        //    Debug.Log($"Have: {item.Name} (R: {item.TotalUserRatings})");
        //}

    }

}
