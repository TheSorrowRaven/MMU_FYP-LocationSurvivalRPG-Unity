using Mapbox.Unity.Map;
using Mapbox.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class G : MonoBehaviour
{
    private static G instance;
    public static G Instance => instance;

    private static GLocationService GLocationProvider => GLocationService.Instance;
    private static GGoogleMapsService GGoogleMapsService => GGoogleMapsService.Instance;

    [field: SerializeField] public AbstractMap Mapbox { get; private set; }

    public PlayerLocation Location { get; private set; }

    public TextMeshProUGUI lastUpdate;

    public TextMeshProUGUI coords;

    public Camera MainCamera;
    public Transform MainCameraTR;

    public POIManager POIManager;

    [NonSerialized] public double PhysicalMetersPerUnityUnits;

    #region UI
    public GJoystick MovementJoystick;
    public GToggle GPSToggle;

    #endregion

    private void Awake()
    {
        instance = this;
        Location = new(2.923140, 101.639631);
    }

    private void Start()
    {
        InitializeServices();

        Mapbox.Initialize(Location, 18);
        Mapbox.UpdateMap(18f);

        PhysicalMetersPerUnityUnits = Haversine(WorldToGeo(Vector3.zero), WorldToGeo(Vector3.right));
    }

    private void Update()
    {

    }

    //Test one more time tomorrow! MAKE SURE to change the Interactable Radius to 100!
    private double TestLat = 2.92040777778856;
    private double TestLon = 101.636452902552;

    [ContextMenu("TestGoogleMapsAPI")]
    public void TestGoogleMapsAPI()
    {
        //GGoogleMapsService.MakeNearbyPlacesRequest(Location.X, Location.Y);
        GGoogleMapsService.MakeNearbyPlacesRequest(TestLat, TestLon);
    }

    private void InitializeServices()
    {
        GLocationProvider.Initialize();
        GGoogleMapsService.Initialize();
    }

    public Vector3 GeoToWorld(double latitude, double longitude)
    {
        return GeoToWorld(new(latitude, longitude));
    }
    public Vector3 GeoToWorld(Vector2d geoCoord)
    {
        return Mapbox.GeoToWorldPosition(geoCoord);
    }
    public Vector2d WorldToGeo(Vector3 world)
    {
        return Mapbox.WorldToGeoPosition(world);
    }

    public static double Haversine(Vector2d pos1, Vector2d pos2)
    {
        return Haversine(pos1.x, pos1.y, pos2.x, pos2.y);
    }

    /// <summary>
    /// Returns the distance between 2 geo coordinates in meters
    /// </summary>
    public static double Haversine(double lat1, double lon1, double lat2, double lon2)
    {
        const double p = Math.PI / 180;
        lat1 *= p;
        lon1 *= p;
        lat2 *= p;
        lon2 *= p;

        return HaversineRad(lat1, lon1, lat2, lon2);
    }

    public static double HaversineRad(double lat1, double lon1, double lat2, double lon2)
    {
        const double m = 6371000; // Meters constant multiplier (Radius of Earth)

        double sLat = Math.Sin((lat2 - lat1) / 2);
        double sLon = Math.Sin((lon2 - lon1) / 2);
        double val = 2 * m * Math.Asin(Math.Sqrt(sLat * sLat + Math.Cos(lat1) * Math.Cos(lat2) * sLon * sLon));

        return val;
    }

}
