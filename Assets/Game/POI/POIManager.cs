using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class POIManager : MonoBehaviour, Save.ISaver
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
    [System.NonSerialized] private readonly HashSet<string> UndefinedTypes = new();
    [System.NonSerialized] private Dictionary<string, Dictionary<string, int>> VisitedPOIs;

    private DateTime lastVisitedPOIDate;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        POIs.Clear();
    }

    private void Start()
    {
        StartInit();
        G.Mapbox.OnUpdated += MapUpdated;
        LoadPOITypesDefinition();
    }

    private void Update()
    {
        if (!gameObject.activeSelf)
        {
            return;
        }
        UpdateClearPOI();
    }

    public void ActivateCombatMode(bool combatMode)
    {
        gameObject.SetActive(!combatMode);
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
            Vector3 pos = poi.ThisTR.localPosition;
            if (Mathf.Abs(pos.x) > distanceFromPlayerToClearPOI.x && Mathf.Abs(pos.z) > distanceFromPlayerToClearPOI.y)
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

    public bool TryGetVisitedPOI(string placeID, out Dictionary<string, int> remainingItemAmts)
    {
        DateTime today = DateTime.Now.Date;
        if (!lastVisitedPOIDate.Equals(today))
        {
            VisitedPOIs.Clear();
            lastVisitedPOIDate = today;
            Save.Instance.SaveRequest();
        }
        return VisitedPOIs.TryGetValue(placeID, out remainingItemAmts);
    }

    public void UpdateVisitedPOIs(string placeID, Dictionary<string, int> remainingItemAmts)
    {
        VisitedPOIs[placeID] = remainingItemAmts;
    }


    public void StartInit()
    {
        Save.Instance.InitSaver(this);
    }

    public void SaveData(Save.Data data)
    {
        data.VisitedPOIDate = lastVisitedPOIDate;
    }

    public void LoadData(Save.Data data)
    {
        data.VisitedPOIs ??= new();
        VisitedPOIs = data.VisitedPOIs;
        lastVisitedPOIDate = data.VisitedPOIDate;
    }


}
