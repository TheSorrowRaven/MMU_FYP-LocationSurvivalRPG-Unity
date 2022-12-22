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

    public GGoogleMapsPOI GPOI { get; private set; }
    [System.NonSerialized] private bool active; // Display is showing

    private void Awake()
    {
        Hidden();
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


    private void Update()
    {
        if (!active)
        {
            return;
        }
        ScreenFacerTR.LookAt(G.MainCameraTR);
    }

}
