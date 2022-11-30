using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    private static GameSettings instance;
    public static GameSettings Instance => instance;


    [field: SerializeField] public float LocationRequestTimeHardLimit { get; private set; } = 0.5f;
    [field: SerializeField] public float LocationAccuracyInMeters { get; private set; } = 5f;
    [field: SerializeField] public float LocationUpdateDistanceInMeters { get; private set; } = 5f;


    [field: Range(0, 1f)]
    [field: SerializeField] public float LocationSmoothingTime { get; private set; } = 0.05f;

    //In meters
    [field: SerializeField] public int POIRadius { get; private set; } = 25;
    [field: SerializeField] public double MetersTravelledBeforePOIQuery { get; set; } = 25;
    //In seconds
    [field: SerializeField] public float DelayBeforePOIQuery { get; set; } = 2f;


    [field: SerializeField] public float PlayerMapOrbitalCameraSpeed { get; private set; } = 1f;
    [field: SerializeField] public double MovementSpeed { get; private set; } = 0.000001;
    [field: SerializeField] public double MovementSpeedMultiplier { get; private set; } = 3.0;

    



    [field: SerializeField] public float GoogleNearbyPlacesNextPageRequestDelay { get; private set; }
    [field: SerializeField] public int WebRequestRetryMax { get; private set; }
    [field: SerializeField] public float WebRequestFailLinearDelay { get; private set; }


    [field: SerializeField] public double LatLonDistanceQueryRadius { get; private set; }
    [field: SerializeField] public double MetersDistanceQueryRadius { get; private set; }
    [field: SerializeField] public double UnityUnitsDistanceQueryRadius { get; private set; }


    private void OnValidate()
    {
        MetersDistanceQueryRadius = G.Haversine(new(0, 0), new(0, LatLonDistanceQueryRadius));
    }



    private void Awake()
    {
        instance = this;
    }

    public void GSetUnityUnitsDistanceQueryRadius(double value)
    {
        UnityUnitsDistanceQueryRadius = value;
    }


}
