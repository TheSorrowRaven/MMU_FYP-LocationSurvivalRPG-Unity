using Mapbox.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapZombie : MonoBehaviour
{
    private static G G => G.Instance;

    public Transform ThisTR;

    [System.NonSerialized] public Vector2d GeoLocation;

    [SerializeField] private GameObject display;

    [System.NonSerialized] private bool active; // Display is showing

    private void Awake()
    {
        Hidden();
    }

    public void MapUpdated()
    {
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        Vector3 pos = G.GeoToWorld(GeoLocation);
        ThisTR.localPosition = pos;
    }


    public void Hidden()
    {
        display.SetActive(false);
        active = false;
    }
    public void Seen()
    {
        display.SetActive(true);
        active = true;
    }

}
