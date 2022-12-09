using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class POIManager : MonoBehaviour
{
    private static G G => G.Instance;
    private static POIManager instance;
    public static POIManager Instance => instance;

    [SerializeField] private Transform POIContainer;
    [SerializeField] private GameObject POIPrefab;

    [System.NonSerialized] private Dictionary<string, POI> POIs = new();
    [System.NonSerialized] private Dictionary<string, POITypeDefinition> POITypeDefinitions;
    [System.NonSerialized] private HashSet<string> UndefinedTypes = new();

    private void Awake()
    {
        instance = this;
        POIs.Clear();
    }

    private void Start()
    {
        G.Mapbox.OnUpdated += MapUpdated;
        LoadPOITypesDefinition();
    }

    private void LoadPOITypesDefinition()
    {
        string text = G.GooglePOITypesDefinition.text;
        POITypeDefinitions = JsonConvert.DeserializeObject<Dictionary<string, POITypeDefinition>>(text);
    }

    public bool TryGetPOITypeDefinition(string type, out POITypeDefinition definition)
    {
        return POITypeDefinitions.TryGetValue(type, out definition);
    }

    public void AddUndefinedTypes(string type)
    {
        bool added = UndefinedTypes.Add(type);
        if (added)
        {
            return;
        }
        Debug.LogWarning($"POI Type: {type} not defined");
    }



    public void SpawnPOI(GGoogleMapsPOI gPOI)
    {
        if (POIs.ContainsKey(gPOI.PlaceID))
        {
            return;
        }
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
