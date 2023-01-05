using Mapbox.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapZombie : MonoBehaviour
{
    private static G G => G.Instance;

    public Transform ThisTR;

    [System.NonSerialized] public Vector2Int cellPos;
    [System.NonSerialized] public Vector2d GeoLocation;

    [SerializeField] private GameObject display;

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
    }
    public void Seen()
    {
        display.SetActive(true);
    }

}
