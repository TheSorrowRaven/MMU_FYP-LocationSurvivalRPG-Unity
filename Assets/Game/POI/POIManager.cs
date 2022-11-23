using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class POIManager : MonoBehaviour
{
    private static G G => G.Instance;

    [SerializeField] private Transform POIContainer;
    [SerializeField] private GameObject POIPrefab;

    [System.NonSerialized] private Dictionary<string, POI> POIs = new();

    private void Awake()
    {
        POIs.Clear();
    }

    private void Start()
    {
        G.Mapbox.OnUpdated += MapUpdated;
    }

    public void SpawnPOI(GGoogleMapsPOI gPOI)
    {
        POI poi = Instantiate(POIPrefab, POIContainer).GetComponent<POI>();
        poi.ActivatePOI(gPOI);

        POIs.Add(gPOI.PlaceID, poi);
    }

    public void UpdatePOI(GGoogleMapsPOI gPOI)
    {
        if (POIs.TryGetValue(gPOI.PlaceID, out POI poi))
        {
            poi.UpdatePOI(gPOI);
        }
        else
        {
            SpawnPOI(gPOI);
        }
    }

    private void MapUpdated()
    {
        foreach (var pair in POIs)
        {
            POI poi = pair.Value;
            poi.MapUpdated();
        }
    }

}
