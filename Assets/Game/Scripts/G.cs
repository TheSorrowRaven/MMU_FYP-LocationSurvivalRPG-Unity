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

    [field: SerializeField] public AbstractMap Mapbox { get; private set; }

    public Location Location { get; private set; }

    public TextMeshProUGUI lastUpdate;

    public TextMeshProUGUI coords;

    public Camera MainCamera;
    public Transform MainCameraTR;

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

        //string loc = Location.ActualX + "%2C" + Location.ActualY;
        //GMaps.Instance.MakeRequest($"https://maps.googleapis.com/maps/api/place/nearbysearch/json?location={loc}&radius=100&key=AIzaSyBb9FmWLtnQwQu2IAfvVsSOUkqadHZTeMk");
    }

    private void Update()
    {
        if (GLocationService.Instance.IsInitialized)
        {
            Location.Update();
            coords.SetText("COORDS: " + Location);
            Mapbox.UpdateMap(Location);
        }
    }

    private void InitializeServices()
    {
        GLocationProvider.Initialize();
    }

    public Vector3 CoordToWorld(double latitude, double longitude)
    {
        return CoordToWorld(new(latitude, longitude));
    }
    public Vector3 CoordToWorld(Vector2d geoCoord)
    {
        return Mapbox.GeoToWorldPosition(geoCoord);
    }
    public Vector2d WorldToCoord(Vector3 world)
    {
        return Mapbox.WorldToGeoPosition(world);
    }

}
