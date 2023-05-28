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
    public Transform FollowTR;
    public Transform LookAtTR;

    [SerializeField] private GameObject ModelObject;
    public Animator Animator;
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


    [System.NonSerialized] private bool isLeavingCombat = false;
    [System.NonSerialized] private float leavingCombatTimeCount;
    [SerializeField] private float leavingCombatTime;

    public WeaponItem UsingWeapon { get; private set; }


    [field: SerializeField] public bool InCombatMode { get; private set; }
    public CombatPlayer CombatPlayer => CombatPlayer.Instance;

    #region Stats

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

    public float StaminaUseWhileRunningTimePass;
    public int StaminaUseWhileRunningPerTimePass;
    private float StaminaUseWhileRunningTimePassCount;

    public int StaminaUsedToSwingWeapon;

    [System.NonSerialized] private int health;
    [System.NonSerialized] private int hunger;
    [System.NonSerialized] private int energy;
    [System.NonSerialized] private int zombification;
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
    public int Energy
    {
        get => energy;
        private set
        {
            energy = value;
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
        get => PlayerSkillSet.Health * 10;
    }
    public int MaxHunger
    {
        get => PlayerSkillSet.Hunger * 10;
    }
    public int MaxStamina
    {
        get => PlayerSkillSet.Energy * 10;
    }
    public int MaxZombification
    {
        get => PlayerSkillSet.Zombification * 10;
    }

    public void UpdatePlayerStats()
    {
        StatMaxUpdated(0, MaxHealth);
        StatMaxUpdated(1, MaxHunger);
        StatMaxUpdated(2, MaxStamina);
        StatMaxUpdated(3, MaxZombification);
        UISkills.Instance.SetSkillSet(PlayerSkillSet);
    }

    public void FoodEaten(FoodItem food)
    {
        int hungerRestore = food.HungerFill;
        int staminaRestore = food.StaminaRestore;

        Hunger = AddToOrMax(MaxHunger, Hunger, hungerRestore);
        Energy = AddToOrMax(MaxStamina, Energy, staminaRestore);
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
        if (Energy >= stamina)
        {
            Energy = MinusOrMin(0, Energy, stamina);
            return true;
        }
        // Not used if not enough stamina
        return false;
    }

    public bool TryConsumeStaminaToSwingWeapon()
    {
        return TryConsumeStamina(StaminaUsedToSwingWeapon);
    }

    //Calls every frame
    public bool TryConsumeStaminaToRun()
    {
        StaminaUseWhileRunningTimePassCount -= Time.deltaTime;
        if (StaminaUseWhileRunningTimePassCount < 0)
        {
            StaminaUseWhileRunningTimePassCount = StaminaUseWhileRunningTimePass;
            return TryConsumeStamina(StaminaUseWhileRunningPerTimePass);
        }
        return true;
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
        return Energy != MaxStamina;
    }

    public void RestoreStaminaFromFood(int stamina)
    {
        Energy = AddToOrMax(MaxStamina, Energy, stamina);
    }

    public void TimePassedTryConvertHungerToStamina()
    {
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


    #region Leveling & Skills

    public static readonly int[] ExperienceRequiredToAdvance = new int[]
    {
        100,
        250,
        500,
        900,
        1500,
        2350,
        3500,
        5000,
        6900,
        9250,
    };
    // Calculation Reference (Unoptimized)
    private static int ExpReq(int level)
    {
        const int baseExp = 100;
        int exp = baseExp;
        for (int i = 0; i <= level; i++)
        {
            exp += (int)(baseExp * i * 0.5f);
        }
        if (level > 0)
        {
            exp += ExpReq(level - 1);
        }
        return exp;
    }



    public int experienceGainPerZombie;

    [System.NonSerialized] private int previousLevel = int.MaxValue;
    private int level;
    public int Level
    {
        get => level;
        private set
        {
            level = value;
            LevelChanged();
        }
    }

    private int experience;
    public int Experience
    {
        get => experience;
        private set
        {
            experience = value;
            ExperienceChanged();
        }
    }
    private int skillPoints;
    public int SkillPoints
    {
        get => skillPoints;
        private set
        {
            skillPoints = value;
            SkillPointsChanged();
        }
    }


    public int MeleeDamage => PlayerSkillSet.MeleeDamage;
    public int RangedDamage => PlayerSkillSet.RangedDamage;




    public static readonly SkillSet BaseSkillSet = new()
    {
        Health = 10,
        Hunger = 10,
        Energy = 10,
        Zombification = 10,
        MeleeDamage = 4,
        RangedDamage = 4,
    };

    public SkillSet PlayerSkillSet { get; private set; }

    public void SkillIncreased(int index)
    {
        PlayerSkillSet.SetSkillValue(index, PlayerSkillSet.GetSkillValue(index) + 1);
        UpdatePlayerStats();

        SkillPoints--;
    }
    public void ZombieKilledGainExperience(CombatZombie zombie)
    {
        PreCheckLevelUp();
        Experience += experienceGainPerZombie;
    }


    private void ExperienceChanged()
    {

        int level = 1;
        for (int i = 0; i < ExperienceRequiredToAdvance.Length; i++)
        {
            if (experience >= ExperienceRequiredToAdvance[i])
            {
                level = i + 2;
            }
            else
            {
                break;
            }
        }
        if (Level != level)
        {
            Level = level;
        }

        int lvlIndex = Level - 1;
        if (lvlIndex >= ExperienceRequiredToAdvance.Length)
        {
            UIStats.Instance.SetExperience(Experience, null);
            UISkills.Instance.SetExperience(Experience, null);
        }
        else
        {
            UIStats.Instance.SetExperience(Experience, ExperienceRequiredToAdvance[Level - 1]);
            UISkills.Instance.SetExperience(Experience, ExperienceRequiredToAdvance[Level - 1]);
        }

        Save.Instance.SaveRequest();
    }
    private void LevelChanged()
    {
        UIStats.Instance.SetLevel(Level);
        UISkills.Instance.SetLevel(Level);


        if (previousLevel >= Level)
        {
            return;
        }
        LeveledUp();
    }
    private void SkillPointsChanged()
    {
        UISkills.Instance.SetSkillPoints(skillPoints);
        Save.Instance.SaveRequest();
    }
    private void LeveledUp()
    {
        Debug.Log($"LEVELED UP TO {Level}!");
        SkillPoints += 10;
    }

    private void PreCheckLevelUp()
    {
        previousLevel = Level;
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
        if (InCombatMode)
        {
            SwitchToCombatMode(null);
        }
        else
        {
            SwitchToMapMode();
        }
        G.MovementJoystick.InputAction += ReceiveJoystickMovement;
        G.AttackButton.InputAction += AttackButtonPressed;
        G.ScreenInput.ClickAction += SelectPOIOrZombie;
    }

    public void PlayerDied()
    {
        Debug.Log("PLAYER DIED!!");

        //New game
        Save.Instance.Delete();
        SwitchToMapMode();
        SwitchToMapScene();
        //Health = MaxHealth;
    }


    public void SwitchToCombatMode(MapZombie zombie)
    {
        if (zombie != null)
        {
            int chasingZombies = MapZombieManager.GetChasingZombiesCountWith(zombie, out int unawareZombies);
            G.chasingZombies = chasingZombies;
            G.unawareZombies = unawareZombies;
        }

        InCombatMode = true;
        //ThisTR.SetParent(null);
        ModelObject.SetActive(false);
        POIManager.Instance.ActivateCombatMode(true);
        G.GPSToggle.gameObject.SetActive(false);
        G.AttackButton.gameObject.SetActive(true);
        G.Location.EnterCombatActivateMovementJoystick();
        SwitchToCombatScene();
    }
    public void SwitchToMapMode()
    {
        isLeavingCombat = false;

        InCombatMode = false;
        ModelObject.SetActive(true);
        POIManager.Instance.ActivateCombatMode(false);
        G.GPSToggle.gameObject.SetActive(true);
        G.AttackButton.gameObject.SetActive(false);


        lastMapZombieCell = new(int.MinValue, int.MinValue);
        lastLat = double.MinValue;
        lastLon = double.MinValue;

    }

    public void EquipWeapon(WeaponItem weaponItem)
    {
        UsingWeapon = weaponItem;
        if (InCombatMode)
        {
            CombatPlayer.WeaponChanged();
        }
    }


    private void ReceiveJoystickMovement(Vector2 movementDelta)
    {
        ReportMovement(movementDelta);
    }

    private void AttackButtonPressed()
    {
        if (!InCombatMode)
        {
            return;
        }
        CombatPlayer.AttackButtonPressed();
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
            if (CombatPlayer == null)
            {
                return;
            }
            CombatPlayer.Move(movementDelta);
            return;
        }
        G.Location.PlayerReportMovement(movementDelta);
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
        else
        {
            Combat_Update();
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
            Animator.SetFloat("MoveSpeed", 0);
            return;
        }
        float dist = new Vector2((float)(lastLat - Location.X), (float)(lastLon - Location.Y)).magnitude / 1e-6f;
        if (dist != Mathf.Infinity)
        {
            Animator.SetFloat("MoveSpeed", 1);
        }
        G.Coords.SetText("COORDS: " + Location);
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




    private void SelectPOIOrZombie(Vector2 screenPosition)
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
            SwitchToCombatMode(zombie);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out MapZombie zombie))
        {
            SwitchToCombatMode(zombie);
        }
    }

    private void SwitchToCombatScene()
    {
        SceneManager.LoadScene(1);
    }

    private void SwitchToMapScene()
    {
        SceneManager.LoadScene(0);
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

    public void ExitReachedInCombat()
    {
        SwitchToMapMode();
        SwitchToMapScene();
        GiveRewardsForZombiesKilled();
    }

    public void NoMoreZombiesLeaveCombat()
    {
        isLeavingCombat = true;
        leavingCombatTimeCount = leavingCombatTime;
    }

    private void GiveRewardsForZombiesKilled()
    {
        int zombiesKilled = CombatZombieManager.Instance.zombiesKilled;
        // TODO add rewards
        CombatReward.Instance.ActivateRewards(zombiesKilled);
    }

    private void Combat_Update()
    {
        if (!isLeavingCombat)
        {
            return;
        }
        leavingCombatTimeCount -= Time.deltaTime;
        if (leavingCombatTimeCount < 0)
        {
            isLeavingCombat = false;
            GiveRewardsForZombiesKilled();

            SwitchToMapMode();
            SwitchToMapScene();
        }
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
        data.PlayerStats[2] = Energy;
        data.PlayerStats[3] = Zombification;

        data.PlayerExperiencePoints = Experience;
        data.PlayerSkillPoints = SkillPoints;

        data.PlayerSkillSet = PlayerSkillSet;
    }

    public void LoadData(Save.Data data)
    {
        if (data.PlayerStats == null)
        {
            Health = 100;
            Hunger = 100;
            Energy = 100;
            Zombification = 0;
        }
        else
        {
            Health = data.PlayerStats[0];
            Hunger = data.PlayerStats[1];
            Energy = data.PlayerStats[2];
            Zombification = data.PlayerStats[3];
        }

        Experience = data.PlayerExperiencePoints;
        SkillPoints = data.PlayerSkillPoints;

        if (data.PlayerSkillSet == null)
        {
            PlayerSkillSet = new(BaseSkillSet);
        }
        else
        {
            PlayerSkillSet = data.PlayerSkillSet;
        }

        UpdatePlayerStats();
    }
}
