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

    [System.NonSerialized] public int zombiesCount;
    [System.NonSerialized] public bool isChasingPlayer = false;

    private void Awake()
    {
        Hidden();
    }

    public void MapUpdated()
    {
        UpdatePosition();
        UpdatePlayerCheck();
    }

    private void Update()
    {
        if (!isChasingPlayer)
        {
            return;
        }
        UpdateChasePlayer();
    }

    private void UpdatePlayerCheck()
    {
        if (!display.activeSelf)
        {
            isChasingPlayer = false;
            return;
        }
        
        if (isChasingPlayer)
        {
            return;
        }

        float distance = ThisTR.localPosition.sqrMagnitude; // Player is (0, 0, 0)
        if (distance < GameSettings.Instance.MapZombieDistanceToDetectPlayerSqr)
        {
            isChasingPlayer = true;
        }

    }

    private void UpdateChasePlayer()
    {
        Vector3 pos = ThisTR.localPosition;
        Vector3 direction = -pos.normalized; // Player is (0, 0, 0)

        float speed = GameSettings.Instance.MapZombieMovementSpeed;

        pos += direction * (speed * Time.deltaTime);
        GeoLocation = G.WorldToGeo(pos);
        ThisTR.localPosition = pos;
        ThisTR.LookAt(Vector3.zero);
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
