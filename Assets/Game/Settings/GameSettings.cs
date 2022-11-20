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


    [field: SerializeField] public double MovementSpeed { get; private set; } = 0.000001;


    private void Awake()
    {
        instance = this;
    }


}
