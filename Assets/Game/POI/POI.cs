using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class POI : MonoBehaviour
{
    private static G G => G.Instance;

    [SerializeField] private Transform ThisTR;

    [SerializeField] private Transform ScreenFacerTR;
    [SerializeField] private TextMeshPro NameText;

    [System.NonSerialized] public GGoogleMapsPOI gPOI;

    public void ActivatePOI(GGoogleMapsPOI gPOI)
    {
        this.gPOI = gPOI;
        UpdatePosition();

        NameText.SetText(gPOI.Name);

        gameObject.SetActive(true);
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
        Vector3 pos = G.GeoToWorld(gPOI.Geometry.Location.Latitude, gPOI.Geometry.Location.Longitude);
        ThisTR.localPosition = pos;
    }

    private void Update()
    {
        ScreenFacerTR.LookAt(G.MainCameraTR);
    }

}
