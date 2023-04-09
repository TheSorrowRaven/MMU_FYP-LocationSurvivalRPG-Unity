using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class POI : MonoBehaviour
{
    private static G G => G.Instance;

    [SerializeField] public Transform ThisTR;

    [SerializeField] private Transform ScreenFacerTR;
    [SerializeField] private TextMeshPro NameText;
    [SerializeField] private GameObject display;

    [NonSerialized] public bool insideRadius;

    public GGoogleMapsPOI GPOI { get; private set; }

    private void Awake()
    {
        display.SetActive(true);
        insideRadius = false;
    }

    public void ActivatePOI(GGoogleMapsPOI gPOI)
    {
        this.GPOI = gPOI;
        UpdatePosition();

        NameText.SetText(gPOI.Name);
    }

    //Full Update
    public void UpdatePOI(GGoogleMapsPOI gPOI)
    {
        ActivatePOI(gPOI);
    }

    public void MapUpdated()
    {
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        Vector3 pos = G.GeoToWorld(GPOI.Geometry.Location.Latitude, GPOI.Geometry.Location.Longitude);
        ThisTR.localPosition = pos;
    }

    public void InRadiusCanLoot()
    {
        insideRadius = true;
    }
    public void OutsideRadiusCannotLoot()
    {
        insideRadius = false;
    }


    private void Update()
    {
        ScreenFacerTR.LookAt(G.MainCameraTR);
    }

}
