using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CombatReward : MonoBehaviour
{
    private static CombatReward instance;
    public static CombatReward Instance => instance;



    private static ItemManager ItemManager => ItemManager.Instance;

    [SerializeField] private Transform UIRewardContainer;
    [SerializeField] private GameObject UIRewardPrefab;
    [SerializeField] private TextMeshProUGUI KillsText;

    private System.Random RewardRNG;
    private readonly Dictionary<Item, int> RewardCount = new();
    private readonly List<ItemAmt> ItemAmtList = new();
    private readonly List<UIItem> RewardList = new();

    private void Awake()
    {
        RewardRNG = new System.Random();
    }

    private void Start()
    {
        if (instance == null)
        {
            gameObject.SetActive(false);
            instance = this;
        }
    }

    public void ActivateRewards(int amount)
    {
        CalculateRandomLoot(amount);
        SpawnRewards();
        KillsText.SetText($"Kills: {amount}");
        gameObject.SetActive(true);
    }

    public void Return()
    {
        ClearLoot();
        gameObject.SetActive(false);
    }

    public void ClearLoot()
    {
        RewardCount.Clear();
        ItemAmtList.Clear();
        for (int i = 0; i < RewardList.Count; i++)
        {
            Destroy(RewardList[i]);
        }
        RewardList.Clear();
        for (int i = 0; i < UIRewardContainer.childCount; i++)
        {
            Destroy(UIRewardContainer.GetChild(i).gameObject);
        }
    }

    private void AddToLoot(Item item, int amt = 1)
    {
        if (amt < 1)
        {
            return;
        }
        if (RewardCount.TryGetValue(item, out int count))
        {
            RewardCount[item] = count + amt;
            return;
        }
        RewardCount.Add(item, amt);
    }

    private void CalculateRandomLoot(int any)
    {
        if (any == 0)
        {
            return;
        }

        for (int i = 0; i < any; i++)
        {
            double rarityChance = RewardRNG.NextDouble();
            Rarity rarity = ItemManager.GetRarityFromChance(rarityChance);
            double lootType = RewardRNG.NextDouble();
            Item item;
            if (lootType < 0.5)
            {
                item = ItemManager.GetFoodFromRarity(rarity);
            }
            else if (lootType < 0.85)
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
        foreach (var pair in RewardCount)
        {
            ItemAmt itemAmt = new(pair.Key, pair.Value);
            ItemAmtList.Add(itemAmt);
        }

        ItemAmtList.Sort(LootComparer.Shared);

        for (int i = 0; i < ItemAmtList.Count; i++)
        {
            UIItem uiLoot = Instantiate(UIRewardPrefab, UIRewardContainer).GetComponent<UIItem>();
            uiLoot.SetNone(ItemAmtList[i]);
            uiLoot.LootAllAsReward();
            RewardList.Add(uiLoot);
        }
    }

}
