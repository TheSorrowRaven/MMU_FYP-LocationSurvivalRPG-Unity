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
    public MeshRenderer PillarRenderer;

    [NonSerialized] public bool insideRadius;

    public event Action<POI> OnExitRadius = _ => { };

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
        PillarRenderer.material.EnableKeyword("_EMISSION");
        insideRadius = true;
    }
    public void OutsideRadiusCannotLoot()
    {
        PillarRenderer.material.DisableKeyword("_EMISSION");
        insideRadius = false;
        OnExitRadius.Invoke(this);
    }


    private void Update()
    {
        ScreenFacerTR.LookAt(G.MainCameraTR);
    }

}
