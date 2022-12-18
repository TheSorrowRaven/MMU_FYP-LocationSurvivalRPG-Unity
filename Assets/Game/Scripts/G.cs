using JetBrains.Annotations;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;

public class G : MonoBehaviour
{
    private static G instance;
    public static G Instance => instance;

    private static GameSettings GameSettings => GameSettings.Instance;
    private static GLocationService GLocationProvider => GLocationService.Instance;
    private static GGoogleMapsService GGoogleMapsService => GGoogleMapsService.Instance;

    [field: SerializeField] public AbstractMap Mapbox { get; private set; }

    public PlayerLocation Location { get; private set; }

    public TextMeshProUGUI lastUpdate;

    public TextMeshProUGUI coords;

    public Camera MainCamera;
    public Transform MainCameraTR;

    public POIManager POIManager;

    public GameObject DebugCube;

    public TextAsset GooglePOITypesDefinition;

    [NonSerialized] public double PhysicalMetersPerUnityUnits;

    #region UI
    public GJoystick MovementJoystick;
    public GToggle GPSToggle;

    #endregion

    private void Awake()
    {
        instance = this;

        Application.targetFrameRate = 60;

        Location = new(2.923140, 101.639631);
        //Location = new(64.140965, -21.912568);
        //Location = new(72.536582, 92.650241);
        //Location = new(0, 0);
        //Location = new(41.864515, -75.125218); //Away from NY
        //Location = new(40.722050, -73.988062); //NY
        //Location = new(82.487342, -32.677854);


    }

    private void Start()
    {
        InitializeServices();

        Mapbox.Initialize(Location, 18);
        Mapbox.UpdateMap(18f);

        PhysicalMetersPerUnityUnits = Haversine(WorldToGeo(Vector3.zero), WorldToGeo(Vector3.right));
        GameSettings.GSetUnityUnitsDistanceQueryRadius(GameSettings.MetersDistanceQueryRadius / PhysicalMetersPerUnityUnits);

    }

    private void Update()
    {
    }


    //private double TestLat = 2.92040777778856;
    //private double TestLon = 101.636452902552;

    //[ContextMenu("TestGoogleMapsAPI")]
    //public void TestGoogleMapsAPI()
    //{
    //    GGoogleMapsService.StartNearbyQueryLocation(new(TestLat, TestLon), (res) =>
    //    {
    //        for (int i = 0; i < res.POIs.Count; i++)
    //        {
    //            GGoogleMapsPOI poi = res.POIs[i];
    //            Debug.Log($"D:{Haversine(TestLat, TestLon, poi.Geometry.Location.Latitude, poi.Geometry.Location.Longitude)} {poi.Name} ({poi.PlaceID})");
    //        }
    //    });
    //}

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

    //// Function to perform a Mercator projection and map coordinates to pixel units
    //public static void MercatorProjection(double longitude, double latitude, int pixelWidth, int pixelHeight, out int x, out int y)
    //{
    //    // Convert longitude and latitude to radians
    //    double lon = longitude * Math.PI / 180.0;
    //    double lat = latitude * Math.PI / 180.0;

    //    // Perform the Mercator projection
    //    x = (int)(pixelWidth * (lon + Math.PI) / (2 * Math.PI));
    //    y = (int)(pixelHeight * (Math.PI - Math.Log(Math.Tan(lat / 2 + Math.PI / 4))));
    //}

    //// Function to perform a Miller cylindrical projection and map coordinates to pixel units
    //public static void MillerCylindricalProjection(double longitude, double latitude, int pixelWidth, int pixelHeight, out int x, out int y)
    //{
    //    // Convert longitude and latitude to radians
    //    double lon = longitude * Math.PI / 180.0;
    //    double lat = latitude * Math.PI / 180.0;

    //    // Perform the Miller cylindrical projection
    //    x = (int)(pixelWidth * (lon + Math.PI) / (2 * Math.PI));
    //    y = (int)(pixelHeight * lat / Math.PI);
    //}

    //public static void MercatorProjection2(double longitude, double latitude, int pixelWidth, int pixelHeight, out int x, out int y)
    //{
    //    int mapWidth = pixelWidth;
    //    int mapHeight = pixelHeight;

    //    // get x value
    //    x = (int)((longitude + 180.0) * (mapWidth / 360.0));

    //    // convert from degrees to radians
    //    double latRad = latitude * Math.PI / 180;

    //    // get y value
    //    double mercN = Math.Log(Math.Tan((Math.PI / 4) + (latRad / 2)));
    //    y = (int)((mapHeight / 2.0) - (mapWidth * mercN / (2 * Math.PI)));
    //}

    public static Vector2Int EquirectangularProjection(double lat, double lon, int imageWidth, int imageHeight)
    {
        // Project the longitude and latitude values onto the image
        int projectedX = (int)((lon + 180) / 360.0 * imageWidth);
        int projectedY = (int)((lat + 90) / 180.0 * imageHeight);

        // Return the projected coordinates as a Point object
        return new Vector2Int(projectedX, projectedY);
    }

    public static Vector2 RandomPosition(Vector2 min, Vector2 max)
    {
        float r = UnityEngine.Random.value;
        float x = min.x + (r * (max.x - min.x));
        r = UnityEngine.Random.value;
        float y = min.y + (r * (max.y - min.y));
        return new(x, y);
    }

}
