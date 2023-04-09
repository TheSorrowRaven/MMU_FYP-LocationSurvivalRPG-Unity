using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem.UI;
using Unity.VisualScripting;
using UnityEngine.UI;
using System;

public class UIPOI : MonoBehaviour, Save.ISaver
{
    private class LootComparer : IComparer<ItemAmt>
    {
        public static LootComparer Shared = new();

        public int Compare(ItemAmt x, ItemAmt y)
        {
            int val = y.item.Rarity.CompareTo(x.item.Rarity);
            if (val != 0)
            {
                return val;
            }
            val = x.GetType().FullName.CompareTo(y.GetType().FullName);
            if (val != 0)
            {
                return val;
            }
            return y.amt.CompareTo(x.amt);
        }
    }

    private static UIPOI instance;
    public static UIPOI Instance => instance;

    private static GameSettings GameSettings => GameSettings.Instance;
    private static GGoogleMapsService GGoogleMapsService => GGoogleMapsService.Instance;
    private static POIManager POIManager => POIManager.Instance;
    private static ItemManager ItemManager => ItemManager.Instance;

    [SerializeField] private GameObject UIPOIObject;

    [SerializeField] private RawImage POIPhotoRawImage;
    [SerializeField] private RectTransform POIPhotoRT;
    [SerializeField] private Texture PhotoNotFoundTexture;
    [SerializeField] private Texture PhotoLoadingTexture;

    [SerializeField] private TextMeshProUGUI POIName;
    [SerializeField] private TextMeshProUGUI POITypes;

    [SerializeField] private Transform UILootContainer;
    [SerializeField] private GameObject UILootPrefab;

    [SerializeField] private Slider ZombieEncounterSlider;

