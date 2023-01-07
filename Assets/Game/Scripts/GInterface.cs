using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script relays functions/information between MonoBehaviour scripts (through Insepctor) and non-MonoBehaviour scripts
/// </summary>
public class GInterface : MonoBehaviour
{
    private static G G => G.Instance;

    private void Start()
    {
        G.GPSToggle.ToggleAction += GPSToggled;
        GPSToggled(G.GPSToggle.ThisToggle.isOn);
    }

    private void GPSToggled(bool gpsOn)
    {
        G.Location.UIReportGPSToggle(gpsOn);
    }



}
