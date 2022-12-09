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

    [SerializeField] private float cooldownToClearPOIs;
    [SerializeField] private Vector2 distanceFromPlayerToClearPOI;
    [System.NonSerialized] private float cooldownCount;

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

    private void Update()
    {
        UpdateClearPOI();
    }

    private void UpdateClearPOI()
    {
        cooldownCount -= Time.deltaTime;
        if (cooldownCount > 0)
        {
            return;
        }
        cooldownCount = cooldownToClearPOIs;
        List<string> removingPOIs = new();
        foreach (KeyValuePair<string, POI> item in POIs)
        {
            POI poi = item.Value;
            Vector2 pos = poi.ThisTR.localPosition;
            if (Mathf.Abs(pos.x) > distanceFromPlayerToClearPOI.x && Mathf.Abs(pos.y) > distanceFromPlayerToClearPOI.y)
            {
                removingPOIs.Add(item.Key);
            }
        }
        for (int i = 0; i < removingPOIs.Count; i++)
        {
            RemovePOI(removingPOIs[i]);
        }
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
        if (!added)
        {
            return;
        }
        Debug.LogWarning($"POI Type: {type} not defined");
    }

    public void StartSpawningPOIs()
    {

    }

    public void RemovePOI(string placeID)
    {
        POIs.Remove(placeID, out POI poi);
        poi.gameObject.Destroy();
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

    public void EndSpawningPOIs()
    {

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
