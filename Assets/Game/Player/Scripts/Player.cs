using Mapbox.Utils;
using UnityEngine;
using UnityEngine.Rendering.UI;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour, Save.ISaver
{
    private static Player instance;
    public static Player Instance => instance;


    private static G G => G.Instance;
    private static GameSettings GameSettings => GameSettings.Instance;
    private static GGoogleMapsService GGoogleMapsService => GGoogleMapsService.Instance;
    private static MapZombieManager MapZombieManager => MapZombieManager.Instance;
    private static UIStats UIStats => UIStats.Instance;

    private static PlayerLocation Location => G.Location;

    public Transform ThisTR;

    [SerializeField] private GameObject ModelObject;
    private LineRenderer RadiusLR => References.Instance.PlayerRadiusLR;
    [SerializeField] private float radius;
    [SerializeField] private int radiusDetail;


    [System.NonSerialized] private Vector2d lastPos;
    [System.NonSerialized] private bool usedWASD;

    [System.NonSerialized] private double lastLat;
    [System.NonSerialized] private double lastLon;

    [System.NonSerialized] private float lastPOIQueryTime;
    [System.NonSerialized] private Vector2d lastQueryLocation;
    [System.NonSerialized] private Vector2d[] queryLocationsBuffer = new Vector2d[4];
    [System.NonSerialized] private Vector2d[] lastQueryLocations = new Vector2d[4];

    [System.NonSerialized] private Vector2Int lastMapZombieCell;



    public bool InCombatMode { get; private set; }
    public CombatPlayer CombatPlayer => CombatPlayer.Instance;

    #region Gameplay

    [field: SerializeField] public int Level { get; private set; }
    [field: SerializeField] public int Experience { get; private set; }

    [SerializeField] private float combatMovementSpeed;
    [SerializeField] private float combatMovementSpeedMultiplier;
    public float CombatMovementSpeed
    {
        get => combatMovementSpeed;
        private set
        {
            combatMovementSpeed = value;
        }
    }
    public float CombatMovementSpeedMultiplier
    {
        get => combatMovementSpeedMultiplier;
        private set
        {
            combatMovementSpeedMultiplier = value;
        }
    }

    public float HungerConsumeTimePass;
    public int HungerConsumedPerTimePass;
    private float HungerConsumeTimePassCount;

    public float HealthConsumeStarvingTimePass;
    public int HealthConsumedStarvingPerTimePass;
    private float HealthConsumeStarvingTimePassCount;

    public float StaminaRestoreFromFoodTimePass;
    public int StaminaRestoredFromFoodPerTimePass;
    public int HungerUsedToRestoreStaminaPerTimePass;
    private float StaminaRestoreFromFoodTimePassCount;

    [System.NonSerialized] private int health;
    [System.NonSerialized] private int hunger;
    [System.NonSerialized] private int stamina;
    [System.NonSerialized] private int zombification;
    [System.NonSerialized] private int maxHealth;
    [System.NonSerialized] private int maxHunger;
    [System.NonSerialized] private int maxStamina;
    [System.NonSerialized] private int maxZombification;
    public int Health
    {
        get => health;
        private set
        {
            health = value;
            StatValueUpdated(0, value);
        }
    }
    public int Hunger
    {
        get => hunger;
        private set
        {
            hunger = value;
            StatValueUpdated(1, value);
        }
    }
    public int Stamina
    {
        get => stamina;
        private set
        {
            stamina = value;
            StatValueUpdated(2, value);
        }
    }
    public int Zombification
    {
        get => zombification;
        private set
        {
            zombification = value;
            StatValueUpdated(3, value);
        }
    }
    public int MaxHealth
    {
        get => maxHealth;
        private set
        {
            maxHealth = value;
            StatMaxUpdated(0, value);
        }
    }
    public int MaxHunger
    {
        get => maxHunger;
        private set
        {
            maxHunger = value;
            StatMaxUpdated(1, value);
        }
    }
    public int MaxStamina
    {
        get => maxStamina;
        private set
        {
            maxStamina = value;
            StatMaxUpdated(2, value);
        }
    }
    public int MaxZombification
    {
        get => maxZombification;
        private set
        {
            maxZombification = value;
            StatMaxUpdated(3, value);
        }
    }

    public void FoodEaten(FoodItem food)
    {
        int hungerRestore = food.HungerFill;
        int staminaRestore = food.StaminaRestore;

        Hunger = AddToOrMax(MaxHunger, Hunger, hungerRestore);
        Stamina = AddToOrMax(MaxStamina, Stamina, staminaRestore);
    }

    public void MedsTaken(MedicalItem meds)
    {
        int healthRestore = meds.HealthFill;
        int zombificationRestore = meds.ZombificationHeal;

        Health = AddToOrMax(MaxHealth, Health, healthRestore);
        Zombification = MinusOrMin(0, Zombification, zombificationRestore);
    }

    public void Damaged(int healthDamage, int zombificationDamage)
    {
        Health = MinusOrMin(0, Health, healthDamage);
        Zombification = AddToOrMax(MaxZombification, Zombification, zombificationDamage);
    }

    public void TimePassedHungrify()
    {
        //Minus one hunger per time passed
        Hunger = MinusOrMin(0, Hunger, HungerConsumedPerTimePass);
    }

    public void TimePassedStarvify()
    {
        //When hunger 0, minus one health per time passed
        Health = MinusOrMin(0, Health, 1);
    }

    public bool TryConsumeStamina(int stamina)
    {
        if (Stamina >= stamina)
        {
            Stamina = MinusOrMin(0, Stamina, stamina);
            return true;
        }
        // Not used if not enough stamina
        return false;
    }

    private bool TryConsumeHungerForStamina(int hunger)
    {
        if (Hunger > hunger)
        {
            Hunger = MinusOrMin(0, Hunger, hunger);
            return true;
        }
        //Not used if not enough hunger
        return false;
    }

    public bool RequireRestoreStamina()
    {
        return Stamina != MaxStamina;
    }

    public void RestoreStaminaFromFood(int stamina)
    {
        Stamina = AddToOrMax(MaxStamina, Stamina, stamina);
    }

    public void TimePassedTryConvertHungerToStamina()
    {
        //TODO require conversion rate
        if (!RequireRestoreStamina())
        {
            return;
        }
        int hungerUse = HungerUsedToRestoreStaminaPerTimePass;
        int staminaGain = StaminaRestoredFromFoodPerTimePass;
        if (TryConsumeHungerForStamina(hungerUse))
        {
            RestoreStaminaFromFood(staminaGain);
        }
        
    }

    private int AddToOrMax(int max, int current, int add)
    {
        int total = current + add;
        if (total > max)
        {
            total = max;
        }
        return total;
    }
    private int MinusOrMin(int min, int current, int minus)
    {
        int total = current - minus;
        if (total < min)
        {
            total = min;
        }
        return total;
    }


    #endregion

    private void StatValueUpdated(int index, int value)
    {
        UIStats.SetSliderValue(index, value);
        Save.Instance.SaveRequest();
    }
    private void StatMaxUpdated(int index, int max)
    {
        UIStats.SetSliderMax(index, max);
        Save.Instance.SaveRequest();
    }



    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        lastLat = double.MinValue;
        lastLon = double.MinValue;
    }

    private void Start()
    {
        StartInit();
        StatInit();
    }

    public void PlayerDied()
    {
        Debug.Log("PLAYER DIED!!");
        //TODO new game
        Health = MaxHealth;
    }


    public void SwitchToCombatMode()
    {
        InCombatMode = true;
        //ThisTR.SetParent(null);
        ModelObject.SetActive(false);
        POIManager.Instance.ActivateCombatMode(true);
    }
    public void SwitchToMapMode()
    {
        InCombatMode = false;
        ModelObject.SetActive(true);
        POIManager.Instance.ActivateCombatMode(false);

    }


    //Unity Event Referenced
    public void ReceiveJoystickMovement(Vector2 movementDelta)
    {
        ReportMovement(movementDelta);
    }

    private void WASDUpdate()
    {
        Vector2 keyboardMovement = Vector2.zero;
        if (Input.GetKey(KeyCode.W))
        {
            keyboardMovement.y += 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            keyboardMovement.y += -1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            keyboardMovement.x += -1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            keyboardMovement.x += 1;
        }
        if (keyboardMovement.x != 0 || keyboardMovement.y != 0)
        {
            usedWASD = true;
            keyboardMovement.Normalize();
            G.MovementJoystick.ExternalJoystickControl(keyboardMovement);
            ReportMovement(keyboardMovement);
        }
        else
        {
            if (usedWASD)
            {
                G.MovementJoystick.ExternalJoystickControl();
                usedWASD = false;
            }
        }
    }

    private void ReportMovement(Vector2 movementDelta)
    {
        if (InCombatMode)
        {
            CombatPlayer.Move(movementDelta);
            return;
        }
        G.Location.PlayerReportMovment(movementDelta);
    }

    private void StatInit()
    {
        HungerConsumeTimePassCount = HungerConsumeTimePass;
        HealthConsumeStarvingTimePassCount = HealthConsumeStarvingTimePass;
        StaminaRestoreFromFoodTimePassCount = StaminaRestoreFromFoodTimePass;
    }


    private void Update()
    {
        WASDUpdate();
        if (!InCombatMode)
        {
            Map_Update();
        }
        Stats_Update();
    }

    private void Stats_Update()
    {
        // Get Hungry
        HungerConsumeTimePassCount -= Time.deltaTime;
        if (HungerConsumeTimePassCount < 0)
        {
            HungerConsumeTimePassCount = HungerConsumeTimePass;
            TimePassedHungrify();
        }

        // Starve
        if (Hunger == 0)
        {
            HealthConsumeStarvingTimePassCount -= Time.deltaTime;
            if (HealthConsumeStarvingTimePassCount < 0)
            {
                HealthConsumeStarvingTimePassCount = HealthConsumeStarvingTimePass;
                TimePassedStarvify();
            }
        }

        // Restore Stamina from hunger/food
        StaminaRestoreFromFoodTimePassCount -= Time.deltaTime;
        if (StaminaRestoreFromFoodTimePassCount < 0)
        {
            StaminaRestoreFromFoodTimePassCount = StaminaRestoreFromFoodTimePass;
            TimePassedTryConvertHungerToStamina();
        }

        if (Health == 0)
        {
            PlayerDied();
        }


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




    public void SelectPOIOrZombie(Vector2 screenPosition)
    {
        if (InCombatMode)
        {
            return;
        }
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
            SwitchToCombatMode();
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
        if (InCombatMode)
        {
            return;
        }
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

        data.PlayerMaxStats ??= new int[4];
        data.PlayerMaxStats[0] = MaxHealth;
        data.PlayerMaxStats[1] = MaxHunger;
        data.PlayerMaxStats[2] = MaxStamina;
        data.PlayerMaxStats[3] = MaxZombification;
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

        if (data.PlayerMaxStats == null)
        {
            MaxHealth = 200;
            MaxHunger = 200;
            MaxStamina = 200;
            MaxZombification = 200;
        }
        else
        {
            MaxHealth = data.PlayerMaxStats[0];
            MaxHunger = data.PlayerMaxStats[1];
            MaxStamina = data.PlayerMaxStats[2];
            MaxZombification = data.PlayerMaxStats[3];
        }

    }
}
