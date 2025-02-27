using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GLocationService
{
    public static readonly GLocationService Instance = new();

    private static G G => G.Instance;
    private static GameSettings GameSettings => GameSettings.Instance;

    private LocationService Location => Input.location;

    public bool IsInitialized { get; private set; } = false;
    public bool HasLocationPermission { get; private set; } = false;
    private LocationInfo CurrentLocation => Location.lastData;
    private LocationInfo lastFetchedLocation;
    private float lastFetchedTime = 0;

    private GLocationService()
    {
    }

    /// <summary>
    /// Initializes, asks for device permission if required. Wait for IsInitialized = true before trying to TryGetLocation
    /// </summary>
    public void Initialize()
    {
        IsInitialized = false;
        HasLocationPermission = false;
        TryToEnableLocation();
    }

    /// <summary>
    /// Check for IsInitialized for true-ness before calling this
    /// </summary>
    /// <param name="tryToEnableLocation">Don't set this to true if calling every frame</param>
    /// <param name="latitude"></param>
    /// <param name="longitude"></param>
    /// <returns></returns>
    public bool TryGetLocation(bool tryToEnableLocation, out double latitude, out double longitude)
    {
        if (!IsInitialized)
        {
            Debug.LogWarning("LocationProvider has to finish initializing first!");
            latitude = 0;
            longitude = 0;
            return false;
        }
        if (!HasLocationPermission)
        {
            if (tryToEnableLocation)
            {
                TryToEnableLocation();
            }
            latitude = 0;
            longitude = 0;
            return false;
        }
        if ((Time.time - lastFetchedTime) >= GameSettings.LocationRequestTimeHardLimit)
        {
            //Update newest location
            FetchLatestLocation();
        }
        latitude = lastFetchedLocation.latitude;
        longitude = lastFetchedLocation.longitude;
        return true;
    }

    private void FetchLatestLocation()
    {
        lastFetchedLocation = CurrentLocation;
        lastFetchedTime = Time.time;
        G.LastUpdate.SetText($"Last Update: {DateTime.Now}\n{lastFetchedLocation.latitude}, {lastFetchedLocation.longitude}");
    }

    private void TryToEnableLocation()
    {
        G.StartCoroutine(EnableLocation());
    }
    public void TryDisableLocation()
    {
        G.StartCoroutine(DisableLocation());
    }

    private IEnumerator EnableLocation()
    {
        IsInitialized = false;
        yield return CheckHasLocationPermission();
        if (!HasLocationPermission)
        {
            Debug.Log("Permission denied for location access");
            IsInitialized = true;
            yield break;
        }
        Location.Start(GameSettings.LocationAccuracyInMeters, GameSettings.LocationUpdateDistanceInMeters);
        Debug.Log("Location is Starting...");
        while (true)
        {
            if (Location.status == LocationServiceStatus.Initializing)
            {
                Debug.Log("Location Status is Initializing...");
                yield return null;
                continue;
            }
            else if (Location.status == LocationServiceStatus.Stopped)
            {
                Debug.LogError("Location Status is Stopped!");
                IsInitialized = true;
                yield break;
            }
            else if (Location.status == LocationServiceStatus.Running)
            {
                Debug.Log("Location Status is Running!");
                break;
            }
            else if (Location.status == LocationServiceStatus.Failed)
            {
                Debug.LogError("Location Status Failed!");
                IsInitialized = true;
                yield break;
            }
            Debug.LogError("Unkown Location Status!");
            IsInitialized = true;
            yield break;
        }
        Debug.Log($"Current Location: {CurrentLocation.latitude}, {CurrentLocation.longitude}");
        FetchLatestLocation();
        IsInitialized = true;
    }

    private IEnumerator DisableLocation()
    {
        Location.Stop();
        Debug.Log("Location is Stopping");
        while (true)
        {
            if (Location.status == LocationServiceStatus.Initializing || Location.status == LocationServiceStatus.Running)
            {
                Debug.Log("Location is Stopping...");
                yield return new WaitForSecondsRealtime(0.5f);
                continue;
            }
            else if (Location.status == LocationServiceStatus.Stopped || Location.status == LocationServiceStatus.Failed)
            {
                Debug.Log("Location Stopped");
                yield break;
            }
            break;
        }
    }

    private IEnumerator CheckHasLocationPermission()
    {
#if UNITY_EDITOR

#elif UNITY_ANDROID
        EnsureAndroidPermission();
#elif UNITY_IOS
        EnsureIOSPermission();
#endif
        Debug.Log("Waiting for Location Permission");
        yield return null;

        bool permissionAllowed = Location.isEnabledByUser;
        Debug.Log($"Permission Allowed: {permissionAllowed}");
        HasLocationPermission = permissionAllowed;
    }

    private void EnsureAndroidPermission()
    {
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.FineLocation))
        {
            Debug.Log("Requesting Android Permission for Fine Location");
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.FineLocation);
            Debug.Log("Requested Android Permission for Fine Location");
        }
    }

    private void EnsureIOSPermission()
    {
        //On iOS, we need location access. To do that, we need to provide the settings field for it under Project Settings -> Player -> iOS -> Location Usage Description.
        //Any non-empty value will do. This is particularly important since without it everything seems to work but the location lookups fail silently.
        return;
    }

}
