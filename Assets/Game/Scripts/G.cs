using Mapbox.Unity.Map;
using Mapbox.Utils;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Mapbox.Unity.Constants.GUI;

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

    }

    private void Update()
    {
    }

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

}
