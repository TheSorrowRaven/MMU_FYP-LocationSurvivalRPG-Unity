using Mapbox.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    private static G G => G.Instance;
    private static GameSettings GameSettings => GameSettings.Instance;
    private static GGoogleMapsService GGoogleMapsService => GGoogleMapsService.Instance;
    private static MapZombieManager MapZombieManager => MapZombieManager.Instance;

    private static PlayerLocation Location => G.Location;

    public Transform ThisTR;

    [SerializeField] private LineRenderer RadiusLR;
    [SerializeField] private float radius;
    [SerializeField] private int radiusDetail;


    [System.NonSerialized] private Vector2d lastPos;

    [System.NonSerialized] private double lastLat;
    [System.NonSerialized] private double lastLon;

    [System.NonSerialized] private float lastPOIQueryTime;
    [System.NonSerialized] private Vector2d lastQueryLocation;
    [System.NonSerialized] private Vector2d[] queryLocationsBuffer = new Vector2d[4];
    [System.NonSerialized] private Vector2d[] lastQueryLocations = new Vector2d[4];

    [System.NonSerialized] private Vector2Int lastMapZombieCell;



    #region Gameplay

    [field: SerializeField] public int Level { get; private set; }
    [field: SerializeField] public int Experience { get; private set; }

    [field: SerializeField] public int Health { get; private set; }
    [field: SerializeField] public int Hunger { get; private set; }
    [field: SerializeField] public int Energy { get; private set; }


    #endregion




    private void Awake()
    {
        lastLat = double.MinValue;
        lastLon = double.MinValue;
    }

    //Unity Event Referenced
    public void ReceiveJoystickMovement(Vector2 movementDelta)
    {
        G.Location.PlayerReportJoystickMovment(movementDelta);
    }


    private void Update()
    {
        Map_Update();
    }

    private void Map_Update()
    {
        Map_PositionUpdate();
        Map_RotationUpdate();
        Map_POIUpdate();
        Map_ZombiesUpdate();
    }

    private void Map_PositionUpdate()
    {
        if (!GLocationService.Instance.IsInitialized)
        {
            return;
        }
        Location.Update();
        
        if (Location.X == lastLat && Location.Y == lastLon)
        {
            //Skip Update (No change in position)
            return;
        }
        G.coords.SetText("COORDS: " + Location);
        G.Mapbox.UpdateMap(Location);

        lastLat = Location.X;
        lastLon = Location.Y;
    }

    private void Map_RotationUpdate()
    {
        Vector2d currentPos = new(G.Instance.Location.Y, G.Instance.Location.X);
        Vector2d direction = (currentPos - lastPos) / GameSettings.MovementSpeed;

        Vector3 lookAt = new Vector3((float)direction.x, 0, (float)direction.y) + ThisTR.localPosition;
        ThisTR.LookAt(lookAt, Vector3.up);

        lastPos = currentPos;
    }

    private void Map_POIUpdate()
    {
        float now = Time.time;
        if (now - lastPOIQueryTime < GameSettings.DelayBeforePOIQuery)
        {
            return;
        }

        Vector2d loc = Location;
        if (loc.x == lastQueryLocation.x && loc.y == lastQueryLocation.y)
        {
            //Location didn't change
            return;
        }
        lastQueryLocation = loc;

        Bounds2d bounds = GGoogleMapsQueryGrid.GetQueryLocationsFromPosition(Location, queryLocationsBuffer);
        Debug.DrawLine(G.GeoToWorld(queryLocationsBuffer[0]), G.GeoToWorld(queryLocationsBuffer[3]), Color.blue);
        for (int i = 1; i < queryLocationsBuffer.Length; i++)
        {
            Debug.DrawLine(G.GeoToWorld(queryLocationsBuffer[i]), G.GeoToWorld(queryLocationsBuffer[i - 1]), Color.blue);
        }

        if (QueryLocationEqualToLastQueryLocations())
        {
            //In the same cell
            return;
        }
        //In a different cell
        UpdateLastQueryLocations();
        lastPOIQueryTime = now;

        //Query
        QueryPOI(bounds);
    }

    private void Map_ZombiesUpdate()
    {
        Vector2Int currentCell = MapZombieManager.GeoToCell(Location);
        if (currentCell == lastMapZombieCell)
        {
            return;
        }
        lastMapZombieCell = currentCell;
        MapZombieManager.PlayerUpdate(currentCell);
    }




    public void SelectPOI(Vector2 screenPosition)
    {
        const int layerMask = 1 << 6 | 1 << 8;
        Ray ray = G.MainCamera.ScreenPointToRay(screenPosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, layerMask))
        {
            return;
        }
        if (hit.collider.TryGetComponent(out POI poi))
        {
            //POI Selected
            UIPOI.Instance.SetGPOI(poi.GPOI);
            Debug.Log(poi.GPOI.Name);
        }
        else if (hit.collider.TryGetComponent(out MapZombie zombie))
        {
            Debug.Log("Tapped on zombie");
        }
    }






    private void UpdateLastQueryLocations()
    {
        for (int i = 0; i < queryLocationsBuffer.Length; i++)
        {
            lastQueryLocations[i] = queryLocationsBuffer[i];
        }
    }

    private bool QueryLocationEqualToLastQueryLocations()
    {
        for (int i = 0; i < queryLocationsBuffer.Length; i++)
        {
            if (lastQueryLocations[i].x != queryLocationsBuffer[i].x)
            {
                return false;
            }
            if (lastQueryLocations[i].y != queryLocationsBuffer[i].y)
            {
                return false;
            }
        }
        return true;
    }

    private void QueryPOI(Bounds2d bounds)
    {
        GGoogleMapsQueryCluster cluster = new();
        cluster.PrepareQueryCluster(bounds, queryLocationsBuffer);
        cluster.QueryAllLocations(ClusterCompleteAction);
    }
    private void ClusterCompleteAction(GGoogleMapsQueryCluster cluster)
    {
        //Debug.Log("Cluster complete!");
        //Spawn all
        G.POIManager.StartSpawningPOIs();
        foreach (GGoogleMapsQueryLocation location in cluster.Locations())
        {
            foreach (GGoogleMapsPOI poi in location.DensityFilteredPOIs)
            {
                //Debug.Log($"{poi.Name} ({poi.PlaceID})");
                G.POIManager.SpawnPOI(poi);
            }
            //var o = Instantiate(G.DebugCube, G.GeoToWorld(location.Location), Quaternion.identity);
            //o.name = location.Location.ToString();
            //(o.GetComponent<GTest2>()).location = location.Location;
        }
        G.POIManager.EndSpawningPOIs();
    }

    [ContextMenu("Draw Player Radius")]
    public void DrawPlayerRadius()
    {
        RadiusLR.positionCount = radiusDetail + 1;
        for (int i = 0; i <= radiusDetail; i++)
        {
            float angle = 360f / radiusDetail * i * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * radius;
            RadiusLR.SetPosition(i, pos);
        }
    }

}
