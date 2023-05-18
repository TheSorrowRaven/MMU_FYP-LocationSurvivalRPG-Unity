using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatReward : MonoBehaviour
{
    private static ItemManager ItemManager => ItemManager.Instance;
    private static POIManager POIManager => POIManager.Instance;


    [SerializeField] private Transform UILootContainer;
    [SerializeField] private GameObject UILootPrefab;

    private System.Random LootRNG;
    private readonly Dictionary<Item, int> LootCount = new();
    private readonly List<ItemAmt> ItemAmtList = new();
    private readonly List<UIItem> UILootList = new();

    private void Awake()
    {
        LootRNG = new System.Random();
    }

    public void CalculateLoot(int amount)
    {
        //TODO call calculate loot
        CalculateRandomLoot(amount);
        SpawnRewards();
    }

    public void Return()
    {
        ClearLoot();
        // TODO close UI
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

    private void CalculateRandomLoot(int any)
    {
        if (any == 0)
        {
            return;
        }

        for (int i = 0; i < any; i++)
        {
            double rarityChance = LootRNG.NextDouble();
            Rarity rarity = ItemManager.GetRarityFromChance(rarityChance);
            double lootType = LootRNG.NextDouble();
            Item item;
            if (lootType < 0.6)
            {
                item = ItemManager.GetFoodFromRarity(rarity);
            }
            else if (lootType < 0.95)
            {
                item = ItemManager.GetMedicalFromRarity(rarity);
            }
            else
            {
                item = ItemManager.GetAmmoFromRarity(rarity);
            }
            AddToLoot(item);
        }

    }

    private void SpawnRewards()
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
            uiLoot.SetNone(ItemAmtList[i]);
            UILootList.Add(uiLoot);
        }
    }

}
