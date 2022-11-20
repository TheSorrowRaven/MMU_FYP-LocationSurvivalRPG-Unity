using Mapbox.Unity.Map;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class G : MonoBehaviour
{
    private static G instance;
    public static G Instance => instance;

    private static GLocationService GLocationProvider => GLocationService.Instance;

    [field: SerializeField] public AbstractMap Mapbox { get; private set; }

    public Location Location { get; private set; }

    public TextMeshProUGUI lastUpdate;

    #region UI
    public Joystick MovementJoystick;

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
            Mapbox.UpdateMap(Location);
        }
    }

    private void InitializeServices()
    {
        GLocationProvider.Initialize();
    }

}
