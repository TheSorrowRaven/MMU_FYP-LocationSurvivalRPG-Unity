using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ItemManager;

public class ItemManager : MonoBehaviour
{
    [System.Serializable]
    public class RarityClass<T>
    {
        public T common;
        public T uncommon;
        public T rare;
        public T epic;
        public T GetFromRarity(Rarity rarity)
        {
            return rarity switch
            {
                Rarity.Common => common,
                Rarity.Uncommon => uncommon,
                Rarity.Rare => rare,
                _ => epic,
            };
        }
    }

    private static ItemManager instance;
    public static ItemManager Instance => instance;

    public Sprite CommonSprite;
    public Sprite UncommonSprite;
    public Sprite RareSprite;
    public Sprite EpicSprite;
    public double CommonChance;
    public double UncommonChance;
    public double RareChance;
    public double EpicChance;

    public List<Item> Items = new();
    public readonly Dictionary<Rarity, Sprite> RarityToBackgroundSprite = new();
    public readonly Dictionary<string, Item> IdentifierToItem = new();

    public RarityClass<FoodItem> FoodClass;
    public RarityClass<MedicalItem> MedicalClass;


    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        Setup();
    }

#if UNITY_EDITOR

    [ContextMenu("Get All Items")]
    public void GetAllItems()
    {
        Items.Clear();
        Item[] AllItems = G.GetAllScriptableObjects<Item>();
        for (int i = 0; i < AllItems.Length; i++)
        {
            Items.Add(AllItems[i]);
        }
    }

#endif

    private void Setup()
    {
        RarityToBackgroundSprite.Clear();
        RarityToBackgroundSprite.Add(Rarity.Common, CommonSprite);
        RarityToBackgroundSprite.Add(Rarity.Uncommon, UncommonSprite);
        RarityToBackgroundSprite.Add(Rarity.Rare, RareSprite);
        RarityToBackgroundSprite.Add(Rarity.Epic, EpicSprite);

        IdentifierToItem.Clear();
        for (int i = 0; i < Items.Count; i++)
        {
            IdentifierToItem.Add(Items[i].Identifier, Items[i]);
        }
    }

    public Rarity GetRarityFromChance(double rarity)
    {
        if (rarity <= CommonChance)
        {
            return Rarity.Common;
        }
        if (rarity <= UncommonChance)
        {
            return Rarity.Uncommon;
        }
        if (rarity <= RareChance)
        {
            return Rarity.Rare;
        }
        return Rarity.Epic;
    }

    public FoodItem GetFoodFromRarity(Rarity rarity)
    {
        return FoodClass.GetFromRarity(rarity);
    }
    public MedicalItem GetMedicalFromRarity(Rarity rarity)
    {
        return MedicalClass.GetFromRarity(rarity);
    }

    public WeaponItem GetWeaponFromRarity(Rarity rarity)
    {
        //TODO temporary
        return (WeaponItem)IdentifierToItem["weapon_sledgehammer"];
    }

}