    [NonSerialized] private GGoogleMapsPOI gPOI;
    [NonSerialized] private Dictionary<string, int> remainingItems;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        UIPOIObject.SetActive(false);
        StartInit();
    }

    public void StartInit()
    {
        Save.Instance.InitSaver(this);
    }

    private void Update()
    {
        LootUpdate();
    }

    public void SetGPOI(GGoogleMapsPOI gPOI)
    {
        this.gPOI = gPOI;

        POIName.SetText(gPOI.Name);
        POITypes.SetText(BuildTypeTextFromGPOI(gPOI));
        RequestGPOIImage(gPOI);

        POIManager.TryGetVisitedPOI(gPOI.PlaceID, out remainingItems);

        UIPOIObject.SetActive(true);

        CalculateLoot();
    }

    private string BuildTypeTextFromGPOI(GGoogleMapsPOI gPOI)
    {
        List<string> types = gPOI.Types;
        if (types.Count == 0)
        {
            return "";
        }
        string text = FirstCharacterToUpper(types[0]);
        for (int i = 1; i < types.Count; i++)
        {
            string typeName = types[i];
            if (typeName.Length == 0)
            {
                continue;
            }
            text += ", " + FirstCharacterToUpper(typeName);
        }
        return text;
    }

    private string FirstCharacterToUpper(string text)
    {
        if (text == null)
        {
            return null;
        }
        if (text.Length == 0)
        {
            return text;
        }
        return char.ToUpper(text[0]) + text[1..];
    }

    public void RequestGPOIImage(GGoogleMapsPOI gPOI)
    {
        List<GGoogleMapsPOI.Photo> photos = gPOI.Photos;
        if (photos == null || photos.Count == 0)
        {
            SetPhotoUnkown();
            return;
        }
        SetPhotoIsLoading();
        GGoogleMapsService.StartPlacePhotosQuery(photos[0].PhotoReference, (int)POIPhotoRT.rect.size.x, (int)POIPhotoRT.rect.size.y, PhotoFetched, SetPhotoUnkown);
    }

    private void PhotoFetched(Texture texture)
    {
        POIPhotoRawImage.texture = texture;
        CorrectPhotoUV();
    }
    private void SetPhotoUnkown()
    {
        POIPhotoRawImage.texture = PhotoNotFoundTexture;
        ResetPhotoUV();
    }
    private void SetPhotoIsLoading()
    {
        POIPhotoRawImage.texture = PhotoLoadingTexture;
        ResetPhotoUV();
    }

    private void ResetPhotoUV()
    {
        POIPhotoRawImage.uvRect = new(0, 0, 1, 1);
    }

    [ContextMenu("Correct Photo UV")]
    public void CorrectPhotoUV()
    {
        Texture texture = POIPhotoRawImage.texture;
        Vector2 rawImageSize = POIPhotoRT.rect.size;
        float rawImageAR = rawImageSize.x / rawImageSize.y;
        float photoAR = (float)texture.width / texture.height;
        Rect uvRect;
        if (photoAR > rawImageAR)
        {
            // Image is wider than the rectangle, so it will be cropped horizontally
            float uvWidth = rawImageAR / photoAR;
            float uvX = (1 - uvWidth) / 2;

            // Set the UV rect
            uvRect = new Rect(uvX, 0, uvWidth, 1);
        }
        else if (photoAR < rawImageAR)
        {
            // Image is taller than the rectangle, so it will be cropped vertically
            float uvHeight = photoAR / rawImageAR;
            float uvY = (1 - uvHeight) / 2;

            // Set the UV rect
            uvRect = new Rect(0, uvY, 1, uvHeight);
        }
        else
        {
            // Image and rectangle have the same aspect ratio, so no cropping is needed
            uvRect = new Rect(0, 0, 1, 1);
        }
        POIPhotoRawImage.uvRect = uvRect;
    }



    #region Loot Calculation

    private System.Random LootRNG;
    private readonly Dictionary<Item, int> LootCount = new();
    private readonly List<ItemAmt> ItemAmtList = new();
    private readonly List<UIItem> UILootList = new();

    private void AddToLoot(Item item, int amt = 1)
    {
        if (amt < 1)
        {
            return;
        }
        if (LootCount.TryGetValue(item, out int count))
        {
            LootCount[item] = count + amt;
            return;
        }
        LootCount.Add(item, amt);
    }

    public void CalculateLoot()
    {
        isLootingAll = false;

        int seed = CreateNumberFromDate(DateTime.Now) + gPOI.PlaceID.GetHashCode();
        LootRNG = new(seed);
        ClearLoot();
        CalculateLootFromTypes();

        SpawnLoot();
    }

    public void ClearLoot()
    {
        LootCount.Clear();
        ItemAmtList.Clear();
        for (int i = 0; i < UILootList.Count; i++)
        {
            Destroy(UILootList[i]);
        }
        UILootList.Clear();
        for (int i = 0; i < UILootContainer.childCount; i++)
        {
            Destroy(UILootContainer.GetChild(i).gameObject);
        }
    }

    public void UILootRemoved(UIItem uiLoot)
    {
        UILootList.Remove(uiLoot);
        Destroy(uiLoot.gameObject);
    }

    public void UILootUpdate()
    {
        LootListUpdated();
    }

    private static int CreateNumberFromDate(DateTime dt)
    {
        string str = dt.Year.ToString() + (dt.Month < 10 ? ("0" + dt.Month) : dt.Month) + (dt.Day < 10 ? ("0" + dt.Day) : dt.Day);
        return int.Parse(str);
    }

    private void CalculateLootFromTypes()
    {
        if (remainingItems != null)
        {
            foreach (var pair in remainingItems)
            {
                Item item = ItemManager.IdentifierToItem[pair.Key];
                AddToLoot(item, pair.Value);
            }
            return;
        }

        List<string> types = gPOI.Types;
        if (types.Count == 0)
        {
            return;
        }
        for (int i = 0; i < types.Count; i++)
        {
            string type = types[i];
            if (POIManager.TryGetPOITypeDefinition(type, out POITypeDefinition definition))
            {
                CalculateLootFromFood(definition.Food);
                CalculateLootFromMedical(definition.Medical);
                CalculateLootFromWeapon(definition.Weapon);
            }
        }

    }

    private void CalculateLootFromFood(int food)
    {
        if (food == 0)
        {
            return;
        }

        for (int i = 0; i < food; i++)
        {
            double rarityChance = LootRNG.NextDouble();
            Rarity rarity = ItemManager.GetRarityFromChance(rarityChance);
            FoodItem item = ItemManager.GetFoodFromRarity(rarity);
            AddToLoot(item);
        }
    }

    private void CalculateLootFromMedical(int medical)
    {
        if (medical == 0)
        {
            return;
        }

        for (int i = 0; i < medical; i++)
        {
            double rarityChance = LootRNG.NextDouble();
            Rarity rarity = ItemManager.GetRarityFromChance(rarityChance);
            MedicalItem item = ItemManager.GetMedicalFromRarity(rarity);
            AddToLoot(item);
        }
    }

    private void CalculateLootFromWeapon(int weapon)
    {
        if (weapon == 0)
        {
            return;
        }

        for (int i = 0; i < weapon; i++)
        {
            double rarityChance = LootRNG.NextDouble();
            Rarity rarity = ItemManager.GetRarityFromChance(rarityChance);
            WeaponItem item = ItemManager.GetWeaponFromRarity(rarity);
            AddToLoot(item);
        }
    }

    private void SpawnLoot()
    {
        foreach (var pair in LootCount)
        {
            ItemAmt itemAmt = new(pair.Key, pair.Value);
            ItemAmtList.Add(itemAmt);
        }

        ItemAmtList.Sort(LootComparer.Shared);

        for (int i = 0; i < ItemAmtList.Count; i++)
        {
            UIItem uiLoot = Instantiate(UILootPrefab, UILootContainer).GetComponent<UIItem>();
            uiLoot.SetLootable(ItemAmtList[i]);
            UILootList.Add(uiLoot);
        }
    }

    private bool isLootingAll = false;
    private float lootingAllTimeCount;

    public void ButtonClickedLootAll()
    {
        if (isLootingAll)
        {
            return;
        }
        isLootingAll = true;
        lootingAllTimeCount = GameSettings.LootAllTime;
    }

    private void LootUpdate()
    {
        if (!isLootingAll)
        {
            return;
        }

        lootingAllTimeCount -= Time.deltaTime;
        if (lootingAllTimeCount > 0)
        {
            return;
        }
        lootingAllTimeCount = GameSettings.LootAllTime;

        bool finishedLooting = true;
        for (int i = 0; i < UILootList.Count; i++)
        {
            if (UILootList[i].itemAmt.amt == 0)
            {
                continue;
            }
            finishedLooting = false;
            UILootList[i].LootOne();
            break;
        }

        if (finishedLooting)
        {
            isLootingAll = false;
        }
    }

    private void LootListUpdated()
    {
        POIManager.UpdateVisitedPOIs(gPOI.PlaceID, GetRemainingItems());
        Save.Instance.SaveRequest();
    }

    private Dictionary<string, int> GetRemainingItems()
    {
        Dictionary<string, int> remainingItems = new();
        for (int i = 0; i < UILootList.Count; i++)
        {
            ItemAmt itemAmt = UILootList[i].itemAmt;
            if (itemAmt.amt == 0)
            {
                continue;
            }
            remainingItems.Add(itemAmt.item.Identifier, itemAmt.amt);
        }
        return remainingItems;
    }

    #endregion

    #region Zombie Encounter

    public void IncreaseZombieEncounter(Item item)
    {
        ZombieEncounterSlider.value += Mathf.Pow(2, ((int)item.Rarity + 1));
        if (ZombieEncounterSlider.value >= ZombieEncounterSlider.maxValue)
        {
            EncounterZombie();
            ZombieEncounterSlider.value = 0;
        }
        Save.Instance.SaveRequest();
    }

    private void EncounterZombie()
    {
        Player.Instance.SwitchToCombatMode(MapZombieManager.Instance.GetRandomZombie());
    }

    #endregion


    public void ButtonClickExitUIPOI()
    {
        isLootingAll = false;
        UIPOIObject.SetActive(false);
    }

    public void SaveData(Save.Data data)
    {
        data.ZombieEncounter = (int)ZombieEncounterSlider.value;
    }

    public void LoadData(Save.Data data)
    {
        ZombieEncounterSlider.value = data.ZombieEncounter;
    }
}
