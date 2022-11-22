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
    [field: SerializeField] public int InteractableRadius { get; private set; }

    [field: SerializeField] public float PlayerMapOrbitalCameraSpeed { get; private set; } = 1f;
    [field: SerializeField] public double MovementSpeed { get; private set; } = 0.000001;
    [field: SerializeField] public double MovementSpeedMultiplier { get; private set; } = 3.0;

    [field: SerializeField] public float GoogleNearbyPlacesNextPageRequestDelay { get; private set; }
    [field: SerializeField] public int WebRequestRetryMax { get; private set; }
    [field: SerializeField] public float WebRequestFailLinearDelay { get; private set; }

    private void Awake()
    {
        instance = this;
    }


}
