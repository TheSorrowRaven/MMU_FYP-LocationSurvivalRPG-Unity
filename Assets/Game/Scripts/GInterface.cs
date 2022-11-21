using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script relays functions/information between MonoBehaviour scripts (through Insepctor) and non-MonoBehaviour scripts
/// </summary>
public class GInterface : MonoBehaviour
{
    private static G G => G.Instance;

    //Unity Event Referenced
    public void GPSToggled(bool gpsOn)
    {
        G.Location.UIReportGPSToggle(gpsOn);
    }



}
