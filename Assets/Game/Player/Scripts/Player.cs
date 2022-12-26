using Mapbox.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour, Save.ISaver
{
    private static G G => G.Instance;
    private static GameSettings GameSettings => GameSettings.Instance;
    private static GGoogleMapsService GGoogleMapsService => GGoogleMapsService.Instance;
    private static MapZombieManager MapZombieManager => MapZombieManager.Instance;
    private static UIStats UIStats => UIStats.Instance;

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

    [System.NonSerialized] private int health;
    [System.NonSerialized] private int hunger;
    [System.NonSerialized] private int stamina;
    [System.NonSerialized] private int zombification;
    public int Health
    {
        get => health;
        private set
        {
            health = value;
            StatUpdated(0, value);
        }
    }
    public int Hunger
    {
        get => hunger;
        private set
        {
            hunger = value;
            StatUpdated(1, value);
        }
    }
    public int Stamina
    {
        get => stamina;
        private set
        {
            stamina = value;
            StatUpdated(2, value);
        }
    }
    public int Zombification
    {
        get => zombification;
        private set
        {
            zombification = value;
            StatUpdated(3, value);
        }
    }


    #endregion

    private void StatUpdated(int index, int val)
    {
        UIStats.SetSliderValue(index, val);
        Save.Instance.SaveRequest();
    }



    private void Awake()
    {
        lastLat = double.MinValue;
        lastLon = double.MinValue;
    }

    private void Start()
    {
        StartInit();
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
            //Debug.Log(poi.GPOI.Name);
        }
        else if (hit.collider.TryGetComponent(out MapZombie zombie))
        {
            SceneManager.LoadScene(1);
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

    public void StartInit()
    {
        Save.Instance.InitSaver(this);
    }

    public void SaveData(Save.Data data)
    {
        data.PlayerStats ??= new int[4];
        data.PlayerStats[0] = Health;
        data.PlayerStats[1] = Hunger;
        data.PlayerStats[2] = Stamina;
        data.PlayerStats[3] = Zombification;
    }

    public void LoadData(Save.Data data)
    {
        if (data.PlayerStats == null)
        {
            Health = 100;
            Hunger = 100;
            Stamina = 100;
            Zombification = 0;
        }
        else
        {
            Health = data.PlayerStats[0];
            Hunger = data.PlayerStats[1];
            Stamina = data.PlayerStats[2];
            Zombification = data.PlayerStats[3];
        }

    }
}
